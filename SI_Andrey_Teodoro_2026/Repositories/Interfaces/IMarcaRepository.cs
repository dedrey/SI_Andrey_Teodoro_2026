using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Models;

namespace SI_Andrey_Teodoro_2026.Repositories.Interfaces;

public interface IMarcaRepository
{
    Task<PaginacaoDto<MarcaListDto>> ObterTodosAsync(FiltroConsultaDto filtro);
    Task<IEnumerable<MarcaListDto>> ObterTodosAtivosAsync();
    Task<Marca?> ObterPorIdAsync(int id);
    Task<int> InserirAsync(MarcaDto dto);
    Task AtualizarAsync(MarcaDto dto);
    Task AlterarStatusAsync(int id, bool ativo);
    Task<bool> ExisteNomeAsync(string nome, int? idOriginalIgnorar = null);
}