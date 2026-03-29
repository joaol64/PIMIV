using Backend.Models;
using Backend.Repositories;
using MongoDB.Driver;

namespace Backend.Services;

/// <summary>Cria atividades (somente admin) e lista por evento com LINQ.</summary>
public class AtividadeService
{
    private readonly AtividadeRepository _atividadeRepository;
    private readonly EventoRepository _eventoRepository;
    private readonly UserRepository _userRepository;

    public AtividadeService(
        AtividadeRepository atividadeRepository,
        EventoRepository eventoRepository,
        UserRepository userRepository)
    {
        _atividadeRepository = atividadeRepository;
        _eventoRepository = eventoRepository;
        _userRepository = userRepository;
    }

    public async Task<(bool Ok, string? ErrorMessage, Atividade? Atividade)> CriarAsync(
        string usuarioAdministradorId,
        string nome,
        DateTime data,
        string eventoId)
    {
        if (string.IsNullOrWhiteSpace(usuarioAdministradorId))
        {
            return (false, "Informe o id do usuário administrador.", null);
        }

        if (string.IsNullOrWhiteSpace(nome))
        {
            return (false, "Nome da atividade é obrigatório.", null);
        }

        if (string.IsNullOrWhiteSpace(eventoId))
        {
            return (false, "EventoId é obrigatório.", null);
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
                return (false, "Apenas administradores podem criar atividades.", null);
            }

            var evento = await _eventoRepository.GetByIdAsync(eventoId.Trim());
            if (evento is null)
            {
                return (false, "Evento não encontrado.", null);
            }

            var atividade = new Atividade(nome.Trim(), data, eventoId.Trim());
            await _atividadeRepository.CreateAsync(atividade);
            return (true, null, atividade);
        }
        catch (MongoException)
        {
            return (false, "Erro ao acessar o banco de dados.", null);
        }
        catch (Exception)
        {
            return (false, "Erro inesperado ao criar atividade.", null);
        }
    }

    /// <summary>Filtra atividades do evento e ordena por data usando LINQ.</summary>
    public async Task<(bool Ok, string? ErrorMessage, List<Atividade> Atividades)> ListarPorEventoAsync(string eventoId)
    {
        if (string.IsNullOrWhiteSpace(eventoId))
        {
            return (false, "EventoId é obrigatório.", new List<Atividade>());
        }

        try
        {
            var todas = await _atividadeRepository.ListarTodasAsync();
            var id = eventoId.Trim();
            // LINQ: apenas atividades deste evento, ordenadas por data.
            var filtradas = todas
                .Where(a => a.EventoId == id)
                .OrderBy(a => a.Data)
                .ToList();
            return (true, null, filtradas);
        }
        catch (MongoException)
        {
            return (false, "Erro ao acessar o banco de dados.", new List<Atividade>());
        }
        catch (Exception)
        {
            return (false, "Erro inesperado ao listar atividades.", new List<Atividade>());
        }
    }

    /// <summary>Lista todas as atividades ordenadas por data (LINQ).</summary>
    public async Task<(bool Ok, string? ErrorMessage, List<Atividade> Atividades)> ListarTodosAsync()
    {
        try
        {
            var todas = await _atividadeRepository.ListarTodasAsync();
            var ordenadas = todas.OrderBy(a => a.Data).ToList();
            return (true, null, ordenadas);
        }
        catch (MongoException)
        {
            return (false, "Erro ao acessar o banco de dados.", new List<Atividade>());
        }
        catch (Exception)
        {
            return (false, "Erro inesperado ao listar atividades.", new List<Atividade>());
        }
    }

    /// <summary>Busca uma atividade pelo id.</summary>
    public async Task<(bool Ok, string? ErrorMessage, Atividade? Atividade)> ObterPorIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return (false, "Id da atividade é obrigatório.", null);
        }

        try
        {
            var atividade = await _atividadeRepository.GetByIdAsync(id.Trim());
            if (atividade is null)
            {
                return (false, "Atividade não encontrada.", null);
            }

            return (true, null, atividade);
        }
        catch (MongoException)
        {
            return (false, "Erro ao acessar o banco de dados.", null);
        }
        catch (Exception)
        {
            return (false, "Erro inesperado ao buscar atividade.", null);
        }
    }
}
