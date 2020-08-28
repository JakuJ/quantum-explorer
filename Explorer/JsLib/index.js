import { initializeSplitPanes } from './split-panes';
import { initializeEditor } from './editor';

export function InitializeSplitPanes(panelJSON) {
  initializeSplitPanes(JSON.parse(panelJSON));
}

export function InitializeEditor() {
    initializeEditor();
}
