using Backend.Helpers;
using Backend.Models;
using Backend.Repositories;
using MongoDB.Driver;

namespace Backend.Services;

/// <summary>Cria atividades (somente admin) e lista por evento com LINQ.</summary>
public class AtividadeService
{
    private const int DescricaoMaxLength = 2000;

    /// <summary>Datas vindas do Mongo costumam ser <see cref="DateTimeKind.Unspecified"/> com valor UTC.</summary>
    private static DateTime ToUtcComparable(DateTime dt)
    {
        if (dt == default) return default;
        return dt.Kind == DateTimeKind.Utc ? dt : DateTime.SpecifyKind(dt, DateTimeKind.Utc);
    }

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
        string dataInicioStr,
        string dataFimStr,
        string eventoId,
        string? descricao)
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

        var descNorm = string.IsNullOrWhiteSpace(descricao) ? null : descricao.Trim();
        if (descNorm != null && descNorm.Length > DescricaoMaxLength)
        {
            return (false, $"A descrição pode ter no máximo {DescricaoMaxLength} caracteres.", null);
        }

        if (!ApiDateParsing.TryParseUtc(dataInicioStr, out var inicioUtc, out var errInicio))
        {
            return (false, $"Data de início da atividade inválida: {errInicio}", null);
        }

        if (!ApiDateParsing.TryParseUtc(dataFimStr, out var fimUtc, out var errFim))
        {
            return (false, $"Data de término da atividade inválida: {errFim}", null);
        }

        if (fimUtc < inicioUtc)
        {
            return (false, "O término da atividade deve ser igual ou posterior ao início.", null);
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

            var inicioEvt = ToUtcComparable(evento.DataInicioEfetiva);
            var fimEvt = ToUtcComparable(evento.DataFimEfetiva);
            if (inicioEvt == default && fimEvt == default)
            {
                return (false, "Evento sem período definido. Cadastre início e término do evento.", null);
            }

            if (inicioUtc < inicioEvt || fimUtc > fimEvt)
            {
                return (false, "Início e término da atividade precisam ficar dentro do período do evento.", null);
            }

            DateTime? dataFimPersist = fimUtc == inicioUtc ? null : fimUtc;
            var atividade = new Atividade(nome.Trim(), inicioUtc, dataFimPersist, eventoId.Trim(), descNorm);
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
