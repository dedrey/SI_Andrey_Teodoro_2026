using SI_Andrey_Teodoro_2026.DTOs;

namespace SI_Andrey_Teodoro_2026.Services.Interfaces;

public interface IVendaService
{
    Task<PaginacaoDto<VendaListDto>> ObterTodosAsync(FiltroConsultaDto filtro);
    Task<VendaDto?> ObterPorIdAsync(int id);
    Task<List<VendaItemListDto>> ObterItensAsync(int vendaId);
    Task<(bool sucesso, string mensagem, int id)> SalvarAsync(VendaDto dto);
    Task<(bool sucesso, string mensagem)> FinalizarAsync(int vendaId);
    Task<(bool sucesso, string mensagem)> CancelarAsync(int vendaId, string motivo);
}