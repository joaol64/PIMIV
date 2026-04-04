// Painel exclusivo do administrador: modal com abas para criar evento ou atividade.
// Datas digitadas como texto (sem calendário nativo). Depende de api.js; dates.js é opcional (dicas em pt-BR).

function pickEventoFields(e) {
  const id = e?.id ?? e?.Id ?? "";
  const nome = e?.nome ?? e?.Nome ?? "Evento";
  const dataInicio = e?.dataInicio ?? e?.DataInicio ?? null;
  const dataFim = e?.dataFim ?? e?.DataFim ?? null;
  const data = e?.data ?? e?.Data ?? null;
  return { id, nome, dataInicio, dataFim, data };
}

function eventoPeriodoIso(e) {
  const { dataInicio, dataFim, data } = pickEventoFields(e);
  if (dataInicio && dataFim) return { inicio: dataInicio, fim: dataFim };
  if (data) return { inicio: data, fim: data };
  return { inicio: null, fim: null };
}

/**
 * Interpreta texto digitado pelo admin (horário local do navegador).
 * Aceita: DD/MM/AAAA HH:mm (preferencial) ou AAAA-MM-DD HH:mm (colagem legada).
 */
function adminParseDataDigitada(s) {
  if (!s || typeof s !== "string") return null;
  const t = s.trim().replace(/\s+/g, " ");
  if (!t) return null;

  let m = /^(\d{1,2})\/(\d{1,2})\/(\d{4})\s+(\d{1,2}):(\d{2})$/.exec(t);
  if (m) {
    const day = Number(m[1]);
    const month = Number(m[2]) - 1;
    const year = Number(m[3]);
    const h = Number(m[4]);
    const min = Number(m[5]);
    const d = new Date(year, month, day, h, min, 0, 0);
    if (Number.isNaN(d.getTime())) return null;
    if (d.getFullYear() !== year || d.getMonth() !== month || d.getDate() !== day) return null;
    return d;
  }

  m = /^(\d{4})-(\d{2})-(\d{2})[T\s](\d{1,2}):(\d{2})$/.exec(t);
  if (m) {
    const d = new Date(
      Number(m[1]),
      Number(m[2]) - 1,
      Number(m[3]),
      Number(m[4]),
      Number(m[5]),
      0,
      0
    );
    return Number.isNaN(d.getTime()) ? null : d;
  }

  return null;
}

/** Mensagem curta quando a data não pôde ser interpretada. */
function adminMsgFormatoData() {
  return "Data ou hora inválida. Ex.: 08/04/2026 09:00 ou 12 números (DD MM AAAA HH mm). Também aceita colar AAAA-MM-DD HH:mm.";
}

const ADMIN_DATE_HELP =
  "Datas em dia/mês/ano: digite só números (ordem DD MM AAAA HH mm, máscara com barras) ou cole DD/MM/AAAA HH:mm.";

/** Comprimento máximo do texto exibido: DD/MM/AAAA HH:mm */
const ADMIN_DATA_MAX_LEN = 16;

/** Formata para exibição no padrão brasileiro (dia/mês/ano, 24h). */
function adminFormatFromDateBr(d) {
  const p2 = (n) => String(n).padStart(2, "0");
  return `${p2(d.getDate())}/${p2(d.getMonth() + 1)}/${d.getFullYear()} ${p2(d.getHours())}:${p2(d.getMinutes())}`;
}

/**
 * Interpreta 12 dígitos: primeiro tenta DD MM YYYY HH mm; se data inválida, AAAAMMDDHHmm (legado).
 */
function adminParseDozeDigitos(digits) {
  if (digits.length !== 12) return null;
  const day = Number(digits.slice(0, 2));
  const month = Number(digits.slice(2, 4));
  const year = Number(digits.slice(4, 8));
  const h = Number(digits.slice(8, 10));
  const min = Number(digits.slice(10, 12));
  let d = new Date(year, month - 1, day, h, min, 0, 0);
  if (
    !Number.isNaN(d.getTime()) &&
    d.getFullYear() === year &&
    d.getMonth() === month - 1 &&
    d.getDate() === day
  ) {
    return d;
  }
  const y = Number(digits.slice(0, 4));
  const mo = Number(digits.slice(4, 6));
  const da = Number(digits.slice(6, 8));
  const hh = Number(digits.slice(8, 10));
  const mi = Number(digits.slice(10, 12));
  d = new Date(y, mo - 1, da, hh, mi, 0, 0);
  if (
    !Number.isNaN(d.getTime()) &&
    d.getFullYear() === y &&
    d.getMonth() === mo - 1 &&
    d.getDate() === da
  ) {
    return d;
  }
  return null;
}

/** Até 12 dígitos na ordem DD MM AAAA HH mm → DD/MM/AAAA HH:mm (durante a digitação). */
function adminDigitsToBrMask(digits) {
  const d = digits.replace(/\D/g, "").slice(0, 12);
  if (!d) return "";
  let out = d.slice(0, Math.min(2, d.length));
  if (d.length >= 3) out += "/" + d.slice(2, Math.min(4, d.length));
  if (d.length >= 5) out += "/" + d.slice(4, Math.min(8, d.length));
  if (d.length >= 9) out += " " + d.slice(8, Math.min(10, d.length));
  if (d.length >= 11) out += ":" + d.slice(10, 12);
  return out;
}

function adminApplyDateFieldInput(el) {
  const t = el.value.trim();
  if (t) {
    const p = adminParseDataDigitada(t);
    if (p) {
      el.value = adminFormatFromDateBr(p);
      return;
    }
  }
  const digits = el.value.replace(/\D/g, "").slice(0, 12);
  el.value = adminDigitsToBrMask(digits);
}

function adminNormalizeDateFieldBlur(el) {
  let t = el.value.trim();
  if (!t) return;
  let p = adminParseDataDigitada(t);
  if (!p) {
    const digits = t.replace(/\D/g, "");
    if (digits.length === 12) {
      p = adminParseDozeDigitos(digits);
    }
  }
  if (p) el.value = adminFormatFromDateBr(p);
}

/** Converte o texto do campo em ISO UTC para o JSON (parse BR/ISO + 12 dígitos + Date.parse). */
function adminDateToIsoForApi(raw) {
  const s = String(raw ?? "").trim();
  if (!s) return null;
  let d = adminParseDataDigitada(s);
  if (!d) {
    const digits = s.replace(/\D/g, "");
    if (digits.length === 12) d = adminParseDozeDigitos(digits);
  }
  if (!d) {
    const ms = Date.parse(s);
    if (!Number.isNaN(ms)) {
      const cand = new Date(ms);
      if (cand.getFullYear() >= 1900) d = cand;
    }
  }
  if (!d || d.getFullYear() < 1900) return null;
  return d.toISOString();
}

function adminWireDateFields(els) {
  els.forEach((el) => {
    if (!el) return;
    el.setAttribute("maxlength", String(ADMIN_DATA_MAX_LEN));
    el.setAttribute("inputmode", "numeric");
    el.setAttribute("autocomplete", "off");
    el.setAttribute("spellcheck", "false");
    el.addEventListener("input", () => adminApplyDateFieldInput(el));
    el.addEventListener("blur", () => adminNormalizeDateFieldBlur(el));
  });
}

async function fetchEventosList() {
  const dados = await apiGet("/eventos");
  return Array.isArray(dados) ? dados : [];
}

function fillEventoSelect(selectEl, eventos, placeholder) {
  selectEl.innerHTML = "";
  const opt0 = document.createElement("option");
  opt0.value = "";
  opt0.textContent = placeholder;
  selectEl.appendChild(opt0);
  eventos.forEach((e) => {
    const { id, nome } = pickEventoFields(e);
    if (!id) return;
    const opt = document.createElement("option");
    opt.value = id;
    opt.textContent = nome;
    const { inicio, fim } = eventoPeriodoIso(e);
    if (inicio) opt.dataset.inicio = String(inicio);
    if (fim) opt.dataset.fim = String(fim);
    selectEl.appendChild(opt);
  });
}

function initAdminPanel() {
  if (typeof getUserFromLocalStorage !== "function") return;
  if (typeof isAdministrator !== "function") return;
  if (typeof apiGet !== "function" || typeof apiPost !== "function") return;

  const user = getUserFromLocalStorage();
  if (!isAdministrator(user) || !user.id) return;

  const topbar = document.querySelector(".card-topbar");
  if (!topbar || topbar.querySelector("[data-admin-panel-btn]")) return;

  const openBtn = document.createElement("button");
  openBtn.type = "button";
  openBtn.className = "secondary admin-panel-open-btn";
  openBtn.setAttribute("data-admin-panel-btn", "");
  openBtn.setAttribute("aria-haspopup", "dialog");
  openBtn.textContent = "Gestão (admin)";

  const themeBtn = document.getElementById("themeToggleBtn");
  if (themeBtn && themeBtn.parentElement === topbar) {
    topbar.insertBefore(openBtn, themeBtn);
  } else {
    topbar.prepend(openBtn);
  }

  const dialog = document.createElement("dialog");
  dialog.className = "admin-dialog";
  dialog.setAttribute("aria-labelledby", "adminDialogTitle");

  dialog.innerHTML = `
    <div class="admin-dialog-inner">
      <header class="admin-dialog-header">
        <h2 id="adminDialogTitle" class="admin-dialog-title">Gestão de eventos e atividades</h2>
        <button type="button" class="secondary admin-dialog-close" aria-label="Fechar">Fechar</button>
      </header>
      <div class="admin-tabs" role="tablist" aria-label="Abas do painel">
        <button type="button" class="admin-tab admin-tab--active" role="tab" aria-selected="true" data-tab="evento" id="adminTabEvento">Novo evento</button>
        <button type="button" class="admin-tab" role="tab" aria-selected="false" data-tab="atividade" id="adminTabAtividade">Nova atividade</button>
      </div>
      <div class="admin-tab-panels">
        <div class="admin-panel admin-panel--active" role="tabpanel" id="adminPanelEvento" aria-labelledby="adminTabEvento">
          <p class="admin-panel-hint">Cada evento agrupa várias atividades. Defina o período em que o evento fica ativo.</p>
          <p class="admin-date-help" role="note">${ADMIN_DATE_HELP}</p>
          <form id="adminFormEvento" class="admin-form">
            <label for="adminEventoNome">Nome do evento</label>
            <input id="adminEventoNome" name="nome" type="text" required maxlength="120" autocomplete="off" />
            <label for="adminEventoDataInicio">Data e hora de início</label>
            <input id="adminEventoDataInicio" name="dataInicio" type="text" required spellcheck="false" placeholder="Ex.: 08/04/2026 09:00" title="DD/MM/AAAA HH:mm ou 12 números (DD MM AAAA HH mm)" />
            <label for="adminEventoDataFim">Data e hora de término</label>
            <input id="adminEventoDataFim" name="dataFim" type="text" required spellcheck="false" placeholder="Ex.: 10/04/2026 18:00" title="DD/MM/AAAA HH:mm ou 12 números (DD MM AAAA HH mm)" />
            <p id="adminEventoMsg" class="message admin-form-msg" role="status"></p>
            <button type="submit">Criar evento</button>
          </form>
        </div>
        <div class="admin-panel" role="tabpanel" id="adminPanelAtividade" aria-labelledby="adminTabAtividade" hidden>
          <p class="admin-panel-hint">Escolha o evento. O intervalo da atividade precisa caber inteiro dentro do período do evento.</p>
          <p class="admin-date-help" role="note">${ADMIN_DATE_HELP}</p>
          <form id="adminFormAtividade" class="admin-form">
            <label for="adminAtividadeEventoId">Evento</label>
            <select id="adminAtividadeEventoId" name="eventoId" required></select>
            <label for="adminAtividadeNome">Nome da atividade</label>
            <input id="adminAtividadeNome" name="nome" type="text" required maxlength="120" autocomplete="off" />
            <label for="adminAtividadeDescricao">Descrição <span class="admin-optional-label">(opcional)</span></label>
            <textarea id="adminAtividadeDescricao" name="descricao" class="admin-atividade-descricao" maxlength="2000" rows="4" spellcheck="true" placeholder="Detalhes da atividade para quem vai se inscrever…" aria-describedby="adminAtividadeDescricaoHint"></textarea>
            <p id="adminAtividadeDescricaoHint" class="admin-textarea-resize-hint">Altura ajustável: arraste o canto inferior direito. O tamanho inicial acompanha a tela.</p>
            <label for="adminAtividadeDataInicio">Data e hora de início</label>
            <input id="adminAtividadeDataInicio" name="dataInicio" type="text" required spellcheck="false" placeholder="Ex.: 09/04/2026 14:00" title="DD/MM/AAAA HH:mm ou 12 números (DD MM AAAA HH mm)" />
            <label for="adminAtividadeDataFim">Data e hora de término</label>
            <input id="adminAtividadeDataFim" name="dataFim" type="text" required spellcheck="false" placeholder="Ex.: 09/04/2026 15:30" title="DD/MM/AAAA HH:mm ou 12 números (DD MM AAAA HH mm)" />
            <p id="adminAtividadeHint" class="admin-panel-hint admin-atividade-range-hint" hidden></p>
            <p id="adminAtividadeMsg" class="message admin-form-msg" role="status"></p>
            <button type="submit">Criar atividade</button>
          </form>
        </div>
      </div>
    </div>
  `;

  document.body.appendChild(dialog);

  const tabButtons = dialog.querySelectorAll(".admin-tab");
  const panels = {
    evento: dialog.querySelector("#adminPanelEvento"),
    atividade: dialog.querySelector("#adminPanelAtividade"),
  };
  const selectEvento = dialog.querySelector("#adminAtividadeEventoId");
  const inputAtividadeDataInicio = dialog.querySelector("#adminAtividadeDataInicio");
  const inputAtividadeDataFim = dialog.querySelector("#adminAtividadeDataFim");
  const hintAtividadeRange = dialog.querySelector("#adminAtividadeHint");
  const formEvento = dialog.querySelector("#adminFormEvento");
  const formAtividade = dialog.querySelector("#adminFormAtividade");
  const msgEvento = dialog.querySelector("#adminEventoMsg");
  const msgAtividade = dialog.querySelector("#adminAtividadeMsg");
  const btnClose = dialog.querySelector(".admin-dialog-close");

  const inputInicio = dialog.querySelector("#adminEventoDataInicio");
  const inputFim = dialog.querySelector("#adminEventoDataFim");
  adminWireDateFields([
    inputInicio,
    inputFim,
    inputAtividadeDataInicio,
    inputAtividadeDataFim,
  ]);

  function setTab(name) {
    tabButtons.forEach((btn) => {
      const on = btn.getAttribute("data-tab") === name;
      btn.classList.toggle("admin-tab--active", on);
      btn.setAttribute("aria-selected", on ? "true" : "false");
    });
    Object.entries(panels).forEach(([key, panel]) => {
      const on = key === name;
      panel.classList.toggle("admin-panel--active", on);
      panel.hidden = !on;
    });
  }

  tabButtons.forEach((btn) => {
    btn.addEventListener("click", () => setTab(btn.getAttribute("data-tab") || "evento"));
  });

  function updateAtividadeRangeHint() {
    const opt = selectEvento.selectedOptions[0];
    if (!opt || !opt.value || !opt.dataset.inicio || !opt.dataset.fim) {
      hintAtividadeRange.hidden = true;
      hintAtividadeRange.textContent = "";
      return;
    }
    const a =
      typeof pimFormatDateTimeBr === "function" ? pimFormatDateTimeBr(opt.dataset.inicio) : opt.dataset.inicio;
    const b =
      typeof pimFormatDateTimeBr === "function" ? pimFormatDateTimeBr(opt.dataset.fim) : opt.dataset.fim;
    hintAtividadeRange.textContent = `Período do evento: ${a} — ${b}.`;
    hintAtividadeRange.hidden = false;
  }

  selectEvento.addEventListener("change", updateAtividadeRangeHint);

  async function refreshEventoSelect() {
    try {
      const list = await fetchEventosList();
      fillEventoSelect(selectEvento, list, "Selecione um evento…");
      updateAtividadeRangeHint();
    } catch {
      fillEventoSelect(selectEvento, [], "Não foi possível carregar eventos");
      hintAtividadeRange.hidden = true;
    }
  }

  function showFormMsg(el, text, kind) {
    el.textContent = text || "";
    el.classList.remove("show", "error", "success");
    if (!text) return;
    el.classList.add("show");
    if (kind === "error") el.classList.add("error");
    if (kind === "success") el.classList.add("success");
  }

  openBtn.addEventListener("click", async () => {
    showFormMsg(msgEvento, "", null);
    showFormMsg(msgAtividade, "", null);
    await refreshEventoSelect();
    dialog.showModal();
    dialog.querySelector("#adminEventoNome")?.focus();
  });

  btnClose.addEventListener("click", () => dialog.close());
  dialog.addEventListener("click", (ev) => {
    if (ev.target === dialog) dialog.close();
  });

  formEvento.addEventListener("submit", async (ev) => {
    ev.preventDefault();
    const nome = dialog.querySelector("#adminEventoNome").value.trim();
    const diRaw = dialog.querySelector("#adminEventoDataInicio").value;
    const dfRaw = dialog.querySelector("#adminEventoDataFim").value;
    if (!nome || !diRaw || !dfRaw) return;

    const diIso = adminDateToIsoForApi(diRaw);
    const dfIso = adminDateToIsoForApi(dfRaw);
    if (!diIso || !dfIso) {
      showFormMsg(msgEvento, adminMsgFormatoData(), "error");
      msgEvento.classList.add("show");
      return;
    }
    if (new Date(dfIso).getTime() < new Date(diIso).getTime()) {
      showFormMsg(msgEvento, "O término deve ser depois do início (ou no mesmo instante).", "error");
      msgEvento.classList.add("show");
      return;
    }
    showFormMsg(msgEvento, "Salvando…", null);
    msgEvento.classList.add("show");
    try {
      await apiPost("/eventos", {
        usuarioId: user.id,
        nome,
        dataInicio: diIso,
        dataFim: dfIso,
      });
      showFormMsg(msgEvento, "Evento criado com sucesso.", "success");
      formEvento.reset();
      await refreshEventoSelect();
      window.dispatchEvent(new CustomEvent("pim-eventos-mutated"));
    } catch (err) {
      showFormMsg(msgEvento, err.message || "Erro ao criar evento.", "error");
    }
  });

  formAtividade.addEventListener("submit", async (ev) => {
    ev.preventDefault();
    const eventoId = selectEvento.value.trim();
    const nome = dialog.querySelector("#adminAtividadeNome").value.trim();
    const diRaw = inputAtividadeDataInicio.value;
    const dfRaw = inputAtividadeDataFim.value;
    if (!eventoId || !nome || !diRaw || !dfRaw) return;

    const diIso = adminDateToIsoForApi(diRaw);
    const dfIso = adminDateToIsoForApi(dfRaw);
    if (!diIso || !dfIso) {
      showFormMsg(msgAtividade, adminMsgFormatoData(), "error");
      msgAtividade.classList.add("show");
      return;
    }
    if (new Date(dfIso).getTime() < new Date(diIso).getTime()) {
      showFormMsg(msgAtividade, "O término deve ser igual ou depois do início.", "error");
      msgAtividade.classList.add("show");
      return;
    }

    const opt = selectEvento.selectedOptions[0];
    if (opt?.dataset.inicio && opt?.dataset.fim) {
      const tDi = new Date(diIso).getTime();
      const tDf = new Date(dfIso).getTime();
      const t0 = new Date(opt.dataset.inicio).getTime();
      const t1 = new Date(opt.dataset.fim).getTime();
      if (Number.isNaN(t0) || Number.isNaN(t1)) {
        showFormMsg(
          msgAtividade,
          "Este evento não tem período válido na API. Crie outro evento ou corrija no banco.",
          "error"
        );
        msgAtividade.classList.add("show");
        return;
      }
      if (tDi < t0 || tDf > t1) {
        showFormMsg(
          msgAtividade,
          "Início e término da atividade precisam ficar dentro do período do evento.",
          "error"
        );
        msgAtividade.classList.add("show");
        return;
      }
    }

    showFormMsg(msgAtividade, "Salvando…", null);
    msgAtividade.classList.add("show");
    try {
      const descricao = dialog.querySelector("#adminAtividadeDescricao")?.value?.trim() || "";
      await apiPost("/atividades", {
        usuarioId: user.id,
        eventoId,
        nome,
        descricao: descricao || undefined,
        dataInicio: diIso,
        dataFim: dfIso,
      });
      showFormMsg(msgAtividade, "Atividade criada com sucesso.", "success");
      dialog.querySelector("#adminAtividadeNome").value = "";
      const taDesc = dialog.querySelector("#adminAtividadeDescricao");
      if (taDesc) taDesc.value = "";
      inputAtividadeDataInicio.value = "";
      inputAtividadeDataFim.value = "";
      await refreshEventoSelect();
      window.dispatchEvent(new CustomEvent("pim-atividades-mutated", { detail: { eventoId } }));
    } catch (err) {
      showFormMsg(msgAtividade, err.message || "Erro ao criar atividade.", "error");
    }
  });
}

if (document.readyState === "loading") {
  document.addEventListener("DOMContentLoaded", initAdminPanel);
} else {
  initAdminPanel();
}
