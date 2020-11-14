import * as monaco from 'monaco-editor/esm/vs/editor/editor.api.js';
import {loadWASM} from 'onigasm';
import {Registry} from 'monaco-textmate';
import {wireTmGrammars} from 'monaco-editor-textmate';
import * as path from 'path';
import {saveCode, loadCode} from './storage';
import {listen, MessageConnection} from 'vscode-ws-jsonrpc';
import {
  MonacoLanguageClient, CloseAction, ErrorAction,
  MonacoServices, createConnection
} from 'monaco-languageclient';
import ReconnectingWebSocket from 'reconnecting-websocket';

const normalizeUrl = require('normalize-url');

const LIGHT_THEME_NAME = 'vs-code-custom-light-theme';
const DARK_THEME_NAME = 'vs-code-custom-dark-theme';

const SYNTAX_FILES_FOLDER = 'syntaxFiles';
const LANGUAGE_ID = 'qsharp';
const MONACO_URI = monaco.Uri.parse('file:///tmp/Bell.qs');

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
    element.innerHTML = '';

    window.editorsDict = window.editorsDict || {};
    window.editorsCounter = window.editorsCounter || 0;

    const id = 'id' + (window.editorsCounter++);

    monaco.languages.register({
      id: LANGUAGE_ID,
      extensions: ['qs'],
      aliases: ['Q#', 'qsharp']
    });

    await loadWASM(ONIGASM_FILE);

    const registry = new Registry({
      getGrammarDefinition: async () => ({
        format: 'json',
        content: await fetch(TM_LANGUAGE).then(x => x.text())
      })
    });

    const grammars = new Map([[LANGUAGE_ID, 'source.qsharp']]);

    monaco.editor.defineTheme(LIGHT_THEME_NAME,
      await fetch(LIGHT_THEME_JSON).then(x => x.json())
    );

    monaco.editor.defineTheme(DARK_THEME_NAME,
      await fetch(DARK_THEME_JSON).then(x => x.json())
    );

    window.editorsDict[id] = monaco.editor.create(element, {
      model: monaco.editor.createModel(loadCode() || INIT_CODE, LANGUAGE_ID, MONACO_URI),
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
        enabled: true
      }
    });

    MonacoServices.install(window.editorsDict[id]);

    // create the web socket
    const url = createUrl('/monaco-editor');
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

    await wireTmGrammars(monaco, registry, grammars, window.editorsDict[id]);

    new ResizeObserver(() => window.editorsDict[id].layout()).observe(element);

    window.editorsDict[id].addAction({
      id: 'change-custom-theme',
      label: 'Switch Light/Dark Theme',
      precondition: null,
      keybindingContext: null,
      contextMenuGroupId: 'editorOptions',
      contextMenuOrder: 0,
      run: ed => {
        const currTheme = ed._themeService.getTheme().themeName;
        if (currTheme === LIGHT_THEME_NAME) {
          monaco.editor.setTheme(DARK_THEME_NAME);
        } else {
          monaco.editor.setTheme(LIGHT_THEME_NAME);
        }
      }
    });

    window.editorsDict[id].addCommand(monaco.KeyMod.CtrlCmd | monaco.KeyCode.KEY_S, () => {
      saveCode(window.editorsDict[id].getValue());
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
  const port = location.protocol === 'https:' ? '5001' : '5000';
  return normalizeUrl(
    `${protocol}://${location.hostname}:${port}${location.pathname}${path}`,
  );
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
    },
    // create a language client connection from the JSON RPC connection on demand
    connectionProvider: {
      async get() {
        return createConnection(connection, console.error, console.warn);
      },
    },
  });
}
