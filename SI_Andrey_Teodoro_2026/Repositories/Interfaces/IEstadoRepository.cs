using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Models;

namespace SI_Andrey_Teodoro_2026.Repositories.Interfaces;

public interface IEstadoRepository
{
    Task<PaginacaoDto<EstadoListDto>> ObterTodosAsync(FiltroConsultaDto filtro);
    Task<IEnumerable<EstadoListDto>> ObterPorPaisAsync(int paisId);
    Task<Estado?> ObterPorIdAsync(int id);
    Task<int> InserirAsync(EstadoDto dto);
    Task AtualizarAsync(EstadoDto dto);
    Task AlterarStatusAsync(int id, bool ativo);
    Task<bool> ExisteUfNoPaisAsync(string uf, int paisId, int? idIgnorar = null);
}