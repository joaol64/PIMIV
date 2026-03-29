// Página inicial do evento (usuário logado): vídeo Libras visível + ações de programação/inscrição.

const welcomeLine = document.getElementById("welcomeLine");
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

function updateVideoStatusLabel(u) {
  if (!videoStatusText) return;
  videoStatusText.textContent = u.jaViuVideo
    ? "Status da conta: vídeo já visualizado."
    : "Status da conta: vídeo ainda não marcado como visualizado.";
}

const user = getUserFromLocalStorage();
if (!user) {
  window.location.href = "index.html";
} else {
  welcomeLine.textContent = `Olá, ${user.nome} — você está conectado.`;
  updateVideoStatusLabel(user);
}

// Ao dar play no vídeo de apresentação, registra no backend (uma vez) como no fluxo anterior.
let videoSeenSyncStarted = false;
videoEl.addEventListener("play", async () => {
  if (!user || user.jaViuVideo || videoSeenSyncStarted) return;
  videoSeenSyncStarted = true;

  try {
    const updatedUser = await apiPost("/auth/video-seen", { userId: user.id });
    saveUserToLocalStorage(updatedUser);
    user.jaViuVideo = updatedUser.jaViuVideo;
    updateVideoStatusLabel(user);
  } catch {
    showMessage("Não foi possível atualizar o status do vídeo agora.", "error");
    videoSeenSyncStarted = false;
  }
});

logoutBtn.addEventListener("click", () => {
  logout();
});
