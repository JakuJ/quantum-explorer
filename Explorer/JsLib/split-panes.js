import Split from 'split.js';

function equalSizes(num) {
  return new Array(num).fill(100 / num);
}

export function initializeSplitPanes(panel) {

  if (panel.children) {
    let ids = panel.children.map(x => `#${x.elementId}`);

    Split(ids, {
      direction: panel.direction,
      gutterSize: 8,
      sizes: equalSizes(ids.length),
    });

    panel.children.forEach(p => initializeSplitPanes(p));
  }
}
