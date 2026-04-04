using Backend.DTOs;
using Backend.Models;
using Backend.Repositories;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CertificadosController : ControllerBase
{
    private readonly CertificadoRepository _certificadoRepository;
    private readonly ParticipanteRepository _participanteRepository;
    private readonly EventoRepository _eventoRepository;
    private readonly CertificadoService _certificadoService;

    public CertificadosController(
        CertificadoRepository certificadoRepository,
        ParticipanteRepository participanteRepository,
        EventoRepository eventoRepository,
        CertificadoService certificadoService)
    {
        _certificadoRepository = certificadoRepository;
        _participanteRepository = participanteRepository;
        _eventoRepository = eventoRepository;
        _certificadoService = certificadoService;
    }

    /// <summary>Lista todos os certificados registrados.</summary>
    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        try
        {
            var lista = await _certificadoRepository.ListarTodosAsync();
            var ordenada = lista.OrderBy(c => c.NomeEvento).ThenBy(c => c.Id).ToList();
            return Ok(ordenada);
        }
        catch (MongoException)
        {
            return StatusCode(500, new ErrorResponse { Message = "Erro ao acessar o banco de dados." });
        }
        catch (Exception)
        {
            return StatusCode(500, new ErrorResponse { Message = "Erro inesperado ao listar certificados." });
        }
    }

    /// <summary>Obtém um certificado pelo id.</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> Obter(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest(new ErrorResponse { Message = "Id é obrigatório." });
        }

        try
        {
            var cert = await _certificadoRepository.GetByIdAsync(id.Trim());
            if (cert is null)
            {
                return NotFound(new ErrorResponse { Message = "Certificado não encontrado." });
            }

            return Ok(cert);
        }
        catch (MongoException)
        {
            return StatusCode(500, new ErrorResponse { Message = "Erro ao acessar o banco de dados." });
        }
        catch (Exception)
        {
            return StatusCode(500, new ErrorResponse { Message = "Erro inesperado ao buscar certificado." });
        }
    }

    /// <summary>Gera o texto/HTML do certificado, grava registro no MongoDB e devolve o conteúdo.</summary>
    [HttpPost]
    public async Task<IActionResult> Gerar([FromBody] GerarCertificadoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ParticipanteId) || string.IsNullOrWhiteSpace(request.EventoId))
        {
            return BadRequest(new ErrorResponse { Message = "ParticipanteId e EventoId são obrigatórios." });
        }

        try
        {
            var participante = await _participanteRepository.GetByIdAsync(request.ParticipanteId.Trim());
            if (participante is null)
            {
                return NotFound(new ErrorResponse { Message = "Participante não encontrado." });
            }

            var evento = await _eventoRepository.GetByIdAsync(request.EventoId.Trim());
            if (evento is null)
            {
                return NotFound(new ErrorResponse { Message = "Evento não encontrado." });
            }

            var conteudo = _certificadoService.GerarCertificado(
                participante.Nome,
                evento.Nome,
                evento.DataInicioEfetiva,
                request.ComoHtml);

            var certificado = new Certificado(
                participante.Id ?? string.Empty,
                evento.Id ?? string.Empty,
                evento.Nome);

            await _certificadoRepository.CreateAsync(certificado);

            var resposta = new CertificadoGeradoResponse
            {
                Id = certificado.Id,
                ParticipanteId = certificado.ParticipanteId,
                EventoId = certificado.EventoId,
                NomeEvento = certificado.NomeEvento,
                NomeParticipante = participante.Nome,
                TotalEventos = 1,
                TotalAtividades = 1,
                Conteudo = conteudo
            };

            return Ok(resposta);
        }
        catch (MongoException)
        {
            return StatusCode(500, new ErrorResponse { Message = "Erro ao acessar o banco de dados." });
        }
        catch (Exception)
        {
            return StatusCode(500, new ErrorResponse { Message = "Erro inesperado ao gerar certificado." });
        }
    }

    /// <summary>
    /// Emite certificado com totais reais de eventos e atividades inscritos pela conta.
    /// Sem inscrições → 400.
    /// </summary>
    [HttpPost("emitir-resumo")]
    public async Task<IActionResult> EmitirResumo([FromBody] EmitirCertificadoUsuarioRequest request)
    {
        var result = await _certificadoService.EmitirPorUsuarioAsync(
            request.UsuarioId,
            request.ComoHtml);

        if (!result.Ok)
        {
            if (result.ErrorMessage == "Usuário não encontrado.")
            {
                return NotFound(new ErrorResponse { Message = result.ErrorMessage });
            }

            if (result.ErrorMessage == "UsuarioId é obrigatório." ||
                result.ErrorMessage?.Contains("Não há inscrições", StringComparison.Ordinal) == true ||
                result.ErrorMessage?.Contains("Não foi possível associar", StringComparison.Ordinal) == true)
            {
                return BadRequest(new ErrorResponse { Message = result.ErrorMessage });
            }

            if (result.ErrorMessage == "Erro ao acessar o banco de dados." ||
                result.ErrorMessage == "Erro inesperado ao emitir certificado.")
            {
                return StatusCode(500, new ErrorResponse { Message = result.ErrorMessage });
            }

            return BadRequest(new ErrorResponse { Message = result.ErrorMessage ?? "Erro" });
        }

        return Ok(result.Resposta);
    }
}
