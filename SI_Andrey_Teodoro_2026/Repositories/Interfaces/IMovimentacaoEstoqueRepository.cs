using System.Data;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Models;

namespace SI_Andrey_Teodoro_2026.Repositories.Interfaces;

public interface IMovimentacaoEstoqueRepository
{
    Task<PaginacaoDto<MovimentacaoEstoqueListDto>> ObterTodosAsync(FiltroConsultaDto filtro);
    Task<MovimentacaoEstoque?> ObterPorIdAsync(int id);
    Task<List<MovimentacaoEstoqueItemListDto>> ObterItensPorMovimentacaoAsync(int movimentacaoId);
    Task<int> ObterEstoqueAtualAsync(int variacaoId, IDbTransaction? tx = null);

    Task<int> InserirAsync(string tipoMovimentacao, string? observacao, int? compraId, IDbTransaction tx);
    Task InserirItemAsync(int movimentacaoId, int variacaoId, int quantidade, decimal valorUnitario, IDbTransaction tx);
    Task<bool> AtualizarEstoqueAsync(int variacaoId, int delta, IDbTransaction tx);
}