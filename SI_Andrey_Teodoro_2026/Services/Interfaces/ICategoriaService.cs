using SI_Andrey_Teodoro_2026.DTOs;

namespace SI_Andrey_Teodoro_2026.Services.Interfaces;

public interface ICategoriaService
{
    Task<PaginacaoDto<CategoriaListDto>> ObterTodosAsync(FiltroConsultaDto filtro);
    Task<IEnumerable<CategoriaListDto>> ObterTodosAtivosAsync();
    Task<CategoriaDto?> ObterPorIdAsync(int id);
    Task<(bool sucesso, string mensagem, int id)> SalvarAsync(CategoriaDto dto);
    Task<(bool sucesso, string mensagem)> AlterarStatusAsync(int id, bool ativar);
}