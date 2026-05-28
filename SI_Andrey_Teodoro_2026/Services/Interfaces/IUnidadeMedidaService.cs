using SI_Andrey_Teodoro_2026.DTOs;

namespace SI_Andrey_Teodoro_2026.Services.Interfaces;

public interface IUnidadeMedidaService
{
    Task<PaginacaoDto<UnidadeMedidaListDto>> ObterTodosAsync(FiltroConsultaDto filtro);
    Task<IEnumerable<UnidadeMedidaListDto>> ObterTodosAtivosAsync();
    Task<UnidadeMedidaDto?> ObterPorIdAsync(int id);
    Task<(bool sucesso, string mensagem, int id)> SalvarAsync(UnidadeMedidaDto dto);
    Task<(bool sucesso, string mensagem)> AlterarStatusAsync(int id, bool ativar);
}