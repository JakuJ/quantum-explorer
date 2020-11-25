import {initializeSplitPanes} from './split-panes';

export {toggleTheme} from './theme';
export {Editor} from './editor';

export function InitializeSplitPanes(panelJSON) {
  initializeSplitPanes(JSON.parse(panelJSON));
}

export function disable(ref) {
  ref.disabled = true;
}

export function enable(ref) {
  ref.disabled = false;
}
