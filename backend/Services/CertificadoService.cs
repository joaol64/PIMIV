using System.Net;
using System.Text;
using Backend.DTOs;
using Backend.Models;
using Backend.Repositories;
using MongoDB.Driver;

namespace Backend.Services;

/// <summary>
/// Gera conteúdo de certificado e, no fluxo por usuário, calcula inscrições e persiste registro.
/// </summary>
public class CertificadoService
{
    private readonly UserRepository _userRepository;
    private readonly ParticipanteRepository _participanteRepository;
    private readonly InscricaoRepository _inscricaoRepository;
    private readonly AtividadeRepository _atividadeRepository;
    private readonly EventoRepository _eventoRepository;
    private readonly CertificadoRepository _certificadoRepository;

    public CertificadoService(
        UserRepository userRepository,
        ParticipanteRepository participanteRepository,
        InscricaoRepository inscricaoRepository,
        AtividadeRepository atividadeRepository,
        EventoRepository eventoRepository,
        CertificadoRepository certificadoRepository)
    {
        _userRepository = userRepository;
        _participanteRepository = participanteRepository;
        _inscricaoRepository = inscricaoRepository;
        _atividadeRepository = atividadeRepository;
        _eventoRepository = eventoRepository;
        _certificadoRepository = certificadoRepository;
    }

    /// <summary>
    /// Monta o certificado legado (um evento). Use <paramref name="comoHtml"/> true para HTML, false para texto puro.
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
            return comoHtml
                ? "<!DOCTYPE html><html><body><p>Erro ao gerar certificado.</p></body></html>"
                : "Erro ao gerar certificado.";
        }
    }

    /// <summary>Certificado com totais reais de eventos e atividades inscritas.</summary>
    public string GerarCertificadoPorInscricoes(
        string nomeParticipante,
        int totalEventos,
        int totalAtividades,
        DateTime dataEmissao,
        bool comoHtml)
    {
        var participante = string.IsNullOrWhiteSpace(nomeParticipante) ? "—" : nomeParticipante.Trim();
        var dataFormatada = dataEmissao.ToString("dd/MM/yyyy");
        var textoEventos = totalEventos == 1 ? "1 evento distinto" : $"{totalEventos} eventos distintos";
        var textoAtividades = totalAtividades == 1 ? "1 atividade" : $"{totalAtividades} atividades";

        try
        {
            if (comoHtml)
            {
                var pHtml = WebUtility.HtmlEncode(participante);
                var dHtml = WebUtility.HtmlEncode(dataFormatada);
                var evHtml = WebUtility.HtmlEncode(textoEventos);
                var atHtml = WebUtility.HtmlEncode(textoAtividades);

                return $@"<!DOCTYPE html>
<html lang=""pt-BR"">
<head>
  <meta charset=""utf-8"" />
  <meta name=""viewport"" content=""width=device-width, initial-scale=1"" />
  <title>Certificado de participação</title>
  <style>
    body {{ font-family: Georgia, serif; max-width: 40rem; margin: 3rem auto; padding: 1rem; line-height: 1.6; }}
    h1 {{ font-size: 1.5rem; text-align: center; }}
    .corpo {{ margin-top: 2rem; text-align: justify; }}
    .destaque {{ margin-top: 1rem; text-align: center; font-size: 1.05rem; }}
    .data {{ margin-top: 2rem; font-size: 0.95rem; text-align: center; }}
  </style>
</head>
<body>
  <h1>Certificado de participação</h1>
  <p class=""corpo"">
    Certificamos que <strong>{pHtml}</strong> possui inscrições confirmadas na plataforma,
    totalizando participação em <strong>{evHtml}</strong> e em <strong>{atHtml}</strong>,
    com dedicação e interesse na programação acadêmica proposta.
  </p>
  <p class=""data"">Emitido em {dHtml}</p>
</body>
</html>";
            }

            var sb = new StringBuilder();
            sb.AppendLine("CERTIFICADO DE PARTICIPACAO");
            sb.AppendLine();
            sb.AppendLine(
                $"Certificamos que {participante} possui inscrições confirmadas na plataforma, " +
                $"totalizando participacao em {textoEventos} e em {textoAtividades}, " +
                "com dedicacao e interesse na programacao academica proposta.");
            sb.AppendLine();
            sb.AppendLine($"Emitido em {dataFormatada}");
            return sb.ToString();
        }
        catch (Exception)
        {
            return comoHtml
                ? "<!DOCTYPE html><html><body><p>Erro ao gerar certificado.</p></body></html>"
                : "Erro ao gerar certificado.";
        }
    }

    /// <summary>
    /// Só emite se existir ao menos uma inscrição vinculada ao e-mail da conta.
    /// </summary>
    public async Task<(bool Ok, string? ErrorMessage, CertificadoGeradoResponse? Resposta)> EmitirPorUsuarioAsync(
        string usuarioId,
        bool comoHtml)
    {
        if (string.IsNullOrWhiteSpace(usuarioId))
        {
            return (false, "UsuarioId é obrigatório.", null);
        }

        try
        {
            var usuario = await _userRepository.GetByIdAsync(usuarioId.Trim());
            if (usuario is null)
            {
                return (false, "Usuário não encontrado.", null);
            }

            var participante = await _participanteRepository.GetByEmailAsync(usuario.Email);
            if (participante is null || string.IsNullOrEmpty(participante.Id))
            {
                return (
                    false,
                    "Não há inscrições registradas para esta conta. Inscreva-se em pelo menos uma atividade para emitir o certificado.",
                    null);
            }

            var inscricoes = await _inscricaoRepository.ListarPorParticipanteAsync(participante.Id);
            if (inscricoes.Count == 0)
            {
                return (
                    false,
                    "Não há inscrições registradas para esta conta. Inscreva-se em pelo menos uma atividade para emitir o certificado.",
                    null);
            }

            var atividadeIds = inscricoes
                .Select(i => i.AtividadeId)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            var totalAtividades = atividadeIds.Count;
            if (totalAtividades == 0)
            {
                return (
                    false,
                    "Não há inscrições registradas para esta conta. Inscreva-se em pelo menos uma atividade para emitir o certificado.",
                    null);
            }

            var todasAtividades = await _atividadeRepository.ListarTodasAsync();
            var mapaAtividadeEvento = todasAtividades
                .Where(a => !string.IsNullOrEmpty(a.Id))
                .ToDictionary(a => a.Id!, a => a.EventoId);

            var eventoIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var aid in atividadeIds)
            {
                if (mapaAtividadeEvento.TryGetValue(aid, out var eid) && !string.IsNullOrWhiteSpace(eid))
                {
                    eventoIds.Add(eid.Trim());
                }
            }

            var totalEventos = eventoIds.Count;
            if (totalEventos == 0)
            {
                return (
                    false,
                    "Não foi possível associar suas inscrições a eventos. Entre em contato com o suporte.",
                    null);
            }

            var dataEmissao = DateTime.UtcNow.Date;
            var conteudo = GerarCertificadoPorInscricoes(
                participante.Nome,
                totalEventos,
                totalAtividades,
                dataEmissao,
                comoHtml);

            var eventoRepresentativo = eventoIds.OrderBy(id => id, StringComparer.Ordinal).First();
            var evento = await _eventoRepository.GetByIdAsync(eventoRepresentativo);
            var nomeEventoResumo = evento?.Nome?.Trim() is { Length: > 0 } n
                ? n
                : "Participação consolidada";

            var certificado = new Certificado(
                participante.Id!,
                eventoRepresentativo,
                nomeEventoResumo,
                totalEventos,
                totalAtividades);

            await _certificadoRepository.CreateAsync(certificado);

            var resposta = new CertificadoGeradoResponse
            {
                Id = certificado.Id,
                ParticipanteId = certificado.ParticipanteId,
                EventoId = certificado.EventoId,
                NomeEvento = certificado.NomeEvento,
                NomeParticipante = participante.Nome,
                TotalEventos = totalEventos,
                TotalAtividades = totalAtividades,
                Conteudo = conteudo,
            };

            return (true, null, resposta);
        }
        catch (MongoException)
        {
            return (false, "Erro ao acessar o banco de dados.", null);
        }
        catch (Exception)
        {
            return (false, "Erro inesperado ao emitir certificado.", null);
        }
    }
}
