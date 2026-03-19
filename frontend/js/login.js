// Código específico da página de login.

const form = document.getElementById("loginForm");
const messageEl = document.getElementById("message");

// Se o usuário já estiver salvo no localStorage, vai direto para home.
const existingUser = getUserFromLocalStorage();
if (existingUser) {
  window.location.href = "home.html";
}

function showMessage(text, type) {
  messageEl.textContent = text;
  messageEl.classList.add("show");
  messageEl.classList.toggle("error", type === "error");
  messageEl.classList.toggle("success", type === "success");
  messageEl.classList.toggle("loading", type === "loading");
}

const LOADING_HINT =
  "Conectando ao servidor… Se demorar um pouco, o serviço pode estar iniciando após um período sem uso — aguarde.";

const MAX_EMAIL = 30;
const MIN_SENHA = 6;

form.addEventListener("submit", async (event) => {
  event.preventDefault();

  const email = document.getElementById("email").value.trim();
  const senha = document.getElementById("senha").value;
  const submitBtn = form.querySelector('button[type="submit"]');

  if (email.length > MAX_EMAIL) {
    showMessage(`O email pode ter no máximo ${MAX_EMAIL} caracteres.`, "error");
    return;
  }
  if (senha.length < MIN_SENHA) {
    showMessage(`A senha deve ter no mínimo ${MIN_SENHA} caracteres.`, "error");
    return;
  }

  showMessage(LOADING_HINT, "loading");
  if (submitBtn) submitBtn.disabled = true;

  try {
    // Frontend -> chama backend via HTTP (fetch)
    // Backend -> responde JSON com { id, nome, email } ou erro { message }
    const user = await apiPost("/auth/login", { email, senha });

    saveUserToLocalStorage(user);
    window.location.href = "home.html";
  } catch (err) {
    showMessage(err.message, "error");
  } finally {
    if (submitBtn) submitBtn.disabled = false;
  }
});

