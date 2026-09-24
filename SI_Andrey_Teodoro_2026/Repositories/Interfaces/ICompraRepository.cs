using System.Data;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Models;
namespace SI_Andrey_Teodoro_2026.Repositories.Interfaces;

public interface ICompraRepository
{
    Task<PaginacaoDto<CompraListDto>> ObterTodosAsync(FiltroConsultaDto filtro);
    Task<Compra?> ObterPorIdAsync(int id);
    Task<List<CompraItemListDto>> ObterItensPorCompraAsync(int compraId);

    // Escritas: sempre dentro da transação aberta pelo CompraService
    Task<int> InserirAsync(CompraDto dto, IDbTransaction tx);
    Task InserirItemAsync(CompraItemDto item, int compraId, IDbTransaction tx);
    Task AtualizarStatusAsync(int compraId, string status, IDbTransaction tx, string? motivoCancelamento = null);

    /// Retorna false se a variação não tem linha em 'estoque' ou se o saldo ficaria negativo.
    Task<bool> AtualizarEstoqueAsync(int variacaoId, int delta, IDbTransaction tx);

    /// Custo e data da última compra da variação = última compra LANCADO (por data de emissão).
    /// Sem compra lançada: custo 0 e data NULL.
    Task RecalcularCustoVariacaoAsync(int variacaoId, IDbTransaction tx);
}