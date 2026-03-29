// inscricao.html — confirma inscrição via ?atividadeId=... (e opcionalmente &nome=...).

const resumoEl = document.getElementById("inscricaoResumo");
const avisoEl = document.getElementById("inscricaoAviso");
const btnConfirmar = document.getElementById("btnConfirmar");
const sucessoEl = document.getElementById("inscricaoSucesso");
const messageEl = document.getElementById("message");

function getQueryParam(name) {
  const params = new URLSearchParams(window.location.search);
  return params.get(name)?.trim() || "";
}

function showErro(text) {
  messageEl.textContent = text;
  messageEl.classList.add("show", "error");
  messageEl.classList.remove("success");
}

function limparErro() {
  messageEl.textContent = "";
  messageEl.classList.remove("show", "error", "success");
}

const atividadeId = getQueryParam("atividadeId");
const nomeAtividade = getQueryParam("nome") || "esta atividade";

if (!atividadeId) {
  resumoEl.textContent = "";
  avisoEl.textContent =
    "Informe o id da atividade na URL, por exemplo: inscricao.html?atividadeId=SEU_ID&nome=Nome%20da%20atividade";
  avisoEl.hidden = false;
  avisoEl.classList.add("show");
  btnConfirmar.disabled = true;
} else {
  resumoEl.textContent = `Você está prestes a se inscrever em: ${nomeAtividade}.`;
}

btnConfirmar.addEventListener("click", async () => {
  limparErro();
  sucessoEl.hidden = true;
  sucessoEl.classList.remove("show");

  const usuario = getUserFromLocalStorage();
  if (!usuario) {
    showErro("Faça login antes de confirmar. Redirecionando…");
    setTimeout(() => {
      window.location.href = "index.html";
    }, 1500);
    return;
  }

  if (!atividadeId) return;

  btnConfirmar.disabled = true;
  try {
    await apiPost("/inscricoes", {
      usuarioId: usuario.id,
      atividadeId: atividadeId,
    });
    sucessoEl.textContent = "Inscrição confirmada com sucesso!";
    sucessoEl.hidden = false;
    sucessoEl.classList.add("show");
    btnConfirmar.disabled = true;
  } catch (err) {
    showErro(err.message || "Não foi possível confirmar a inscrição.");
    btnConfirmar.disabled = false;
  }
});
