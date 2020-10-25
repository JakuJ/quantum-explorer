import * as monaco from 'monaco-editor/esm/vs/editor/editor.api.js';
import {loadWASM} from 'onigasm';
import {Registry} from 'monaco-textmate';
import {wireTmGrammars} from 'monaco-editor-textmate';
import * as path from 'path';
import {saveCode, loadCode} from './storage';

const LIGHT_THEME_NAME = 'vs-code-custom-light-theme';
const DARK_THEME_NAME = 'vs-code-custom-dark-theme';

const SYNTAX_FILES_FOLDER = 'syntaxFiles';

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

    operation RandomBit () : Result {
        using (q = Qubit()) {
            H(q);
            return MResetZ(q);
        }
    }
}`;

export class Editor {

  static async InitializeEditor(element) {
    element.innerHTML = '';

    window.editorsDict = window.editorsDict || {};
    window.editorsCounter = window.editorsCounter || 0;

    const id = 'id' + (window.editorsCounter++);

    monaco.languages.register({
      id: 'qsharp',
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

    const grammars = new Map([['qsharp', 'source.qsharp']]);

    monaco.editor.defineTheme(LIGHT_THEME_NAME,
      await fetch(LIGHT_THEME_JSON).then(x => x.json())
    );

    monaco.editor.defineTheme(DARK_THEME_NAME,
      await fetch(DARK_THEME_JSON).then(x => x.json())
    );

    window.editorsDict[id] = monaco.editor.create(element, {
      value: loadCode() || INIT_CODE,
      language: 'qsharp',
      theme: LIGHT_THEME_NAME,
      minimap: {
        enabled: false
      },
      scrollbar: {
        vertical: 'hidden',
        horizontal: 'auto'
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

