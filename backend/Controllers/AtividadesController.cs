using Backend.DTOs;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AtividadesController : ControllerBase
{
    private readonly AtividadeService _atividadeService;

    public AtividadesController(AtividadeService atividadeService)
    {
        _atividadeService = atividadeService;
    }

    /// <summary>Lista todas as atividades (ordenadas por data no serviço).</summary>
    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var result = await _atividadeService.ListarTodosAsync();
        if (!result.Ok)
        {
            return StatusCode(500, new ErrorResponse { Message = result.ErrorMessage ?? "Erro" });
        }

        return Ok(result.Atividades);
    }

    /// <summary>Obtém uma atividade pelo id.</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> Obter(string id)
    {
        var result = await _atividadeService.ObterPorIdAsync(id);
        if (!result.Ok)
        {
            if (result.ErrorMessage == "Atividade não encontrada.")
            {
                return NotFound(new ErrorResponse { Message = result.ErrorMessage });
            }

            if (result.ErrorMessage == "Id da atividade é obrigatório.")
            {
                return BadRequest(new ErrorResponse { Message = result.ErrorMessage });
            }

            return StatusCode(500, new ErrorResponse { Message = result.ErrorMessage ?? "Erro" });
        }

        return Ok(result.Atividade);
    }

    /// <summary>Cria atividade — somente administrador (mesma regra dos eventos).</summary>
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CreateAtividadeRequest request)
    {
        var dataInicio = !string.IsNullOrWhiteSpace(request.DataInicio)
            ? request.DataInicio
            : request.Data ?? string.Empty;
        var dataFim = !string.IsNullOrWhiteSpace(request.DataFim) ? request.DataFim : dataInicio;

        var result = await _atividadeService.CriarAsync(
            request.UsuarioId,
            request.Nome,
            dataInicio,
            dataFim,
            request.EventoId,
            request.Descricao);

        if (!result.Ok)
        {
            if (result.ErrorMessage == "Usuário não encontrado." || result.ErrorMessage == "Evento não encontrado.")
            {
                return NotFound(new ErrorResponse { Message = result.ErrorMessage });
            }

            if (result.ErrorMessage == "Apenas administradores podem criar atividades.")
            {
                return StatusCode(403, new ErrorResponse { Message = result.ErrorMessage });
            }

            if (result.ErrorMessage == "Erro ao acessar o banco de dados." ||
                result.ErrorMessage == "Erro inesperado ao criar atividade.")
            {
                return StatusCode(500, new ErrorResponse { Message = result.ErrorMessage });
            }

            return BadRequest(new ErrorResponse { Message = result.ErrorMessage ?? "Erro" });
        }

        return Ok(result.Atividade);
    }
}
