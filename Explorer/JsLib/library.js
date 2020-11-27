// this is the main module, exposed globally as "Library"
// it exports all objects and methods used by Blazor

export {initializeSplitPanes} from './split-panes';
export {toggleTheme} from './theme';
export {Editor} from './editor';

// disabling and enabling DOM elements

export function disable(ref) {
  ref.disabled = true;
}

export function enable(ref) {
  ref.disabled = false;
}
