import Split from 'split.js';

// Make the gutters in the split pane layout draggable
export function initializeSplitPanes() {

  Split(['#left-pane', '#right-pane'], {
    direction: 'horizontal',
    gutterSize: 8,
    sizes: [50, 50],
  });

  Split(['#top-pane', '#bottom-pane'], {
    direction: 'vertical',
    gutterSize: 8,
    sizes: [50, 50],
  });

}
