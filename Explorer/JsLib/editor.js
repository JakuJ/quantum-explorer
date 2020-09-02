import * as monaco from 'monaco-editor';

export class Editor {

    static InitializeEditor(element) {
        element.innerHTML = "";

        window.editorsArray = window.editorsArray || {};
        window.editorsCounter = window.editorsCounter || 0;

        var id = "id" + window.editorsCounter;
        window.editorsCounter = window.editorsCounter + 1;

        window.editorsArray[id] = monaco.editor.create(element, {
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

        return id;
    }


    static GetCode(id) {
        var text = window.editorsArray[id].getValue();
        console.log(text);
        return text;
    }

    static SetCode(id, code) {
        console.log(code);
        window.editorsArray[id].setValue(code);
    }
}


