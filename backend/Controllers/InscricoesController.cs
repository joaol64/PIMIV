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
