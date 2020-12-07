// This is the main module, exposed globally as "Library".
// It exports all objects and methods used by Blazor.

export { initializeSplitPanes } from './split-panes';
export { toggleTheme } from './theme';
export { Editor } from './editor';
export { showSharePopOver, initPopOverDestroyer} from './share';

// Helper functions for disabling and enabling DOM elements

export function disable(ref) {
    ref.disabled = true;
}

export function enable(ref) {
    ref.disabled = false;
}

export function copyToClipboard(text) {
    navigator.clipboard.writeText(text)
        .catch(function (error) {
            alert(error);
        });
}

export function changeUrl(url) {
    history.pushState(null, '', url);
}
