using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Models;

namespace SI_Andrey_Teodoro_2026.Repositories.Interfaces;

public interface IPaisRepository
{
    Task<PaginacaoDto<PaisListDto>> ObterTodosAsync(FiltroConsultaDto filtro);
    Task<IEnumerable<PaisListDto>> ObterTodosAtivosSemPaginacaoAsync();
    Task<Pais?> ObterPorIdAsync(int id);
    Task<int> InserirAsync(PaisDto dto);
    Task AtualizarAsync(PaisDto dto);
    Task AlterarStatusAsync(int id, bool ativo);
    Task<bool> ExisteSiglaAsync(string sigla, int? idOriginalIgnorar = null);
    Task<bool> ExisteNomeAsync(string nome, int? idOriginalIgnorar = null);
    Task<bool> ExisteDdiAsync(string ddi, int? idOriginalIgnorar = null);   // ← NOVO
}