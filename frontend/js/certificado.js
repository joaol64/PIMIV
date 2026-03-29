// certificado.html — exibe certificado simulado e “download” mockado (.txt).

const elNome = document.getElementById("certNomeParticipante");
const elEvento = document.getElementById("certNomeEvento");
const elData = document.getElementById("certData");
const btnBaixar = document.getElementById("btnBaixar");

function getQueryParam(name) {
  const params = new URLSearchParams(window.location.search);
  return params.get(name)?.trim() || "";
}

function montarTextoCertificado(nomeParticipante, nomeEvento, dataStr) {
  return [
    "CERTIFICADO DE PARTICIPACAO",
    "",
    `Certificamos que ${nomeParticipante} participou do evento ${nomeEvento},`,
    "com dedicacao e interesse na programacao academica proposta.",
    "",
    `Data: ${dataStr}`,
    "",
    "Documento gerado para fins de demonstracao (PIM).",
  ].join("\n");
}

const hoje = new Date().toLocaleDateString("pt-BR", {
  day: "2-digit",
  month: "long",
  year: "numeric",
});

const user = typeof getUserFromLocalStorage === "function" ? getUserFromLocalStorage() : null;
const nomeParticipante =
  getQueryParam("participante") || (user && user.nome) || "Participante";
const nomeEvento = getQueryParam("evento") || "Evento acadêmico";

elNome.textContent = nomeParticipante;
elEvento.textContent = nomeEvento;
elData.textContent = `Emitido em ${hoje}.`;

btnBaixar.addEventListener("click", () => {
  const texto = montarTextoCertificado(nomeParticipante, nomeEvento, hoje);
  const blob = new Blob([texto], { type: "text/plain;charset=utf-8" });
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
