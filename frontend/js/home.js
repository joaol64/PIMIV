// Código específico da página logada (home).

const welcomeTitle = document.getElementById("welcomeTitle");
const showVideoBtn = document.getElementById("showVideoBtn");
const logoutBtn = document.getElementById("logoutBtn");
const videoEl = document.getElementById("librasVideo");
const messageEl = document.getElementById("message");
const videoStatusText = document.getElementById("videoStatusText");

function showMessage(text, type) {
  messageEl.textContent = text;
  messageEl.classList.add("show");
  messageEl.classList.toggle("error", type === "error");
  messageEl.classList.toggle("success", type === "success");
}

function updateVideoStatusLabel(user) {
  if (!videoStatusText) return;
  videoStatusText.textContent = user.jaViuVideo
    ? "Status da conta: video ja visualizado."
    : "Status da conta: video ainda nao visualizado.";
}

const user = getUserFromLocalStorage();
if (!user) {
  // Se alguém abrir home.html direto, sem login, volta para login.
  window.location.href = "index.html";
} else {
  welcomeTitle.textContent = `Bem-vindo, ${user.nome}`;
  updateVideoStatusLabel(user);
}

showVideoBtn.addEventListener("click", async () => {
  // Mostra o vídeo HTML5 na tela.
  videoEl.classList.add("show");

  videoEl.play(); // 🔥 começa o vídeo

  // Se o arquivo video.mp4 não existir, o navegador mostrará erro no player.
  // Isso é ok porque é um placeholder (você pode colocar um vídeo real depois).
  showMessage("Video exibido abaixo.", "success");

  if (!user || user.jaViuVideo) return;

  try {
    const updatedUser = await apiPost("/auth/video-seen", { userId: user.id });
    saveUserToLocalStorage(updatedUser);
    user.jaViuVideo = updatedUser.jaViuVideo;
    updateVideoStatusLabel(user);
  } catch {
    showMessage("Nao foi possivel atualizar o status do video agora.", "error");
  }
});

logoutBtn.addEventListener("click", () => {
  logout();
});

