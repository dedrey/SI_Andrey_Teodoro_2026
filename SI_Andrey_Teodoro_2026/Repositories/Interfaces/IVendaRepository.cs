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
    Task AtualizarTotaisAsync(int vendaId, decimal subtotal, decimal desconto, decimal total);
    Task AtualizarStatusAsync(int vendaId, string status, int? movimentacaoId = null, string? motivoCancelamento = null);
    Task AtualizarEstoqueAsync(int variacaoId, int delta);
    Task<int> ObterEstoqueAtualAsync(int variacaoId);
    Task<int> InserirMovimentacaoSaidaAsync(int vendaId);
    Task InserirMovimentacaoItemAsync(int movimentacaoId, int variacaoId, int quantidade, decimal valorUnitario);
    Task<int> InserirContaReceberAsync(int clienteId, int vendaId, string descricao, DateTime vencimento,
        decimal valor, bool jaRecebida = false);
}