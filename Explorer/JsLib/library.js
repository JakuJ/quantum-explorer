import { initializeSplitPanes } from './split-panes';
import { Editor } from './editor';

export function InitializeSplitPanes(panelJSON) {
  initializeSplitPanes(JSON.parse(panelJSON));
}

export { Editor }