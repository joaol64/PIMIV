// Código específico da página de cadastro.

const form = document.getElementById("registerForm");
const messageEl = document.getElementById("message");
const backBtn = document.getElementById("backBtn");

backBtn.addEventListener("click", () => {
  window.location.href = "index.html";
});

function showMessage(text, type) {
  messageEl.textContent = text;
  messageEl.classList.add("show");
  messageEl.classList.toggle("error", type === "error");
  messageEl.classList.toggle("success", type === "success");
  messageEl.classList.toggle("loading", type === "loading");
}

const LOADING_HINT =
  "Conectando ao servidor… Se demorar um pouco, o serviço pode estar iniciando após um período sem uso — aguarde.";

const MAX_NOME_EMAIL = 30;
const MIN_SENHA = 6;

form.addEventListener("submit", async (event) => {
  event.preventDefault();

  const nome = document.getElementById("nome").value.trim();
  const email = document.getElementById("email").value.trim();
  const senha = document.getElementById("senha").value;
  const submitBtn = form.querySelector('button[type="submit"]');

  if (nome.length > MAX_NOME_EMAIL || email.length > MAX_NOME_EMAIL) {
    showMessage(
      `Nome e email podem ter no máximo ${MAX_NOME_EMAIL} caracteres.`,
      "error"
    );
    return;
  }
  if (senha.length < MIN_SENHA) {
    showMessage(`A senha deve ter no mínimo ${MIN_SENHA} caracteres.`, "error");
    return;
  }

  showMessage(LOADING_HINT, "loading");
  if (submitBtn) submitBtn.disabled = true;

  let reenableSubmit = true;
  try {
    await apiPost("/auth/register", { nome, email, senha });
    showMessage("Cadastro realizado! Agora faça login.", "success");
    reenableSubmit = false;

    // Pequeno atraso para o usuário ler a mensagem.
    setTimeout(() => {
      window.location.href = "index.html";
    }, 900);
  } catch (err) {
    showMessage(err.message, "error");
  } finally {
    if (submitBtn && reenableSubmit) submitBtn.disabled = false;
  }
});

