// Parsing e formatação de datas vindas da API (.NET / Mongo) e de inputs datetime-local.

/**
 * Converte valor da API em Date válido ou null.
 * Aceita ISO com ou sem Z, e objeto Mongo estendido { $date: "..." }.
 */
function pimParseApiDate(valor) {
  if (valor == null || valor === "") return null;
  if (typeof valor === "object" && valor !== null) {
    if (typeof valor.$date === "string") return pimParseApiDate(valor.$date);
    if (typeof valor.$date === "number") {
      const d = new Date(valor.$date);
      return Number.isNaN(d.getTime()) ? null : d;
    }
  }
  const s = String(valor).trim();
  if (!s) return null;
  const d = new Date(s);
  if (Number.isNaN(d.getTime())) return null;
  const y = d.getFullYear();
  if (y < 100) return null;
  return d;
}

/** Data e hora em pt-BR (use Intl — evita bug de year "1" com toLocaleDateString + hour). */
function pimFormatDateTimeBr(valor) {
  const d = pimParseApiDate(valor);
  if (!d) return "—";
  return new Intl.DateTimeFormat("pt-BR", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  }).format(d);
}

/** Valor para atributo min/max de <input type="datetime-local"> (horário local). */
function pimToDatetimeLocalValue(valor) {
  const d = pimParseApiDate(valor);
  if (!d) return "";
  const pad = (n) => String(n).padStart(2, "0");
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
}

/** ISO UTC a partir do valor do datetime-local (interpretação local do navegador). */
function pimLocalDatetimeInputToIsoUtc(inputValue) {
  const d = new Date(inputValue);
  if (Number.isNaN(d.getTime())) return null;
  return d.toISOString();
}
