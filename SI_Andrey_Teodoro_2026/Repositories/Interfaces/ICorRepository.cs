using SI_Andrey_Teodoro_2026.DTOs;
namespace SI_Andrey_Teodoro_2026.Repositories.Interfaces;

public interface ICorRepository
{
    Task<PaginacaoDto<CorListDto>> ObterTodosAsync(FiltroConsultaDto filtro);
    Task<IEnumerable<CorListDto>> ObterTodosAtivosAsync();
    Task<int> InserirAsync(CorDto dto);
    Task AtualizarAsync(CorDto dto);
    Task AlterarStatusAsync(int id, bool ativo);
    Task<bool> ExisteNomeAsync(string nome, int? idIgnorar = null);
}