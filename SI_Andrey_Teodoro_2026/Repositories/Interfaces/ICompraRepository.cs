using System.Data;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Models;
namespace SI_Andrey_Teodoro_2026.Repositories.Interfaces;

public interface ICompraRepository
{
    Task<PaginacaoDto<CompraListDto>> ObterTodosAsync(FiltroConsultaDto filtro);
    Task<Compra?> ObterPorIdAsync(int id);
    Task<List<CompraItemListDto>> ObterItensPorCompraAsync(int compraId);

    Task<int> InserirAsync(CompraDto dto, IDbTransaction tx);
    Task InserirItemAsync(CompraItemDto item, int compraId, IDbTransaction tx);
    Task AtualizarStatusAsync(int compraId, string status, IDbTransaction tx, string? motivoCancelamento = null);

    Task<bool> AtualizarEstoqueAsync(int variacaoId, int delta, IDbTransaction tx);

    Task RecalcularCustoVariacaoAsync(int variacaoId, IDbTransaction tx);
}