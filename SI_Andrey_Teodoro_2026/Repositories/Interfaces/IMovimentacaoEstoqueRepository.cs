using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Models;

namespace SI_Andrey_Teodoro_2026.Repositories.Interfaces;

public interface IMovimentacaoEstoqueRepository
{
    Task<PaginacaoDto<MovimentacaoEstoqueListDto>> ObterTodosAsync(FiltroConsultaDto filtro);
    Task<MovimentacaoEstoque?> ObterPorIdAsync(int id);
    Task<List<MovimentacaoEstoqueItemListDto>> ObterItensPorMovimentacaoAsync(int movimentacaoId);
    Task<int> InserirAsync(MovimentacaoEstoqueDto dto);
    Task InserirItemAsync(MovimentacaoEstoqueItemDto item, int movimentacaoId);
    Task AtualizarEstoqueAsync(int variacaoId, int delta);
    Task<int> ObterEstoqueAtualAsync(int variacaoId);
}