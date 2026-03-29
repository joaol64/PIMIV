using System.Net;
using System.Text;

namespace Backend.Services;

/// <summary>
/// Gera o conteúdo de um certificado simples (texto ou HTML) com dados do participante e do evento.
/// Não grava arquivo nem banco — apenas monta a string para você exibir, salvar ou enviar.
/// </summary>
public class CertificadoService
{
    /// <summary>
    /// Monta o certificado. Use <paramref name="comoHtml"/> true para HTML, false para texto puro.
    /// Nomes são escapados no HTML para evitar injeção de marcação.
    /// </summary>
    public string GerarCertificado(string nomeParticipante, string nomeEvento, DateTime data, bool comoHtml = true)
    {
        var participante = string.IsNullOrWhiteSpace(nomeParticipante) ? "—" : nomeParticipante.Trim();
        var evento = string.IsNullOrWhiteSpace(nomeEvento) ? "—" : nomeEvento.Trim();
        var dataFormatada = data.ToString("dd/MM/yyyy");

        try
        {
            if (comoHtml)
            {
                var pHtml = WebUtility.HtmlEncode(participante);
                var eHtml = WebUtility.HtmlEncode(evento);
                var dHtml = WebUtility.HtmlEncode(dataFormatada);

                // Cadeia interpolada clássica: chaves do CSS são duplicadas {{ e }}.
                return $@"<!DOCTYPE html>
<html lang=""pt-BR"">
<head>
  <meta charset=""utf-8"" />
  <meta name=""viewport"" content=""width=device-width, initial-scale=1"" />
  <title>Certificado — {eHtml}</title>
  <style>
    body {{ font-family: Georgia, serif; max-width: 40rem; margin: 3rem auto; padding: 1rem; line-height: 1.6; }}
    h1 {{ font-size: 1.5rem; text-align: center; }}
    .corpo {{ margin-top: 2rem; text-align: center; }}
    .data {{ margin-top: 2rem; font-size: 0.95rem; }}
  </style>
</head>
<body>
  <h1>Certificado de participação</h1>
  <p class=""corpo"">
    Certificamos que <strong>{pHtml}</strong> participou do evento
    <strong>{eHtml}</strong>.
  </p>
  <p class=""data"">Data: {dHtml}</p>
</body>
</html>";
            }

            // Texto simples, sem tags (útil para e-mail ou arquivo .txt).
            var sb = new StringBuilder();
            sb.AppendLine("CERTIFICADO DE PARTICIPAÇÃO");
            sb.AppendLine();
            sb.AppendLine($"Certificamos que {participante} participou do evento {evento}.");
            sb.AppendLine();
            sb.AppendLine($"Data: {dataFormatada}");
            return sb.ToString();
        }
        catch (Exception)
        {
            // Falha inesperada (ex.: encoding): retorno mínimo para não quebrar o fluxo da API.
            return comoHtml
                ? "<!DOCTYPE html><html><body><p>Erro ao gerar certificado.</p></body></html>"
                : "Erro ao gerar certificado.";
        }
    }
}
