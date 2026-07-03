using SI_Andrey_Teodoro_2026.DTOs;

namespace SI_Andrey_Teodoro_2026.Repositories.Interfaces;

public interface INfeRepository
{
    Task<PaginacaoDto<NfeListDto>> ObterTodosAsync(FiltroConsultaDto filtro);
    Task<NfeDto?> ObterPorIdAsync(int id);
    Task<IEnumerable<VendaParaNfeDto>> ObterVendasDisponiveisAsync();
    Task<List<VendaItemParaNfeDto>> ObterItensVendaParaNfeAsync(int vendaId);
    Task<int> ProximoNumeroAsync(int emitenteId, short serie);
    Task<int> InserirAsync(NfeDto dto, List<NfeProdutoDto> itens);
    Task AlterarStatusAsync(int id, string status);
}