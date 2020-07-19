import Split from 'split.js';

export function initializeSplitPanes() {
  Split(['#left-pane', '#right-pane'], {
    gutterSize: 8,
    sizes: [50, 50],
  });

  Split(['#upper-pane', '#lower-pane'], {
    direction: 'vertical',
    sizes: [50, 50],
    gutterSize: 8,
  });
}
