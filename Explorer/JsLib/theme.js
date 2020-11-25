// load theme from localStorage of preferences
import {Editor} from './editor';

let theme = localStorage.getItem('theme');
if (!theme) {
  const prefersDarkScheme = window.matchMedia('(prefers-color-scheme: dark)');
  theme = prefersDarkScheme.matches ? 'dark' : 'light';
}

setSiteTheme();

function setSiteTheme() {
  localStorage.setItem('theme', theme);

  if (theme === 'dark') {
    document.querySelector(':root').classList.add('dark-mode');
  } else {
    document.querySelector(':root').classList.remove('dark-mode');
  }
}

export function toggleTheme() {
  theme = theme === 'dark' ? 'light' : 'dark';
  setSiteTheme();
  Editor.setColorTheme();
}
