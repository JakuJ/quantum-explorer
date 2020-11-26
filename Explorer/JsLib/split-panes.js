import Split from 'split.js';

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
