import { initializeSplitPanes } from './split-panes';
import { Editor } from './editor.js';

export function InitializeSplitPanes(panelJSON) {
  initializeSplitPanes(JSON.parse(panelJSON));
}

export { Editor } 



