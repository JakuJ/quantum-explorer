import { initializeSplitPanes } from './split-panes';
import { Editor } from './editor';

export function InitializeSplitPanes(panelJSON) {
  initializeSplitPanes(JSON.parse(panelJSON));
}

export function enable(element, state) {
  element.disabled = !state;
}

export { Editor }
