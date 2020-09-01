import * as monaco from 'monaco-editor';

export function initializeEditor() {

    var editor = document.getElementById('editorRoot');
    editor.innerHTML = "";


    monaco.editor.create(editor, {
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
}
