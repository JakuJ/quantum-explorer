import {initializeSplitPanes} from './split-panes';

export {Editor} from './editor';
export {initGrids, associateGate} from './composer';

export function InitializeSplitPanes(panelJSON) {
  initializeSplitPanes(JSON.parse(panelJSON));
}
