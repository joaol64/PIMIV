// Centraliza a URL da API.
// IMPORTANTE:
// - Frontend (HTML/JS) e backend (.NET) são projetos separados.
// - O navegador chama o backend via HTTP usando fetch().
// - O backend responde JSON.
const API_URL = "https://pimiv.onrender.com/api";

async function apiGet(path) {
  const response = await fetch(`${API_URL}${path}`);

  const text = await response.text();
  let data = null;
  if (text) {
    try {
      data = JSON.parse(text);
    } catch {
      data = null;
    }
  }

  if (!response.ok) {
    const message = data?.message || data?.Message || "Erro ao chamar a API";
    throw new Error(message);
  }

  return data;
}

async function apiPost(path, body) {
  const response = await fetch(`${API_URL}${path}`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(body),
  });

  // Tentamos ler JSON mesmo quando dá erro, para pegar { message } do backend.
  let data = null;
  try {
    data = await response.json();
  } catch {
    // Se não veio JSON, tudo bem.
  }

  if (!response.ok) {
    const message = data?.message || data?.Message || "Erro ao chamar a API";
    throw new Error(message);
  }

  return data;
}

function saveUserToLocalStorage(user) {
  // EXTRA: manter login salvo
  localStorage.setItem("user", JSON.stringify(user));
}

function getUserFromLocalStorage() {
  const raw = localStorage.getItem("user");
  if (!raw) return null;
  try {
    return JSON.parse(raw);
  } catch {
    return null;
  }
}

function logout() {
  localStorage.removeItem("user");
  window.location.href = "index.html";
}

