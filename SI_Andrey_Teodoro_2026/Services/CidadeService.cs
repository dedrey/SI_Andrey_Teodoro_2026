using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Repositories.Interfaces;
using SI_Andrey_Teodoro_2026.Services.Interfaces;

namespace SI_Andrey_Teodoro_2026.Services;

public class CidadeService : BaseService<CidadeDto, CidadeListDto>, ICidadeService
{
    private readonly ICidadeRepository _repo;

    public CidadeService(ICidadeRepository repo) => _repo = repo;

    protected override string NomeEntidade => "Cidade";

    public Task<PaginacaoDto<CidadeListDto>> ObterTodosAsync(FiltroConsultaDto filtro)
        => _repo.ObterTodosAsync(filtro);

    public Task<IEnumerable<CidadeListDto>> ObterPorEstadoAsync(int estadoId)
        => _repo.ObterPorEstadoAsync(estadoId);

    public Task<IEnumerable<CidadeListDto>> ObterTodosAtivosSemPaginacaoAsync()
        => _repo.ObterTodosAtivosSemPaginacaoAsync();

    public async Task<CidadeDto?> ObterPorIdAsync(int id)
    {
        var c = await _repo.ObterPorIdAsync(id);
        if (c == null) return null;
        return new CidadeDto
        {
            Id = c.Id,
            IdOriginal = c.Id,
            NomeCidade = c.NomeCidade,
            Ddd = c.Ddd,
            EstadoId = c.EstadoId,
            Ativo = c.Ativo,
            AtualizadoEm = c.AtualizadoEm,
            NomeAtualizadoPor = c.NomeAtualizadoPor
        };
    }

    public async Task<(bool sucesso, string mensagem, int id)> SalvarAsync(CidadeDto dto)
    {
        try
        {
            dto.NomeCidade = dto.NomeCidade.Trim();

            int? ignorar = dto.IdOriginal > 0 ? dto.IdOriginal : null;

            if (await _repo.ExisteNomeNoEstadoAsync(dto.NomeCidade, dto.EstadoId, ignorar))
                return (false, $"Já existe a cidade '{dto.NomeCidade}' neste estado.", 0);

            if (dto.IdOriginal == 0)
            {
                var novoId = await _repo.InserirAsync(dto);
                return (true, "Cidade cadastrada com sucesso!", novoId);
            }

            await _repo.AtualizarAsync(dto);
            return (true, "Cidade atualizada com sucesso!", dto.Id);
        }
        catch (Exception ex) { return (false, Erro(ex).mensagem, 0); }
    }

    public async Task<(bool sucesso, string mensagem)> AlterarStatusAsync(int id, bool ativar)
    {
        try
        {
            await _repo.AlterarStatusAsync(id, ativar);
            return SucessoStatus(ativar);
        }
        catch (Exception ex) { return ErroStatus(ex); }
    }
}