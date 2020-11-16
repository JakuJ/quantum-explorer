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

const LIGHT_THEME_NAME = 'vs-code-custom-light-theme';
const DARK_THEME_NAME = 'vs-code-custom-dark-theme';

const SYNTAX_FILES_FOLDER = 'syntaxFiles';
const LANGUAGE_ID = 'qsharp';
const WEBSOCKET_PORT = '8091';

const UUID = uuidv4();
const WORKSPACE_NAME = `${UUID}-workspace`;
const WORKSPACE_URI = monaco.Uri.parse(`file:///tmp/qsharp/${WORKSPACE_NAME}`);
const FILE_URI = monaco.Uri.parse(`file:///tmp/qsharp/${WORKSPACE_NAME}/_content_.qs`);

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
].map(x => path.join(SYNTAX_FILES_FOLDER, x));

const INIT_CODE = `namespace HelloWorld {

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

export class Editor {

  static async InitializeEditor(element) {
    await loadWASM(ONIGASM_FILE);

    element.innerHTML = '';

    window.editorsDict = window.editorsDict || {};
    window.editorsCounter = window.editorsCounter || 0;

    const id = 'id' + (window.editorsCounter++);

    monaco.languages.register({
      id: LANGUAGE_ID,
      extensions: ['qs'],
      aliases: ['Q#', 'qsharp']
    });

    const registry = new Registry({
      getGrammarDefinition: async () => ({
        format: 'json',
        content: await fetch(TM_LANGUAGE).then(x => x.text())
      })
    });

    monaco.editor.defineTheme(LIGHT_THEME_NAME,
      await fetch(LIGHT_THEME_JSON).then(x => x.json())
    );

    monaco.editor.defineTheme(DARK_THEME_NAME,
      await fetch(DARK_THEME_JSON).then(x => x.json())
    );

    window.editorsDict[id] = monaco.editor.create(element, {
      model: monaco.editor.createModel(loadCode() || INIT_CODE, LANGUAGE_ID, FILE_URI),
      theme: LIGHT_THEME_NAME,
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

    MonacoServices.install(window.editorsDict[id]);

    const grammars = new Map([[LANGUAGE_ID, 'source.qsharp']]);
    await wireTmGrammars(monaco, registry, grammars, window.editorsDict[id]);

    new ResizeObserver(() => window.editorsDict[id].layout()).observe(element);

    window.editorsDict[id].addAction({
      id: 'set-light-theme',
      label: 'Switch to Light Theme',
      precondition: null,
      keybindingContext: null,
      contextMenuGroupId: 'editorOptions',
      contextMenuOrder: 0,
      run: () => {
        monaco.editor.setTheme(LIGHT_THEME_NAME);
      }
    });

    window.editorsDict[id].addAction({
      id: 'set-dark-theme',
      label: 'Switch to Dark Theme',
      precondition: null,
      keybindingContext: null,
      contextMenuGroupId: 'editorOptions',
      contextMenuOrder: 1,
      run: () => {
        monaco.editor.setTheme(DARK_THEME_NAME);
      }
    });

    window.editorsDict[id].addCommand(monaco.KeyMod.CtrlCmd | monaco.KeyCode.KEY_S, () => {
      saveCode(window.editorsDict[id].getValue());
    });

    // create the web socket
    const url = createUrl('monaco-editor');
    const webSocket = createWebSocket(url);
    // listen when the web socket is opened
    listen({
      webSocket,
      onConnection: connection => {
        // create and start the language client
        const languageClient = createLanguageClient(connection);
        const disposable = languageClient.start();
        connection.onClose(() => disposable.dispose());
      }
    });

    return id;
  }

  static GetCode(id) {
    return window.editorsDict[id].getValue();
  }

  static SetCode(id, code) {
    window.editorsDict[id].setValue(code);
  }
}

function createUrl(path) {
  const protocol = location.protocol === 'https:' ? 'wss' : 'ws';
  const port = WEBSOCKET_PORT;
  return `${protocol}://${location.hostname}:${port}/${path}`;
}

function createWebSocket(url) {
  const socketOptions = {
    maxReconnectionDelay: 10000,
    minReconnectionDelay: 1000,
    reconnectionDelayGrowFactor: 1.3,
    connectionTimeout: 10000,
    maxRetries: Infinity,
    debug: false,
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
