using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Models;

namespace SI_Andrey_Teodoro_2026.Repositories.Interfaces;

public interface ICidadeRepository
{
    Task<PaginacaoDto<CidadeListDto>> ObterTodosAsync(FiltroConsultaDto filtro);
    Task<IEnumerable<CidadeListDto>> ObterPorEstadoAsync(int estadoId);
    Task<IEnumerable<CidadeListDto>> ObterTodosAtivosSemPaginacaoAsync();   // ← novo
    Task<Cidade?> ObterPorIdAsync(int id);
    Task<int> InserirAsync(CidadeDto dto);
    Task AtualizarAsync(CidadeDto dto);
    Task AlterarStatusAsync(int id, bool ativo);
    Task<bool> ExisteNomeNoEstadoAsync(string nome, int estadoId, int? idIgnorar = null);
}