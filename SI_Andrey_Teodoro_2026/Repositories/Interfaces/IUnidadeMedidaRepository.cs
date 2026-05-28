using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Models;

namespace SI_Andrey_Teodoro_2026.Repositories.Interfaces;

public interface IUnidadeMedidaRepository
{
    Task<PaginacaoDto<UnidadeMedidaListDto>> ObterTodosAsync(FiltroConsultaDto filtro);
    Task<IEnumerable<UnidadeMedidaListDto>> ObterTodosAtivosAsync();
    Task<UnidadeMedida?> ObterPorIdAsync(int id);
    Task<int> InserirAsync(UnidadeMedidaDto dto);
    Task AtualizarAsync(UnidadeMedidaDto dto);
    Task AlterarStatusAsync(int id, bool ativo);
    Task<bool> ExisteSiglaAsync(string sigla, int? idOriginalIgnorar = null);
}