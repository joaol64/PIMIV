using Backend.Models;
using Backend.Repositories;
using MongoDB.Driver;

namespace Backend.Services;

/// <summary>Inscrição de usuário comum (conta <see cref="User"/>) em uma atividade.</summary>
public class InscricaoService
{
    private readonly UserRepository _userRepository;
    private readonly ParticipanteRepository _participanteRepository;
    private readonly AtividadeRepository _atividadeRepository;
    private readonly EventoRepository _eventoRepository;
    private readonly InscricaoRepository _inscricaoRepository;

    public InscricaoService(
        UserRepository userRepository,
        ParticipanteRepository participanteRepository,
        AtividadeRepository atividadeRepository,
        EventoRepository eventoRepository,
        InscricaoRepository inscricaoRepository)
    {
        _userRepository = userRepository;
        _participanteRepository = participanteRepository;
        _atividadeRepository = atividadeRepository;
        _eventoRepository = eventoRepository;
        _inscricaoRepository = inscricaoRepository;
    }

    /// <summary>
    /// Usa o id da conta logada (<see cref="User"/>), garante um <see cref="Participante"/> e cria a inscrição.
    /// </summary>
    public async Task<(bool Ok, string? ErrorMessage, Inscricao? Inscricao)> InscreverAsync(
        string usuarioId,
        string atividadeId)
    {
        if (string.IsNullOrWhiteSpace(usuarioId))
        {
            return (false, "Informe o id do usuário logado.", null);
        }

        if (string.IsNullOrWhiteSpace(atividadeId))
        {
            return (false, "AtividadeId é obrigatório.", null);
        }

        try
        {
            var conta = await _userRepository.GetByIdAsync(usuarioId.Trim());
            if (conta is null)
            {
                return (false, "Usuário não encontrado.", null);
            }

            var atividade = await _atividadeRepository.GetByIdAsync(atividadeId.Trim());
            if (atividade is null)
            {
                return (false, "Atividade não encontrada.", null);
            }

            // Reaproveita ou cria participante com mesmo nome/email da conta (usuário comum).
            var participante = await _participanteRepository.GetByEmailAsync(conta.Email);
            if (participante is null)
            {
                participante = new Participante(conta.Nome, conta.Email);
                await _participanteRepository.CreateAsync(participante);
                // Garante id preenchido (alguns cenários do driver só atualizam após releitura).
                if (string.IsNullOrEmpty(participante.Id))
                {
                    participante = await _participanteRepository.GetByEmailAsync(conta.Email);
                }
            }

            if (participante is null || string.IsNullOrEmpty(participante.Id))
            {
                return (false, "Não foi possível obter o participante após gravação.", null);
            }

            var pid = participante.Id;
            var aid = atividade.Id ?? string.Empty;

            if (await _inscricaoRepository.ExisteAsync(pid, aid))
            {
                return (false, "Você já está inscrito nesta atividade.", null);
            }

            var inscricao = new Inscricao(pid, aid);
            await _inscricaoRepository.CreateAsync(inscricao);
            return (true, null, inscricao);
        }
        catch (ArgumentException ex)
        {
            return (false, ex.Message, null);
        }
        catch (MongoException)
        {
            return (false, "Erro ao acessar o banco de dados.", null);
        }
        catch (Exception)
        {
            return (false, "Erro inesperado ao inscrever.", null);
        }
    }

    /// <summary>Lista todas as inscrições (ordenadas por participante e atividade, em memória).</summary>
    public async Task<(bool Ok, string? ErrorMessage, List<Inscricao> Inscricoes)> ListarTodosAsync()
    {
        try
        {
            var lista = await _inscricaoRepository.ListarTodosAsync();
            var ordenada = lista
                .OrderBy(i => i.ParticipanteId)
                .ThenBy(i => i.AtividadeId)
                .ToList();
            return (true, null, ordenada);
        }
        catch (MongoException)
        {
            return (false, "Erro ao acessar o banco de dados.", new List<Inscricao>());
        }
        catch (Exception)
        {
            return (false, "Erro inesperado ao listar inscrições.", new List<Inscricao>());
        }
    }

    /// <summary>Busca uma inscrição pelo id.</summary>
    public async Task<(bool Ok, string? ErrorMessage, Inscricao? Inscricao)> ObterPorIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return (false, "Id da inscrição é obrigatório.", null);
        }

        try
        {
            var inscricao = await _inscricaoRepository.GetByIdAsync(id.Trim());
            if (inscricao is null)
            {
                return (false, "Inscrição não encontrada.", null);
            }

            return (true, null, inscricao);
        }
        catch (MongoException)
        {
            return (false, "Erro ao acessar o banco de dados.", null);
        }
        catch (Exception)
        {
            return (false, "Erro inesperado ao buscar inscrição.", null);
        }
    }

    /// <summary>Quantas inscrições existem na atividade (no máximo uma por participante).</summary>
    public async Task<(bool Ok, string? ErrorMessage, long Total)> ContarPorAtividadeAsync(string atividadeId)
    {
        if (string.IsNullOrWhiteSpace(atividadeId))
        {
            return (false, "AtividadeId é obrigatório.", 0);
        }

        try
        {
            var atividade = await _atividadeRepository.GetByIdAsync(atividadeId.Trim());
            if (atividade is null)
            {
                return (false, "Atividade não encontrada.", 0);
            }

            var aid = atividade.Id ?? string.Empty;
            var total = await _inscricaoRepository.CountByAtividadeAsync(aid);
            return (true, null, total);
        }
        catch (MongoException)
        {
            return (false, "Erro ao acessar o banco de dados.", 0);
        }
        catch (Exception)
        {
            return (false, "Erro inesperado ao contar inscrições.", 0);
        }
    }

    /// <summary>
    /// Agrega inscrições de todas as atividades do evento: total de linhas e participantes distintos.
    /// </summary>
    public async Task<(bool Ok, string? ErrorMessage, long TotalInscricoes, long ParticipantesDistintos)> ContarPorEventoAsync(
        string eventoId)
    {
        if (string.IsNullOrWhiteSpace(eventoId))
        {
            return (false, "EventoId é obrigatório.", 0, 0);
        }

        try
        {
            var evento = await _eventoRepository.GetByIdAsync(eventoId.Trim());
            if (evento is null)
            {
                return (false, "Evento não encontrado.", 0, 0);
            }

            var idsAtividades = await _atividadeRepository.ListarIdsPorEventoAsync(evento.Id!);
            if (idsAtividades.Count == 0)
            {
                return (true, null, 0, 0);
            }

            var total = await _inscricaoRepository.CountByAtividadesAsync(idsAtividades);
            var distintos = await _inscricaoRepository.CountParticipantesDistintosPorAtividadesAsync(idsAtividades);
            return (true, null, total, distintos);
        }
        catch (MongoException)
        {
            return (false, "Erro ao acessar o banco de dados.", 0, 0);
        }
        catch (Exception)
        {
            return (false, "Erro inesperado ao contar inscrições.", 0, 0);
        }
    }
}
