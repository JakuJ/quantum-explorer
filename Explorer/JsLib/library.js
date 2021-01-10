// This is the main module, exposed globally as "Library".
// It exports all objects and methods used by Blazor.

export {initializeSplitPanes} from './split-panes';
export {toggleTheme} from './theme';
export {initGrids, setReference} from './composer';
export {Editor} from './editor';
export {showSharePopOver, initPopOverDestroyer} from './share';

// Helper functions for disabling and enabling DOM elements

export function disable(ref) {
    var c = ref.children;
    for (var i = 0; i < c.length; i++) {
        if (c[i].classList.contains("btn")) {
            c[i].disabled = true;
        }
    }
}

export function enable(ref) {
    var c = ref.children;
    console.log(c);
    for (var i = 0; i < c.length; i++) {
        if (c[i].classList.contains("btn")) {
            c[i].disabled = false;
        }
    }
}

export function copyToClipboard() {
  const text = $('#link-placeholder').attr('value');
  navigator.clipboard.writeText(text);
  $('#copied-toast').toast('show');
}

export function changeUrl(url) {
  history.pushState(null, '', url);
}

export function saveTextFile(filename, data) {
  var blob = new Blob([data], { type: 'text/csv' });
  if (window.navigator.msSaveOrOpenBlob) {
    window.navigator.msSaveBlob(blob, filename);
  }
  else {
    var elem = window.document.createElement('a');
    elem.href = window.URL.createObjectURL(blob);
    elem.download = filename;
    document.body.appendChild(elem);
    elem.click();
    document.body.removeChild(elem);
  }
}
