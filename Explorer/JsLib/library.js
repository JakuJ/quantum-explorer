import { initializeSplitPanes } from './split-panes';
export { Editor } from './editor';

export function InitializeSplitPanes(panelJSON) {
  initializeSplitPanes(JSON.parse(panelJSON));
}
