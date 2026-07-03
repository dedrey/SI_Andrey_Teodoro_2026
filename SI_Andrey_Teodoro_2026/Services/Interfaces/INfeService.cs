using SI_Andrey_Teodoro_2026.DTOs;

namespace SI_Andrey_Teodoro_2026.Services.Interfaces;

public interface INfeService
{
    Task<PaginacaoDto<NfeListDto>> ObterTodosAsync(FiltroConsultaDto filtro);
    Task<NfeDto?> ObterPorIdAsync(int id);
    Task<IEnumerable<VendaParaNfeDto>> ObterVendasDisponiveisAsync();
    Task<(bool sucesso, string mensagem, int id)> GerarAsync(int vendaId, int emitenteId, int? transportadoraId);
    Task<(bool sucesso, string mensagem)> CancelarAsync(int id);
}