using SI_Andrey_Teodoro_2026.DTOs;

namespace SI_Andrey_Teodoro_2026.Services.Interfaces;

public interface IPaisService
{
    Task<PaginacaoDto<PaisListDto>> ObterTodosAsync(FiltroConsultaDto filtro);
    Task<IEnumerable<PaisListDto>> ObterTodosAtivosAsync();
    Task<PaisDto?> ObterPorIdAsync(int id);
    Task<(bool sucesso, string mensagem, int id)> SalvarAsync(PaisDto dto);
    Task<(bool sucesso, string mensagem)> AlterarStatusAsync(int id, bool ativar);
}