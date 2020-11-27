export {initializeSplitPanes} from './split-panes';
export {toggleTheme} from './theme';
export {initGrids, setReferences} from './composer';
export {Editor} from './editor';

export function disable(ref) {
  ref.disabled = true;
}

export function enable(ref) {
  ref.disabled = false;
}
