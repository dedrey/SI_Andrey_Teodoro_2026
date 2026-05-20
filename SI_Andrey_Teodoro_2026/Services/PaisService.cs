using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Repositories.Interfaces;
using SI_Andrey_Teodoro_2026.Services.Interfaces;

namespace SI_Andrey_Teodoro_2026.Services;

public class PaisService : IPaisService
{
    private readonly IPaisRepository _repo;

    public PaisService(IPaisRepository repo) => _repo = repo;

    public Task<PaginacaoDto<PaisListDto>> ObterTodosAsync(FiltroConsultaDto filtro)
        => _repo.ObterTodosAsync(filtro);

    public Task<IEnumerable<PaisListDto>> ObterTodosAtivosAsync()
        => _repo.ObterTodosAtivosSemPaginacaoAsync();

    public async Task<PaisDto?> ObterPorIdAsync(int id)
    {
        var pais = await _repo.ObterPorIdAsync(id);
        if (pais == null) return null;
        return new PaisDto
        {
            Id = pais.Id,
            IdOriginal = pais.Id,
            Ddi = pais.Ddi,
            Sigla = pais.Sigla,
            Moeda = pais.Moeda,
            SimboleMoeda = pais.SimboleMoeda,
            NomePais = pais.NomePais,
            Ativo = pais.Ativo
        };
    }

    public async Task<(bool sucesso, string mensagem, int id)> SalvarAsync(PaisDto dto)
    {
        try
        {
            dto.Sigla = dto.Sigla.ToUpper().Trim();
            dto.NomePais = dto.NomePais.Trim();
            dto.Ddi = dto.Ddi.Trim();
            dto.SimboleMoeda = dto.SimboleMoeda.ToUpper().Trim();
            int? ignorar = dto.IdOriginal > 0 ? dto.IdOriginal : null;

            if (await _repo.ExisteSiglaAsync(dto.Sigla, ignorar))
                return (false, $"Já existe um país com a sigla '{dto.Sigla}'.", 0);

            if (await _repo.ExisteNomeAsync(dto.NomePais, ignorar))
                return (false, $"Já existe um país com o nome '{dto.NomePais}'.", 0);
            if (await _repo.ExisteDdiAsync(dto.Ddi, ignorar))
                return (false, $"Já existe um país com o DDI '{dto.Ddi}'. Cada país deve ter um DDI único.", 0);

            if (dto.IdOriginal == 0)
            {
                var novoId = await _repo.InserirAsync(dto);
                return (true, "País cadastrado com sucesso!", novoId);
            }
            else
            {
                await _repo.AtualizarAsync(dto);
                return (true, "País atualizado com sucesso!", dto.Id);
            }
        }
        catch (Exception ex)
        {
            return (false, $"Erro ao salvar país: {ex.Message}", 0);
        }
    }

    public async Task<(bool sucesso, string mensagem)> AlterarStatusAsync(int id, bool ativar)
    {
        try
        {
            await _repo.AlterarStatusAsync(id, ativar);
            return (true, $"País {(ativar ? "ativado" : "desativado")} com sucesso!");
        }
        catch (Exception ex)
        {
            return (false, $"Erro ao alterar status: {ex.Message}");
        }
    }
}