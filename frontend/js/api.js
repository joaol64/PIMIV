// Centraliza a URL da API.
// IMPORTANTE:
// - Frontend (HTML/JS) e backend (.NET) são projetos separados.
// - O navegador chama o backend via HTTP usando fetch().
// - O backend responde JSON.
// Com dotnet run na sua máquina a API costuma ser http://localhost:5000/api
// (troque para a URL do Render em produção).
const API_URL = "http://localhost:5000/api";

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

/** Normaliza resposta do login (camelCase ou PascalCase) e garante `tipoUsuario` numérico. */
function normalizeAuthUser(raw) {
  if (!raw || typeof raw !== "object") return null;
  const id = raw.id ?? raw.Id ?? "";
  const nome = raw.nome ?? raw.Nome ?? "";
  const email = raw.email ?? raw.Email ?? "";
  const jaViuVideo = Boolean(raw.jaViuVideo ?? raw.JaViuVideo);
  let tipoUsuario = raw.tipoUsuario ?? raw.TipoUsuario ?? 0;
  if (tipoUsuario === "Administrador") tipoUsuario = 1;
  if (tipoUsuario === "Participante") tipoUsuario = 0;
  tipoUsuario = Number(tipoUsuario);
  if (Number.isNaN(tipoUsuario)) tipoUsuario = 0;
  return { id, nome, email, jaViuVideo, tipoUsuario };
}

/** true se tipoUsuario === 1 (Administrador no backend). */
function isAdministrator(user) {
  const u = user && normalizeAuthUser(user);
  return Boolean(u && u.tipoUsuario === 1);
}

function saveUserToLocalStorage(user) {
  const normalized = normalizeAuthUser(user);
  if (!normalized) return;
  localStorage.setItem("user", JSON.stringify(normalized));
}

function getUserFromLocalStorage() {
  const raw = localStorage.getItem("user");
  if (!raw) return null;
  try {
    const parsed = JSON.parse(raw);
    return normalizeAuthUser(parsed);
  } catch {
    return null;
  }
}

function logout() {
  localStorage.removeItem("user");
  window.location.href = "index.html";
}

/** Frase legível para exibir totais públicos de inscrição (evento ou atividade). */
function pimTextoContagemContasPublica(n) {
  if (n == null) return "—";
  if (n === 0) return "Nenhuma conta inscrita ainda";
  if (n === 1) return "1 conta inscrita";
  return `${n} contas inscritas`;
}

/** Contagem pública: participantes distintos com inscrição em alguma atividade do evento. Retorna null se a API falhar. */
async function pimContagemInscricoesEvento(eventoId) {
  if (!eventoId) return null;
  try {
    const r = await apiGet(`/inscricoes/contagem/evento/${encodeURIComponent(eventoId)}`);
    const n = r?.participantesDistintos ?? r?.ParticipantesDistintos;
    const num = Number(n);
    return Number.isFinite(num) ? num : 0;
  } catch {
    return null;
  }
}

/** Contagem pública: inscrições na atividade (no máximo uma por conta). Retorna null se a API falhar. */
async function pimContagemInscricoesAtividade(atividadeId) {
  if (!atividadeId) return null;
  try {
    const r = await apiGet(`/inscricoes/contagem/atividade/${encodeURIComponent(atividadeId)}`);
    const n = r?.total ?? r?.Total;
    const num = Number(n);
    return Number.isFinite(num) ? num : 0;
  } catch {
    return null;
  }
}

