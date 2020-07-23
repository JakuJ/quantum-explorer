import Split from 'split.js';

function equalSizes(num) {
  return new Array(num).fill(100 / num);
}

export function initializeSplitPanes(panel) {

  if (panel.hasOwnProperty('Children')) {
    let ids = panel.Children.map(x => `#${x.ElementId}`);

    Split(ids, {
      direction: panel.Direction === 1 ? 'vertical' : 'horizontal',
      gutterSize: 8,
      sizes: equalSizes(ids.length),
    });

    panel.Children.forEach(p => initializeSplitPanes(p));
  }
}
