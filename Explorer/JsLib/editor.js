import * as monaco from 'monaco-editor';
import { loadWASM } from 'onigasm'
import { Registry } from 'monaco-textmate'
import { wireTmGrammars } from 'monaco-editor-textmate'
import * as path from 'path'

const SYNTAX_FILES_FOLDER = 'syntaxFiles'
const ONIGASM_FILE = 'onigasm.wasm'
const TM_LANGUAGE = 'qsharp.tmLanguage.json'
const DARK_THEME = 'darkTheme.json'
const LIGHT_THEME = 'lightTheme.json'

const INIT_CODE = `namespace HelloWorldOperations {

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

        var id = "id" + window.editorsCounter;
        window.editorsCounter = window.editorsCounter + 1;

        monaco.languages.register({
            id: 'qsharp',
            extensions: ['qs'],
            aliases: ['Q#', 'qsharp']
        })

        await loadWASM(path.join(SYNTAX_FILES_FOLDER, ONIGASM_FILE))

        const registry = new Registry({
            getGrammarDefinition: async (scopeName) => {
                return {
                    format: 'json',
                    content: await (await fetch(path.join(SYNTAX_FILES_FOLDER, TM_LANGUAGE))).text()
                }
            }
        })

        const grammars = new Map()
        grammars.set('qsharp', 'source.qsharp')

        monaco.editor.defineTheme('vs-code-theme-converted',
            await (await fetch(path.join(SYNTAX_FILES_FOLDER, LIGHT_THEME))).json()
        );

        window.editorsDict[id] = monaco.editor.create(element, {
            value: INIT_CODE,
            language: 'qsharp',
            theme: 'vs-code-theme-converted'
        });

        await wireTmGrammars(monaco, registry, grammars, window.editorsDict[id])

        new ResizeObserver(function () {
            window.editorsDict[id].layout();
        }).observe(element);

        return id;
    }


    static GetCode(id) {
        var text = window.editorsDict[id].getValue();
        return text;
    }

    static SetCode(id, code) {
        window.editorsDict[id].setValue(code);
    }
}


