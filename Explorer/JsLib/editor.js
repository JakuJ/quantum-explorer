import * as monaco from 'monaco-editor/esm/vs/editor/editor.api.js';
import { loadWASM } from 'onigasm'
import { Registry } from 'monaco-textmate'
import { wireTmGrammars } from 'monaco-editor-textmate'
import * as path from 'path'

const SYNTAX_FILES_FOLDER = 'syntaxFiles'

const [
    ONIGASM_FILE,
    TM_LANGUAGE,
    LIGHT_THEME,
    DARK_THEME,
] = [
    'onigasm.wasm',
    'qsharp.tmLanguage.json',
    'lightTheme.json',
    'darkTheme.json',
].map(x => path.join(SYNTAX_FILES_FOLDER, x))

const INIT_CODE = `namespace HelloWorld {

    open Microsoft.Quantum.Intrinsic;
    open Microsoft.Quantum.Measurement;

    operation RandomBit () : Result {
        using (q = Qubit()) {
            H(q);
            return MResetZ(q);
        }
    }   
}`

export class Editor {

    static async InitializeEditor(element) {
        element.innerHTML = "";

        window.editorsDict = window.editorsDict || {};
        window.editorsCounter = window.editorsCounter || 0;

        var id = "id" + (window.editorsCounter++);

        monaco.languages.register({
            id: 'qsharp',
            extensions: ['qs'],
            aliases: ['Q#', 'qsharp']
        });

        await loadWASM(ONIGASM_FILE);

        const registry = new Registry({
            getGrammarDefinition: async _ => ({
                format: 'json',
                content: await fetch(TM_LANGUAGE).then(x => x.text())
            })
        });

        const grammars = new Map([['qsharp', 'source.qsharp']]);

        monaco.editor.defineTheme('vs-code-theme-converted',
            await fetch(LIGHT_THEME).then(x => x.json())
        );

        window.editorsDict[id] = monaco.editor.create(element, {
            value: INIT_CODE,
            language: 'qsharp',
            theme: 'vs-code-theme-converted'
        });

        await wireTmGrammars(monaco, registry, grammars, window.editorsDict[id]);

        new ResizeObserver(() => window.editorsDict[id].layout()).observe(element);

        return id;
    }

    static GetCode(id) {
        return window.editorsDict[id].getValue();
    }

    static SetCode(id, code) {
        window.editorsDict[id].setValue(code);
    }
}


