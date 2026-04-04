// certificado.html — emite certificado via API com totais reais de eventos e atividades inscritos.

const elNome = document.getElementById("certNomeParticipante");
const elResumo = document.getElementById("certResumoInscricoes");
const elData = document.getElementById("certData");
const btnBaixar = document.getElementById("btnBaixar");
const certStatus = document.getElementById("certStatus");
const certWrapper = document.getElementById("certificadoWrapper");

function textoResumoInscricoes(totalEventos, totalAtividades) {
  const e = Number(totalEventos) || 0;
  const a = Number(totalAtividades) || 0;
  const parteEventos = e === 1 ? "1 evento distinto" : `${e} eventos distintos`;
  const parteAtividades = a === 1 ? "1 atividade" : `${a} atividades`;
  return `${parteEventos} e ${parteAtividades}`;
}

function showStatus(text, kind) {
  certStatus.hidden = false;
  certStatus.textContent = text;
  certStatus.classList.remove("error", "success", "show");
  certStatus.classList.add("show");
  if (kind === "error") certStatus.classList.add("error");
  if (kind === "success") certStatus.classList.add("success");
  if (kind === "loading") certStatus.classList.remove("error", "success");
}

function hideStatus() {
  certStatus.hidden = true;
  certStatus.textContent = "";
  certStatus.classList.remove("show", "error", "success");
}

function pick(obj, camel, pascal) {
  if (!obj || typeof obj !== "object") return undefined;
  if (obj[camel] !== undefined && obj[camel] !== null) return obj[camel];
  if (obj[pascal] !== undefined && obj[pascal] !== null) return obj[pascal];
  return undefined;
}

let textoArquivoDownload = "";

async function carregarCertificado() {
  hideStatus();
  certWrapper.hidden = true;
  btnBaixar.disabled = true;
  textoArquivoDownload = "";

  const user = typeof getUserFromLocalStorage === "function" ? getUserFromLocalStorage() : null;
  if (!user || !user.id) {
    showStatus("Faça login para emitir seu certificado. Abra Entrar no menu ou em home.html.", "error");
    return;
  }

  showStatus("Gerando certificado…", "loading");

  try {
    const raw = await apiPost("/certificados/emitir-resumo", {
      usuarioId: user.id,
      comoHtml: false,
    });

    const nome = pick(raw, "nomeParticipante", "NomeParticipante") || user.nome || "Participante";
    const totalEv = pick(raw, "totalEventos", "TotalEventos");
    const totalAt = pick(raw, "totalAtividades", "TotalAtividades");
    const conteudo = pick(raw, "conteudo", "Conteudo") || "";

    elNome.textContent = nome;
    elResumo.textContent = textoResumoInscricoes(totalEv, totalAt);

    const hoje = new Date().toLocaleDateString("pt-BR", {
      day: "2-digit",
      month: "long",
      year: "numeric",
    });
    elData.textContent = `Emitido em ${hoje}.`;

    textoArquivoDownload = typeof conteudo === "string" && conteudo.trim() ? conteudo : montarTextoFallback(nome, totalEv, totalAt, hoje);

    certWrapper.hidden = false;
    hideStatus();
    btnBaixar.disabled = false;
  } catch (err) {
    showStatus(err.message || "Não foi possível emitir o certificado.", "error");
    certWrapper.hidden = true;
    btnBaixar.disabled = true;
  }
}

function montarTextoFallback(nomeParticipante, totalEventos, totalAtividades, dataStr) {
  const resumo = textoResumoInscricoes(totalEventos, totalAtividades);
  return [
    "CERTIFICADO DE PARTICIPACAO",
    "",
    `Certificamos que ${nomeParticipante} possui inscricoes confirmadas na plataforma,`,
    `totalizando participacao em ${resumo}, com dedicacao e interesse na programacao academica proposta.`,
    "",
    `Emitido em ${dataStr}`,
    "",
    "Documento gerado a partir dos dados da API (PIM).",
  ].join("\n");
}

btnBaixar.addEventListener("click", () => {
  if (!textoArquivoDownload) return;
  const blob = new Blob([textoArquivoDownload], { type: "text/plain;charset=utf-8" });
  const url = URL.createObjectURL(blob);
  const a = document.createElement("a");
  a.href = url;
  a.download = "certificado-participacao.txt";
  a.rel = "noopener";
  document.body.appendChild(a);
  a.click();
  a.remove();
  URL.revokeObjectURL(url);
});

carregarCertificado();
