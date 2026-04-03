using Backend.DTOs;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InscricoesController : ControllerBase
{
    private readonly InscricaoService _inscricaoService;

    public InscricoesController(InscricaoService inscricaoService)
    {
        _inscricaoService = inscricaoService;
    }

    /// <summary>Total de inscrições na atividade (público — útil para exibir vagas/participantes).</summary>
    [HttpGet("contagem/atividade/{atividadeId}")]
    public async Task<IActionResult> ContarPorAtividade(string atividadeId)
    {
        var result = await _inscricaoService.ContarPorAtividadeAsync(atividadeId);
        if (!result.Ok)
        {
            if (result.ErrorMessage == "Atividade não encontrada.")
            {
                return NotFound(new ErrorResponse { Message = result.ErrorMessage });
            }

            if (result.ErrorMessage == "AtividadeId é obrigatório.")
            {
                return BadRequest(new ErrorResponse { Message = result.ErrorMessage });
            }

            return StatusCode(500, new ErrorResponse { Message = result.ErrorMessage ?? "Erro" });
        }

        return Ok(new ContagemInscricaoAtividadeResponse { Total = result.Total });
    }

    /// <summary>
    /// Total de inscrições no evento (soma em todas as atividades) e quantos participantes distintos
    /// têm ao menos uma inscrição em alguma atividade desse evento.
    /// </summary>
    [HttpGet("contagem/evento/{eventoId}")]
    public async Task<IActionResult> ContarPorEvento(string eventoId)
    {
        var result = await _inscricaoService.ContarPorEventoAsync(eventoId);
        if (!result.Ok)
        {
            if (result.ErrorMessage == "Evento não encontrado.")
            {
                return NotFound(new ErrorResponse { Message = result.ErrorMessage });
            }

            if (result.ErrorMessage == "EventoId é obrigatório.")
            {
                return BadRequest(new ErrorResponse { Message = result.ErrorMessage });
            }

            return StatusCode(500, new ErrorResponse { Message = result.ErrorMessage ?? "Erro" });
        }

        return Ok(new ContagemInscricaoEventoResponse
        {
            TotalInscricoes = result.TotalInscricoes,
            ParticipantesDistintos = result.ParticipantesDistintos,
        });
    }

    /// <summary>Lista todas as inscrições.</summary>
    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var result = await _inscricaoService.ListarTodosAsync();
        if (!result.Ok)
        {
            return StatusCode(500, new ErrorResponse { Message = result.ErrorMessage ?? "Erro" });
        }

        return Ok(result.Inscricoes);
    }

    /// <summary>Obtém uma inscrição pelo id.</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> Obter(string id)
    {
        var result = await _inscricaoService.ObterPorIdAsync(id);
        if (!result.Ok)
        {
            if (result.ErrorMessage == "Inscrição não encontrada.")
            {
                return NotFound(new ErrorResponse { Message = result.ErrorMessage });
            }

            if (result.ErrorMessage == "Id da inscrição é obrigatório.")
            {
                return BadRequest(new ErrorResponse { Message = result.ErrorMessage });
            }

            return StatusCode(500, new ErrorResponse { Message = result.ErrorMessage ?? "Erro" });
        }

        return Ok(result.Inscricao);
    }

    /// <summary>Inscreve o usuário comum (id retornado no login) numa atividade.</summary>
    [HttpPost]
    public async Task<IActionResult> Inscrever([FromBody] InscricaoRequest request)
    {
        var result = await _inscricaoService.InscreverAsync(request.UsuarioId, request.AtividadeId);

        if (!result.Ok)
        {
            if (result.ErrorMessage == "Usuário não encontrado." || result.ErrorMessage == "Atividade não encontrada.")
            {
                return NotFound(new ErrorResponse { Message = result.ErrorMessage });
            }

            if (result.ErrorMessage == "Você já está inscrito nesta atividade.")
            {
                return Conflict(new ErrorResponse { Message = result.ErrorMessage });
            }

            if (result.ErrorMessage == "Erro ao acessar o banco de dados." ||
                result.ErrorMessage == "Erro inesperado ao inscrever.")
            {
                return StatusCode(500, new ErrorResponse { Message = result.ErrorMessage });
            }

            return BadRequest(new ErrorResponse { Message = result.ErrorMessage ?? "Erro" });
        }

        return Ok(result.Inscricao);
    }
}
