import {initializeSplitPanes} from './split-panes';

export {Editor} from './editor';
export {initGrids, setReferences} from './composer';

export function InitializeSplitPanes(panelJSON) {
  initializeSplitPanes(JSON.parse(panelJSON));
}
