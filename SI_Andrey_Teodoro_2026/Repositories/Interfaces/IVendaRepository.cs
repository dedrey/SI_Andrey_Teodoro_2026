using System.Data;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Models;
namespace SI_Andrey_Teodoro_2026.Repositories.Interfaces;
public interface IVendaRepository
{
    Task<PaginacaoDto<VendaListDto>> ObterTodosAsync(FiltroConsultaDto filtro);
    Task<Venda?> ObterPorIdAsync(int id);
    Task<List<VendaItemListDto>> ObterItensPorVendaAsync(int vendaId);
    Task<int> InserirAsync(VendaDto dto);
    Task AtualizarAsync(VendaDto dto);
    Task InserirItemAsync(VendaItemDto item, int vendaId);
    Task RemoverItensAsync(int vendaId);
    Task AtualizarStatusAsync(int vendaId, string status, IDbTransaction tx,
        int? movimentacaoId = null, string? motivoCancelamento = null);
    Task<int> ObterEstoqueAtualAsync(int variacaoId);
    Task<(decimal PrecoCusto, DateTime? DataUltimaCompra)?> ObterCustoVariacaoAsync(int variacaoId);
    Task<int> InserirContaReceberAsync(int clienteId, int vendaId, string descricao, DateTime vencimento,
        decimal valor, IDbTransaction tx, DateTime? dataRecebimento = null);
}