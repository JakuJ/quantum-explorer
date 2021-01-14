import {Editor} from './editor';

// load theme from localStorage of preferences
let theme = localStorage.getItem('theme');
if (!theme) {
  const prefersDarkScheme = window.matchMedia('(prefers-color-scheme: dark)');
  theme = prefersDarkScheme.matches ? 'dark' : 'light';
}

// set the theme on page opened
setSiteTheme();

function setSiteTheme() {
  localStorage.setItem('theme', theme);

  if (theme === 'dark') {
    document.querySelector(':root').classList.add('dark-mode');
  } else {
    document.querySelector(':root').classList.remove('dark-mode');
  }
}

// Toggle between light and dark themes
export function toggleTheme() {
  theme = theme === 'dark' ? 'light' : 'dark';
  setSiteTheme();
  Editor.setColorTheme();
}
