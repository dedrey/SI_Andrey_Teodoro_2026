using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Models;

namespace SI_Andrey_Teodoro_2026.Repositories.Interfaces;

public interface ICategoriaRepository
{
    Task<PaginacaoDto<CategoriaListDto>> ObterTodosAsync(FiltroConsultaDto filtro);
    Task<IEnumerable<CategoriaListDto>> ObterTodosAtivosAsync();
    Task<Categoria?> ObterPorIdAsync(int id);
    Task<int> InserirAsync(CategoriaDto dto);
    Task AtualizarAsync(CategoriaDto dto);
    Task AlterarStatusAsync(int id, bool ativo);
    Task<bool> ExisteNomeAsync(string nome, int? idOriginalIgnorar = null);
}