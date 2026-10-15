using SI_Andrey_Teodoro_2026.DTOs;
namespace SI_Andrey_Teodoro_2026.Repositories.Interfaces;

public interface ITamanhoRepository
{
    Task<PaginacaoDto<TamanhoListDto>> ObterTodosAsync(FiltroConsultaDto filtro);
    Task<IEnumerable<TamanhoListDto>> ObterTodosAtivosAsync();
    Task<int> InserirAsync(TamanhoDto dto);
    Task AtualizarAsync(TamanhoDto dto);
    Task<bool> ExisteNomeAsync(string nome, int? idIgnorar = null);
    Task<int> ObterProximoIdAsync();
    Task<bool> ExisteIdAsync(int id);
}