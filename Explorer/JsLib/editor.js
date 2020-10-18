import * as monaco from 'monaco-editor';
import { loadWASM } from 'onigasm'
import { Registry } from 'monaco-textmate'
import { wireTmGrammars } from 'monaco-editor-textmate'


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

        var startCode =`namespace Bell {
    open Microsoft.Quantum.Intrinsic;
    open Microsoft.Quantum.Canon;

    operation SetQubitState(desired : Result, q1 : Qubit) : Unit {
        if (desired != M(q1)) {
            X(q1);
        }
    }
}`

        await loadWASM('syntaxFiles/onigasm.wasm')

        const registry = new Registry({
            getGrammarDefinition: async (scopeName) => {
                return {
                    format: 'json',
                    content: await (await fetch('syntaxFiles/qsharp.tmLanguage.json')).text()
                }
            }
        })

        const grammars = new Map()
        grammars.set('qsharp', 'source.qsharp')

        monaco.editor.defineTheme('vs-code-theme-converted',
            await (await fetch('syntaxFiles/newTheme.json')).json()
        );

        window.editorsDict[id] = monaco.editor.create(element, {
            value: startCode,
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


