using SI_Andrey_Teodoro_2026.DTOs;

namespace SI_Andrey_Teodoro_2026.Services.Interfaces;

public interface IMovimentacaoEstoqueService
{
    Task<PaginacaoDto<MovimentacaoEstoqueListDto>> ObterTodosAsync(FiltroConsultaDto filtro);
    Task<MovimentacaoEstoqueDto?> ObterPorIdAsync(int id);
    Task<List<MovimentacaoEstoqueItemListDto>> ObterItensAsync(int movimentacaoId);
    Task<(bool sucesso, string mensagem, int id)> SalvarAsync(MovimentacaoEstoqueDto dto);
}