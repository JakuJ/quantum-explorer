import * as monaco from 'monaco-editor';


export class Editor {

    static InitializeEditor(element) {
        element.innerHTML = "";

        window.editorsDict = window.editorsDict || {};
        window.editorsCounter = window.editorsCounter || 0;

        var id = "id" + window.editorsCounter;
        window.editorsCounter = window.editorsCounter + 1;

        window.editorsDict[id] = monaco.editor.create(element, {
            value: `using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

        namespace CSharpTutorials
{
            class Program
    {
            static void Main(string[] args)
        {
            string message = "Hello World!!";

            Console.WriteLine(message);
        }
    }
}`,
            language: "csharp"
        });


        new ResizeObserver(function () {
            window.editorsDict[id].layout();
        }).observe(element);

        return id;
    }


    static GetCode(id) {
        var text = window.editorsDict[id].getValue();
        console.log(text);
        return text;
    }

    static SetCode(id, code) {
        console.log(code);
        window.editorsDict[id].setValue(code);
    }
}


