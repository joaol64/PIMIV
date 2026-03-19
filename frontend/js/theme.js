const THEME_STORAGE_KEY = "theme";

function applyTheme(theme) {
  document.body.setAttribute("data-theme", theme);
}

function getSavedTheme() {
  return localStorage.getItem(THEME_STORAGE_KEY) || "dark";
}

function saveTheme(theme) {
  localStorage.setItem(THEME_STORAGE_KEY, theme);
}

function getToggleLabel(theme) {
  return theme === "light" ? "Tema escuro" : "Tema claro";
}

const themeToggleBtn = document.getElementById("themeToggleBtn");
const initialTheme = getSavedTheme();
applyTheme(initialTheme);

if (themeToggleBtn) {
  themeToggleBtn.textContent = getToggleLabel(initialTheme);
  themeToggleBtn.addEventListener("click", () => {
    const currentTheme = document.body.getAttribute("data-theme") || "dark";
    const nextTheme = currentTheme === "dark" ? "light" : "dark";
    applyTheme(nextTheme);
    saveTheme(nextTheme);
    themeToggleBtn.textContent = getToggleLabel(nextTheme);
  });
}
