// load it from cookie, if exists
const themes = ['light', 'dark'];
let currentThemeIndex = 0;

const savedTheme = getCookie('theme');
if (savedTheme && themes.includes(savedTheme)) {
    applyTheme(savedTheme);
    currentThemeIndex = themes.indexOf(savedTheme);
} else {
    applyTheme('light'); // default
}

document.getElementById('theme-toggle').addEventListener('click', () => {
    currentThemeIndex = (currentThemeIndex + 1) % themes.length;
    const nextTheme = themes[currentThemeIndex];
    applyTheme(nextTheme);
    setCookie('theme', nextTheme, 365);
});

function applyTheme(theme) {
    document.body.classList.remove(...themes.map(t => `theme-${t}`));
    document.body.classList.add(`theme-${theme}`);
}

function setCookie(name, value, days) {
    const d = new Date();
    d.setTime(d.getTime() + (days * 24 * 60 * 60 * 1000));
    document.cookie = `${name}=${value};expires=${d.toUTCString()};path=/`;
}

function getCookie(name) {
    const value = `; ${document.cookie}`;
    const parts = value.split(`; ${name}=`);
    if (parts.length === 2) return parts.pop().split(';').shift();
}