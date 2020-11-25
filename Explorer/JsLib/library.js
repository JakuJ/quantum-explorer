import {initializeSplitPanes} from './split-panes';

export {toggleTheme} from './theme';
export {Editor} from './editor';

export function InitializeSplitPanes(panelJSON) {
  initializeSplitPanes(JSON.parse(panelJSON));
}
