// Código específico da página logada (home).

const welcomeTitle = document.getElementById("welcomeTitle");
const showVideoBtn = document.getElementById("showVideoBtn");
const logoutBtn = document.getElementById("logoutBtn");
const videoEl = document.getElementById("librasVideo");
const messageEl = document.getElementById("message");

function showMessage(text, type) {
  messageEl.textContent = text;
  messageEl.classList.add("show");
  messageEl.classList.toggle("error", type === "error");
  messageEl.classList.toggle("success", type === "success");
}

const user = getUserFromLocalStorage();
if (!user) {
  // Se alguém abrir home.html direto, sem login, volta para login.
  window.location.href = "index.html";
} else {
  welcomeTitle.textContent = `Bem-vindo, ${user.nome}`;
}

showVideoBtn.addEventListener("click", () => {
  // Mostra o vídeo HTML5 na tela.
  videoEl.classList.add("show");

  // Se o arquivo video.mp4 não existir, o navegador mostrará erro no player.
  // Isso é ok porque é um placeholder (você pode colocar um vídeo real depois).
  showMessage("Vídeo exibido abaixo.", "success");
});

logoutBtn.addEventListener("click", () => {
  logout();
});

