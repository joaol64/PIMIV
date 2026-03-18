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
}

form.addEventListener("submit", async (event) => {
  event.preventDefault();

  const nome = document.getElementById("nome").value.trim();
  const email = document.getElementById("email").value.trim();
  const senha = document.getElementById("senha").value;

  try {
    await apiPost("/auth/register", { nome, email, senha });
    showMessage("Cadastro realizado! Agora faça login.", "success");

    // Pequeno atraso para o usuário ler a mensagem.
    setTimeout(() => {
      window.location.href = "index.html";
    }, 900);
  } catch (err) {
    showMessage(err.message, "error");
  }
});

