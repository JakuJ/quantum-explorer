import * as monaco from 'monaco-editor/esm/vs/editor/editor.api.js';
import {loadWASM} from 'onigasm';
import {Registry} from 'monaco-textmate';
import {wireTmGrammars} from 'monaco-editor-textmate';
import * as path from 'path';
import {loadCode, saveCode} from './storage';
import {listen} from 'vscode-ws-jsonrpc';
import {CloseAction, createConnection, ErrorAction, MonacoLanguageClient, MonacoServices} from 'monaco-languageclient';
import ReconnectingWebSocket from 'reconnecting-websocket';
import {v4 as uuidv4} from 'uuid';

//#region Constants

// Websocket connection
const IS_PRODUCTION = process.env.NODE_ENV === 'production';
const PRODUCTION_HOST = 'qexplorer-ls.herokuapp.com';
const WEBSOCKET_ENDPOINT = 'monaco-editor';

// Custom editor theme identifiers
const LIGHT_THEME_NAME = 'vs-code-custom-light-theme';
const DARK_THEME_NAME = 'vs-code-custom-dark-theme';

// URIs for the language server
const UUID = uuidv4();
const WORKSPACE_NAME = `${UUID}-workspace`;
const WORKSPACE_URI = monaco.Uri.parse(`file://${process.env.TEMP_DIR}/qsharp/${WORKSPACE_NAME}`);
const FILE_URI = monaco.Uri.parse(`file://${process.env.TEMP_DIR}/qsharp/${WORKSPACE_NAME}/_content_.qs`);
const LANGUAGE_ID = 'qsharp';

// Paths to files in the syntax directory
const [
  ONIGASM_FILE,
  TM_LANGUAGE,
  LIGHT_THEME_JSON,
  DARK_THEME_JSON,
] = [
  'onigasm.wasm',
  'qsharp.tmLanguage.json',
  'lightTheme.json',
  'darkTheme.json',
].map(x => path.join('syntax', x));

// Default code for the editor
// TODO: When we have the database up and running, fetch it from there
const DEFAULT_CODE = `namespace HelloWorld {

    open Microsoft.Quantum.Intrinsic;
    open Microsoft.Quantum.Measurement;
    open Microsoft.Quantum.Canon;

    operation RandomBit(): Result {
        using (q = Qubit()) {
            H(q);
            return MResetZ(q);
        }
    }

    @EntryPoint()
    operation Hello(): Unit {
      let bit = RandomBit();
      Message($"A random bit: {bit}");
    }
}`;

//#endregion

let statusRef = null;

export class Editor {

  // Setup the Q# editor and establish a connection to the language server if possible
  static async InitializeEditor(element) {
    element.innerHTML = '';

    window.editorsDict = window.editorsDict || {};
    window.editorsCounter = window.editorsCounter || 0;

    const id = 'id' + (window.editorsCounter++);

    // preload onigasm
    await loadWASM(ONIGASM_FILE);

    // register the Q# language
    monaco.languages.register({
      id: LANGUAGE_ID,
      extensions: ['qs'],
      aliases: ['Q#', 'qsharp']
    });

    // set up the registry with our custom grammar
    const registry = new Registry({
      getGrammarDefinition: async () => ({
        format: 'json',
        content: await fetch(TM_LANGUAGE).then(x => x.text())
      })
    });

    // define light and dark themes
    monaco.editor.defineTheme(LIGHT_THEME_NAME,
      await fetch(LIGHT_THEME_JSON).then(x => x.json())
    );

    monaco.editor.defineTheme(DARK_THEME_NAME,
      await fetch(DARK_THEME_JSON).then(x => x.json())
    );

    // create the editor instance
    window.editorsDict[id] = monaco.editor.create(element, {
      model: monaco.editor.createModel(loadCode() || DEFAULT_CODE, LANGUAGE_ID, FILE_URI),
      theme: getThemeName(),
      minimap: {
        enabled: false
      },
      scrollbar: {
        vertical: 'hidden',
        horizontal: 'auto'
      },
      glyphMargin: true,
      lightbulb: {
        enabled: false
      },
      mouseWheelZoom: false,
      codeLens: false,
      foldingStrategy: 'indentation',
    });

    // install Monaco services required to communicate with the LS
    MonacoServices.install(window.editorsDict[id]);

    // wire TM grammars
    const grammars = new Map([[LANGUAGE_ID, 'source.qsharp']]);
    await wireTmGrammars(monaco, registry, grammars, window.editorsDict[id]);

    // enable dynamic layout changes
    new ResizeObserver(() => window.editorsDict[id].layout()).observe(element);

    // bind the save command to Ctrl+S
    window.editorsDict[id].addCommand(monaco.KeyMod.CtrlCmd | monaco.KeyCode.KEY_S, () => {
      saveCode(window.editorsDict[id].getValue());
    });

    // create the web socket
    const url = IS_PRODUCTION
      ? `wss://${PRODUCTION_HOST}/${WEBSOCKET_ENDPOINT}`
      : `ws://localhost:8091/${WEBSOCKET_ENDPOINT}`;
    const webSocket = createWebSocket(url);

    // listen when the web socket is opened
    listen({
      webSocket,
      onConnection: async connection => {
        // create and start the language client
        const languageClient = createLanguageClient(connection);
        const disposable = languageClient.start();
        const con = await languageClient.connectionProvider.get();

        // gracefully stop the client on page closed
        window.addEventListener('beforeunload', () => {
          languageClient.stop();
        });

        con.onLogMessage(async ({message}) => {
          // log detailed LS connection messages outside production
          if (!IS_PRODUCTION) {
            console.log(message);
          }

          let status = null;

          // change LS status based on received notifications
          switch (true) {
          case message.startsWith('Discovered Q# project'):
            status = 'Connecting';
            break;
          case message.startsWith('Done loading project'):
            status = 'Connected';
            break;
          case message.startsWith('Error on loading project'):
            // This happens from time to time with multiple clients connecting at once
            // The client retries the connection right away, so nothing has to be done
            status = 'Disconnected';
            break;
          case message.endsWith('Only syntactic diagnostics are generated.'):
            status = 'SyntaxOnly';
            break;
          default:
            return;
          }

          await statusRef.invokeMethodAsync('SetState', status);
        });

        // Dispose when the connection is closed by the server
        connection.onClose(async () => {
          disposable.dispose();
        });
      },
    });

    return id;
  }

  // Get the code that's currently in the editor
  static GetCode(id) {
    return window.editorsDict[id].getValue();
  }

  // Save the reference to the LS connection status component
  static SetStatusReference(ref) {
    statusRef = ref;
  }

  static setColorTheme() {
    monaco.editor.setTheme(getThemeName());
  }
}

function getThemeName() {
  return localStorage.getItem('theme') === 'dark' ? DARK_THEME_NAME : LIGHT_THEME_NAME;
}

function createWebSocket(url) {
  const socketOptions = {
    maxReconnectionDelay: 10000,
    minReconnectionDelay: 1000,
    reconnectionDelayGrowFactor: 1.3,
    connectionTimeout: 10000,
    maxRetries: Infinity,
    debug: !IS_PRODUCTION,
  };
  return new ReconnectingWebSocket(url, [], socketOptions);
}

function createLanguageClient(connection) {
  return new MonacoLanguageClient({
    name: 'Q# Language Client',
    clientOptions: {
      // use a language id as a document selector
      documentSelector: [LANGUAGE_ID],
      // disable the default error handler
      errorHandler: {
        error: () => ErrorAction.Continue,
        closed: () => CloseAction.DoNotRestart,
      },
      workspaceFolder: {
        uri: WORKSPACE_URI,
        name: WORKSPACE_NAME
      }
    },
    // create a language client connection from the JSON RPC connection on demand
    connectionProvider: {
      async get() {
        return createConnection(connection, console.error, console.warn);
      },
    },
  });
}
