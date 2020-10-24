export function saveCode(code) {
  try {
    window.localStorage.setItem('code', code);
    $('#saved-toast').toast('show');
    return true;
  } catch (err) {
    console.error(err);
    $('#save-failed-toast').toast('show');
    return false;
  }
}

export function loadCode() {
  return window.localStorage.getItem('code');
}
