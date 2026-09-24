using SI_Andrey_Teodoro_2026.DTOs;

namespace SI_Andrey_Teodoro_2026.Services.Interfaces;

public interface ICompraService
{
    Task<PaginacaoDto<CompraListDto>> ObterTodosAsync(FiltroConsultaDto filtro);
    Task<CompraDto?> ObterPorIdAsync(int id);
    Task<List<CompraItemListDto>> ObterItensAsync(int compraId);
    Task<(bool sucesso, string mensagem, int id)> SalvarAsync(CompraDto dto);
    Task<(bool sucesso, string mensagem)> CancelarAsync(int compraId, string motivo);
}
