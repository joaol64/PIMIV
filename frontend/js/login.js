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
}

form.addEventListener("submit", async (event) => {
  event.preventDefault();

  const email = document.getElementById("email").value.trim();
  const senha = document.getElementById("senha").value;

  try {
    // Frontend -> chama backend via HTTP (fetch)
    // Backend -> responde JSON com { id, nome, email } ou erro { message }
    const user = await apiPost("/auth/login", { email, senha });

    saveUserToLocalStorage(user);
    window.location.href = "home.html";
  } catch (err) {
    showMessage(err.message, "error");
  }
});

