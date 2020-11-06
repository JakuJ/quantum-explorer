import {initializeSplitPanes} from './split-panes';

export {Editor} from './editor';
export {initGrid, associateGate} from './composer';

export function InitializeSplitPanes(panelJSON) {
  initializeSplitPanes(JSON.parse(panelJSON));
}
