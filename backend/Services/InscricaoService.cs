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
    private readonly InscricaoRepository _inscricaoRepository;

    public InscricaoService(
        UserRepository userRepository,
        ParticipanteRepository participanteRepository,
        AtividadeRepository atividadeRepository,
        InscricaoRepository inscricaoRepository)
    {
        _userRepository = userRepository;
        _participanteRepository = participanteRepository;
        _atividadeRepository = atividadeRepository;
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
}
