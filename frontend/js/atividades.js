// atividades.html — escolha do evento (API) e, em seguida, lista de atividades com inscrição.

const atividadesList = document.getElementById("atividadesList");
const atividadesLoading = document.getElementById("atividadesLoading");
const atividadesEmpty = document.getElementById("atividadesEmpty");
const eventoSelect = document.getElementById("eventoSelect");
const eventoStepHint = document.getElementById("eventoStepHint");
const atividadesStep2 = document.getElementById("atividadesStep2");
const eventoNomeEscolhido = document.getElementById("eventoNomeEscolhido");
const btnTrocarEvento = document.getElementById("btnTrocarEvento");
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

function formatarData(valor) {
  if (typeof pimFormatDateTimeBr === "function") return pimFormatDateTimeBr(valor);
  if (valor == null || valor === "") return "—";
  const d = new Date(valor);
  if (Number.isNaN(d.getTime())) return String(valor);
  return new Intl.DateTimeFormat("pt-BR", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  }).format(d);
}

function mesmoInstanteAtividade(a, b) {
  const da = typeof pimParseApiDate === "function" ? pimParseApiDate(a) : null;
  const db = typeof pimParseApiDate === "function" ? pimParseApiDate(b) : null;
  if (da && db) return da.getTime() === db.getTime();
  return String(a) === String(b);
}

function pickEvento(e) {
  const id = e?.id ?? e?.Id ?? "";
  const nome = e?.nome ?? e?.Nome ?? "Evento";
  const dataInicio = e?.dataInicio ?? e?.DataInicio ?? null;
  const dataFim = e?.dataFim ?? e?.DataFim ?? null;
  const data = e?.data ?? e?.Data ?? null;
  let inicio = dataInicio;
  let fim = dataFim;
  if (!inicio && data) inicio = data;
  if (!fim && data) fim = data;
  return { id, nome, inicio, fim };
}

function textoPeriodoEvento(inicio, fim) {
  if (!inicio && !fim) return "";
  if (inicio && fim && String(inicio) !== String(fim)) {
    const a = formatarData(inicio);
    const b = formatarData(fim);
    return ` (${a} — ${b})`;
  }
  return ` (${formatarData(inicio || fim)})`;
}

function esconderLoading() {
  atividadesLoading.hidden = true;
  atividadesLoading.classList.remove("show");
}

function renderizarAtividades(itens, contagemPorAtividadeId) {
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
    const dataInicio = a.data ?? a.Data ?? null;
    const dataFim = a.dataFim ?? a.DataFim ?? null;
    const fimEfetivo = dataFim ?? dataInicio;
    const intervalo =
      dataInicio && fimEfetivo && !mesmoInstanteAtividade(dataInicio, fimEfetivo);

    const li = document.createElement("li");
    li.className = "eventos-item atividades-item";

    const h2 = document.createElement("h2");
    h2.className = "eventos-nome";
    h2.textContent = nome;

    li.appendChild(h2);

    if (intervalo) {
      const periodo = document.createElement("div");
      periodo.className = "eventos-item-periodo";

      const rowIni = document.createElement("div");
      rowIni.className = "eventos-periodo-row";
      const labIni = document.createElement("span");
      labIni.className = "eventos-periodo-label";
      labIni.textContent = "Início";
      const timeIni = document.createElement("time");
      timeIni.className = "eventos-periodo-value";
      timeIni.textContent = formatarData(dataInicio);
      if (dataInicio && typeof pimParseApiDate === "function" && pimParseApiDate(dataInicio)) {
        timeIni.setAttribute("datetime", String(dataInicio));
      }
      rowIni.appendChild(labIni);
      rowIni.appendChild(timeIni);
      periodo.appendChild(rowIni);

      const rowFim = document.createElement("div");
      rowFim.className = "eventos-periodo-row";
      const labFim = document.createElement("span");
      labFim.className = "eventos-periodo-label";
      labFim.textContent = "Término";
      const timeFim = document.createElement("time");
      timeFim.className = "eventos-periodo-value";
      timeFim.textContent = formatarData(fimEfetivo);
      if (fimEfetivo && typeof pimParseApiDate === "function" && pimParseApiDate(fimEfetivo)) {
        timeFim.setAttribute("datetime", String(fimEfetivo));
      }
      rowFim.appendChild(labFim);
      rowFim.appendChild(timeFim);
      periodo.appendChild(rowFim);

      li.appendChild(periodo);
    } else {
      const p = document.createElement("p");
      p.className = "eventos-data";
      const time = document.createElement("time");
      const um = dataInicio || fimEfetivo;
      if (um) time.setAttribute("datetime", String(um));
      time.textContent = formatarData(um);
      p.appendChild(time);
      li.appendChild(p);
    }

    if (id && contagemPorAtividadeId) {
      const rowIns = document.createElement("div");
      rowIns.className = "eventos-periodo-row eventos-inscritos-row atividades-inscritos-row";
      const labIns = document.createElement("span");
      labIns.className = "eventos-periodo-label";
      labIns.textContent = "Contas inscritas";
      const valIns = document.createElement("span");
      valIns.className = "eventos-periodo-value";
      const n = contagemPorAtividadeId.get(id);
      valIns.textContent =
        typeof pimTextoContagemContasPublica === "function"
          ? pimTextoContagemContasPublica(n)
          : String(n ?? "—");
      rowIns.appendChild(labIns);
      rowIns.appendChild(valIns);
      li.appendChild(rowIns);
    }

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

let eventoSelecionadoId = "";
let eventoSelecionadoNome = "";

async function carregarAtividades(eventoId) {
  clearMessage();
  atividadesEmpty.hidden = true;
  atividadesList.hidden = true;
  atividadesList.innerHTML = "";
  atividadesLoading.hidden = false;
  atividadesLoading.classList.add("show");

  try {
    const dados = await apiGet(`/eventos/${encodeURIComponent(eventoId)}/atividades`);
    const lista = Array.isArray(dados) ? dados : [];
    const ids = lista.map((a) => a.id ?? a.Id ?? "").filter(Boolean);
    const contagens = await Promise.all(
      ids.map(async (aid) => {
        const n =
          typeof pimContagemInscricoesAtividade === "function"
            ? await pimContagemInscricoesAtividade(aid)
            : null;
        return [aid, n];
      })
    );
    const contagemPorAtividadeId = new Map(contagens);
    renderizarAtividades(lista, contagemPorAtividadeId);
  } catch (err) {
    showMessage(
      err.message ||
        "Não foi possível carregar as atividades. Verifique se a API está no ar e se o evento existe.",
      "error"
    );
    atividadesList.hidden = true;
  } finally {
    esconderLoading();
  }
}

function mostrarPasso2(nomeEvento) {
  atividadesStep2.hidden = false;
  eventoNomeEscolhido.textContent = nomeEvento;
}

function mostrarPasso1() {
  atividadesStep2.hidden = true;
  atividadesList.hidden = true;
  atividadesEmpty.hidden = true;
  atividadesList.innerHTML = "";
  eventoSelecionadoId = "";
  clearMessage();
}

async function popularEventos() {
  eventoSelect.innerHTML = "";
  const optLoad = document.createElement("option");
  optLoad.value = "";
  optLoad.textContent = "Carregando eventos…";
  eventoSelect.appendChild(optLoad);
  eventoSelect.disabled = true;

  try {
    const dados = await apiGet("/eventos");
    const lista = Array.isArray(dados) ? dados : [];
    eventoSelect.innerHTML = "";
    const opt0 = document.createElement("option");
    opt0.value = "";
    opt0.textContent = lista.length ? "Selecione um evento…" : "Nenhum evento cadastrado";
    eventoSelect.appendChild(opt0);

    lista.forEach((e) => {
      const { id, nome, inicio, fim } = pickEvento(e);
      if (!id) return;
      const opt = document.createElement("option");
      opt.value = id;
      opt.dataset.nomeEvento = nome;
      opt.textContent = nome + textoPeriodoEvento(inicio, fim);
      eventoSelect.appendChild(opt);
    });

    eventoSelect.disabled = false;

    const fromUrl = getQueryParam("eventoId");
    if (fromUrl && [...eventoSelect.options].some((o) => o.value === fromUrl)) {
      eventoSelect.value = fromUrl;
      await onEventoChosen(fromUrl);
    }
  } catch (err) {
    eventoSelect.innerHTML = "";
    const optErr = document.createElement("option");
    optErr.value = "";
    optErr.textContent = "Erro ao carregar eventos";
    eventoSelect.appendChild(optErr);
    showMessage(err.message || "Não foi possível listar eventos.", "error");
    eventoSelect.disabled = true;
  }
}

async function onEventoChosen(eventoId) {
  if (!eventoId) {
    mostrarPasso1();
    return;
  }
  const opt = eventoSelect.selectedOptions[0];
  eventoSelecionadoNome = opt?.dataset.nomeEvento || (opt ? opt.textContent.replace(/\s+\(.+\)\s*$/, "") : "") || "Evento";
  eventoSelecionadoId = eventoId;
  mostrarPasso2(eventoSelecionadoNome);
  await carregarAtividades(eventoId);
}

eventoSelect.addEventListener("change", () => {
  const v = eventoSelect.value.trim();
  if (!v) {
    mostrarPasso1();
    return;
  }
  onEventoChosen(v);
});

btnTrocarEvento.addEventListener("click", () => {
  eventoSelect.value = "";
  mostrarPasso1();
});

window.addEventListener("pim-eventos-mutated", () => {
  popularEventos();
});

window.addEventListener("pim-atividades-mutated", (ev) => {
  const id = ev.detail?.eventoId;
  if (id && id === eventoSelecionadoId) {
    carregarAtividades(id);
  }
});

if (eventoStepHint) {
  eventoStepHint.textContent =
    "Primeiro escolha o evento na lista (cada evento agrupa suas atividades). Depois confira as atividades e use Se inscrever quando quiser participar.";
}

popularEventos();
