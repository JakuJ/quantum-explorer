import * as monaco from 'monaco-editor';

export function initializeEditor() {

    monaco.editor.create(document.getElementById('editorRoot'), {
        value: 'console.log("Hello, world")',
        language: 'javascript'
    });
}
