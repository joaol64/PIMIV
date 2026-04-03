// eventos.html — lista eventos da API; cards com início e término do período.

const eventosList = document.getElementById("eventosList");
const eventosLoading = document.getElementById("eventosLoading");
const eventosEmpty = document.getElementById("eventosEmpty");
const messageEl = document.getElementById("message");

function showMessage(text, type) {
  messageEl.textContent = text;
  messageEl.classList.add("show");
  messageEl.classList.toggle("error", type === "error");
  messageEl.classList.remove("success");
}

function clearMessage() {
  messageEl.textContent = "";
  messageEl.classList.remove("show", "error", "success");
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

function formatarOuEmDash(valor) {
  return typeof pimFormatDateTimeBr === "function" ? pimFormatDateTimeBr(valor) : "—";
}

function mesmoInstante(a, b) {
  const da = pimParseApiDate(a);
  const db = pimParseApiDate(b);
  if (!da || !db) return String(a) === String(b);
  return da.getTime() === db.getTime();
}

function renderEventos(lista, contagemPorEventoId) {
  eventosList.innerHTML = "";
  if (!lista.length) {
    eventosEmpty.hidden = false;
    return;
  }
  eventosEmpty.hidden = true;

  lista.forEach((e) => {
    const { id, nome, inicio, fim } = pickEvento(e);
    const li = document.createElement("li");
    li.className = "eventos-item";

    const top = document.createElement("div");
    top.className = "eventos-item-top";

    const h2 = document.createElement("h2");
    h2.className = "eventos-nome";
    h2.textContent = nome;

    top.appendChild(h2);

    if (id) {
      const a = document.createElement("a");
      a.className = "secondary eventos-detalhe-btn";
      a.href = `atividades.html?eventoId=${encodeURIComponent(id)}`;
      a.textContent = "Ver atividades";
      top.appendChild(a);
    }

    li.appendChild(top);

    const meta = document.createElement("p");
    meta.className = "eventos-meta";
    meta.textContent = "Evento acadêmico";
    li.appendChild(meta);

    const periodo = document.createElement("div");
    periodo.className = "eventos-item-periodo";

    const rowIni = document.createElement("div");
    rowIni.className = "eventos-periodo-row";
    const labIni = document.createElement("span");
    labIni.className = "eventos-periodo-label";
    labIni.textContent = "Início";
    const timeIni = document.createElement("time");
    timeIni.className = "eventos-periodo-value";
    timeIni.textContent = formatarOuEmDash(inicio);
    if (inicio && pimParseApiDate(inicio)) timeIni.setAttribute("datetime", String(inicio));
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
    timeFim.textContent = formatarOuEmDash(fim);
    if (fim && pimParseApiDate(fim)) timeFim.setAttribute("datetime", String(fim));
    rowFim.appendChild(labFim);
    rowFim.appendChild(timeFim);
    periodo.appendChild(rowFim);

    if (inicio && fim && mesmoInstante(inicio, fim)) {
      const note = document.createElement("p");
      note.className = "eventos-periodo-note";
      note.textContent = "Período de um único instante (evento legado ou cadastro pontual).";
      periodo.appendChild(note);
    }

    if (id && contagemPorEventoId) {
      const rowIns = document.createElement("div");
      rowIns.className = "eventos-periodo-row eventos-inscritos-row";
      const labIns = document.createElement("span");
      labIns.className = "eventos-periodo-label";
      labIns.textContent = "Contas inscritas";
      const valIns = document.createElement("span");
      valIns.className = "eventos-periodo-value";
      const n = contagemPorEventoId.get(id);
      valIns.textContent =
        typeof pimTextoContagemContasPublica === "function"
          ? pimTextoContagemContasPublica(n)
          : String(n ?? "—");
      rowIns.appendChild(labIns);
      rowIns.appendChild(valIns);
      periodo.appendChild(rowIns);
    }

    li.appendChild(periodo);

    const pFoot = document.createElement("div");
    pFoot.className = "eventos-data";
    const status = document.createElement("span");
    status.className = "eventos-status";
    status.textContent = "Ativo";
    pFoot.appendChild(status);
    li.appendChild(pFoot);

    eventosList.appendChild(li);
  });
}

async function carregar() {
  clearMessage();
  eventosEmpty.hidden = true;
  eventosList.innerHTML = "";
  eventosLoading.hidden = false;
  eventosLoading.classList.add("show");

  try {
    const dados = await apiGet("/eventos");
    const lista = Array.isArray(dados) ? dados : [];
    const ids = lista.map((e) => pickEvento(e).id).filter(Boolean);
    const contagens = await Promise.all(
      ids.map(async (eventoId) => {
        const n =
          typeof pimContagemInscricoesEvento === "function"
            ? await pimContagemInscricoesEvento(eventoId)
            : null;
        return [eventoId, n];
      })
    );
    const contagemPorEventoId = new Map(contagens);
    renderEventos(lista, contagemPorEventoId);
  } catch (err) {
    showMessage(err.message || "Não foi possível carregar os eventos.", "error");
  } finally {
    eventosLoading.hidden = true;
    eventosLoading.classList.remove("show");
  }
}

window.addEventListener("pim-eventos-mutated", () => {
  carregar();
});

carregar();
