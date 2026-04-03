using Backend.DTOs;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventosController : ControllerBase
{
    private readonly EventoService _eventoService;
    private readonly AtividadeService _atividadeService;

    public EventosController(EventoService eventoService, AtividadeService atividadeService)
    {
        _eventoService = eventoService;
        _atividadeService = atividadeService;
    }

    /// <summary>Lista eventos ordenados por data (qualquer pessoa — usuário comum pode ver).</summary>
    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var result = await _eventoService.ListarTodosAsync();
        if (!result.Ok)
        {
            return StatusCode(500, new ErrorResponse { Message = result.ErrorMessage ?? "Erro" });
        }

        return Ok(result.Eventos);
    }

    /// <summary>Retorna um evento pelo id.</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> Obter(string id)
    {
        var result = await _eventoService.ObterPorIdAsync(id);
        if (!result.Ok)
        {
            if (result.ErrorMessage == "Evento não encontrado.")
            {
                return NotFound(new ErrorResponse { Message = result.ErrorMessage });
            }

            if (result.ErrorMessage == "Id do evento é obrigatório.")
            {
                return BadRequest(new ErrorResponse { Message = result.ErrorMessage });
            }

            return StatusCode(500, new ErrorResponse { Message = result.ErrorMessage ?? "Erro" });
        }

        return Ok(result.Evento);
    }

    /// <summary>Cria evento — exige <see cref="CreateEventoRequest.UsuarioId"/> de administrador.</summary>
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CreateEventoRequest request)
    {
        var result = await _eventoService.CriarAsync(
            request.UsuarioId,
            request.Nome,
            request.DataInicio,
            request.DataFim);

        if (!result.Ok)
        {
            if (result.ErrorMessage == "Usuário não encontrado.")
            {
                return NotFound(new ErrorResponse { Message = result.ErrorMessage });
            }

            if (result.ErrorMessage == "Apenas administradores podem criar eventos.")
            {
                return StatusCode(403, new ErrorResponse { Message = result.ErrorMessage });
            }

            if (result.ErrorMessage == "Erro ao acessar o banco de dados." ||
                result.ErrorMessage == "Erro inesperado ao criar evento.")
            {
                return StatusCode(500, new ErrorResponse { Message = result.ErrorMessage });
            }

            return BadRequest(new ErrorResponse { Message = result.ErrorMessage ?? "Erro" });
        }

        return Ok(result.Evento);
    }

    /// <summary>Atividades de um evento (filtradas e ordenadas no serviço com LINQ).</summary>
    [HttpGet("{eventoId}/atividades")]
    public async Task<IActionResult> ListarAtividades(string eventoId)
    {
        var result = await _atividadeService.ListarPorEventoAsync(eventoId);
        if (!result.Ok)
        {
            if (result.ErrorMessage == "EventoId é obrigatório.")
            {
                return BadRequest(new ErrorResponse { Message = result.ErrorMessage });
            }

            return StatusCode(500, new ErrorResponse { Message = result.ErrorMessage ?? "Erro" });
        }

        return Ok(result.Atividades);
    }
}
