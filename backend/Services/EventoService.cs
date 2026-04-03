using Backend.Helpers;
using Backend.Models;
using Backend.Repositories;
using MongoDB.Driver;

namespace Backend.Services;

/// <summary>
/// Regras de negócio para eventos. Somente usuário com <see cref="TipoUsuario.Administrador"/> pode criar.
/// </summary>
public class EventoService
{
    private readonly EventoRepository _eventoRepository;
    private readonly UserRepository _userRepository;

    public EventoService(EventoRepository eventoRepository, UserRepository userRepository)
    {
        _eventoRepository = eventoRepository;
        _userRepository = userRepository;
    }

    /// <summary>Cria evento se <paramref name="usuarioAdministradorId"/> for de um admin.</summary>
    public async Task<(bool Ok, string? ErrorMessage, Evento? Evento)> CriarAsync(
        string usuarioAdministradorId,
        string nome,
        string dataInicioStr,
        string dataFimStr)
    {
        if (string.IsNullOrWhiteSpace(usuarioAdministradorId))
        {
            return (false, "Informe o id do usuário administrador.", null);
        }

        if (string.IsNullOrWhiteSpace(nome))
        {
            return (false, "Nome do evento é obrigatório.", null);
        }

        if (!ApiDateParsing.TryParseUtc(dataInicioStr, out var inicioUtc, out var errIni))
        {
            return (false, $"Data de início inválida: {errIni}", null);
        }

        if (!ApiDateParsing.TryParseUtc(dataFimStr, out var fimUtc, out var errFim))
        {
            return (false, $"Data de término inválida: {errFim}", null);
        }

        if (fimUtc < inicioUtc)
        {
            return (false, "A data de término deve ser posterior ou igual à data de início.", null);
        }

        try
        {
            var usuario = await _userRepository.GetByIdAsync(usuarioAdministradorId.Trim());
            if (usuario is null)
            {
                return (false, "Usuário não encontrado.", null);
            }

            if (usuario.TipoUsuario != TipoUsuario.Administrador)
            {
                return (false, "Apenas administradores podem criar eventos.", null);
            }

            var evento = new Evento(nome.Trim(), inicioUtc, fimUtc);
            await _eventoRepository.CreateAsync(evento);
            return (true, null, evento);
        }
        catch (MongoException)
        {
            return (false, "Erro ao acessar o banco de dados.", null);
        }
        catch (Exception)
        {
            return (false, "Erro inesperado ao criar evento.", null);
        }
    }

    /// <summary>Lista todos os eventos ordenados pelo início do período (LINQ).</summary>
    public async Task<(bool Ok, string? ErrorMessage, List<Evento> Eventos)> ListarTodosAsync()
    {
        try
        {
            var todos = await _eventoRepository.ListarTodosAsync();
            var ordenados = todos.OrderBy(e => e.DataInicioEfetiva).ToList();
            return (true, null, ordenados);
        }
        catch (MongoException)
        {
            return (false, "Erro ao acessar o banco de dados.", new List<Evento>());
        }
        catch (Exception)
        {
            return (false, "Erro inesperado ao listar eventos.", new List<Evento>());
        }
    }

    /// <summary>Busca um evento pelo id (MongoDB).</summary>
    public async Task<(bool Ok, string? ErrorMessage, Evento? Evento)> ObterPorIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return (false, "Id do evento é obrigatório.", null);
        }

        try
        {
            var evento = await _eventoRepository.GetByIdAsync(id.Trim());
            if (evento is null)
            {
                return (false, "Evento não encontrado.", null);
            }

            return (true, null, evento);
        }
        catch (MongoException)
        {
            return (false, "Erro ao acessar o banco de dados.", null);
        }
        catch (Exception)
        {
            return (false, "Erro inesperado ao buscar evento.", null);
        }
    }
}
