// atividades.html — lista atividades do evento via API e inscrição do usuário logado.

const atividadesList = document.getElementById("atividadesList");
const atividadesLoading = document.getElementById("atividadesLoading");
const atividadesEmpty = document.getElementById("atividadesEmpty");
const atividadesMissingParam = document.getElementById("atividadesMissingParam");
const eventoIdLine = document.getElementById("eventoIdLine");
const messageEl = document.getElementById("message");

function showMessage(text, type) {
  messageEl.textContent = text;
  messageEl.classList.add("show");
  messageEl.classList.toggle("error", type === "error");
  messageEl.classList.toggle("success", type === "success");
  messageEl.classList.remove("message--warn");
}

function clearMessage() {
  messageEl.textContent = "";
  messageEl.classList.remove("show", "error", "success");
}

function getQueryParam(name) {
  const params = new URLSearchParams(window.location.search);
  return params.get(name)?.trim() || "";
}

/** Formata data ISO ou similar para exibição em pt-BR. */
function formatarData(valor) {
  if (valor == null || valor === "") return "—";
  const d = new Date(valor);
  if (Number.isNaN(d.getTime())) return String(valor);
  return d.toLocaleDateString("pt-BR", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });
}

function esconderLoading() {
  atividadesLoading.hidden = true;
  atividadesLoading.classList.remove("show");
}

function renderizarAtividades(itens) {
  atividadesList.innerHTML = "";
  if (!Array.isArray(itens) || itens.length === 0) {
    atividadesList.hidden = true;
    atividadesEmpty.hidden = false;
    return;
  }

  atividadesEmpty.hidden = true;
  atividadesList.hidden = false;

  itens.forEach((a) => {
    const id = a.id ?? a.Id ?? "";
    const nome = a.nome ?? a.Nome ?? "Atividade";
    const dataRaw = a.data ?? a.Data;

    const li = document.createElement("li");
    li.className = "eventos-item atividades-item";

    const h2 = document.createElement("h2");
    h2.className = "eventos-nome";
    h2.textContent = nome;

    const p = document.createElement("p");
    p.className = "eventos-data";
    const time = document.createElement("time");
    if (dataRaw) time.setAttribute("datetime", String(dataRaw));
    time.textContent = formatarData(dataRaw);
    p.appendChild(time);

    li.appendChild(h2);
    li.appendChild(p);

    if (id) {
      const q = new URLSearchParams({ atividadeId: id, nome: nome });
      const link = document.createElement("a");
      link.href = `inscricao.html?${q.toString()}`;
      link.className = "home-cta-btn eventos-detalhe-btn atividades-inscrever-link";
      link.textContent = "Se inscrever";
      li.appendChild(link);
    } else {
      const span = document.createElement("span");
      span.className = "atividades-inscrever-placeholder";
      span.textContent = "Se inscrever (sem id na API)";
      li.appendChild(span);
    }

    atividadesList.appendChild(li);
  });
}

async function carregarAtividades(eventoId) {
  clearMessage();
  atividadesMissingParam.hidden = true;
  atividadesEmpty.hidden = true;
  atividadesList.hidden = true;
  atividadesList.innerHTML = "";
  atividadesLoading.hidden = false;
  atividadesLoading.classList.add("show");
  eventoIdLine.textContent = `Evento selecionado (id): ${eventoId}`;

  try {
    const dados = await apiGet(`/eventos/${encodeURIComponent(eventoId)}/atividades`);
    const lista = Array.isArray(dados) ? dados : [];
    renderizarAtividades(lista);
  } catch (err) {
    eventoIdLine.textContent = `Evento (id): ${eventoId}`;
    showMessage(
      err.message ||
        "Não foi possível carregar as atividades. Verifique se a API está no ar e se o eventoId existe.",
      "error"
    );
    atividadesList.hidden = true;
  } finally {
    esconderLoading();
  }
}

const eventoId = getQueryParam("eventoId");

if (!eventoId) {
  esconderLoading();
  atividadesMissingParam.hidden = false;
  atividadesMissingParam.classList.add("show");
  eventoIdLine.textContent = "";
} else {
  carregarAtividades(eventoId);
}
