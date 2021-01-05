// This is the main module, exposed globally as "Library".
// It exports all objects and methods used by Blazor.

export {initializeSplitPanes} from './split-panes';
export {toggleTheme} from './theme';
export {initGrids, initializeToggles, setReference} from './composer';
export {Editor} from './editor';
export {showSharePopOver, initPopOverDestroyer} from './share';

// Helper functions for disabling and enabling DOM elements

export function disable(ref) {
  ref.disabled = true;
}

export function enable(ref) {
  ref.disabled = false;
}

export function copyToClipboard() {
  const text = $('#link-placeholder').attr('value');
  navigator.clipboard.writeText(text);
  $('#copied-toast').toast('show');
}

export function changeUrl(url) {
  history.pushState(null, '', url);
}
