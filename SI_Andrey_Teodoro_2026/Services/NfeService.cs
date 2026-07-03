using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Repositories.Interfaces;
using SI_Andrey_Teodoro_2026.Services.Interfaces;

namespace SI_Andrey_Teodoro_2026.Services;

public class NfeService : INfeService
{
    private readonly INfeRepository _repo;
    private readonly IVendaRepository _vendaRepo;

    public NfeService(INfeRepository repo, IVendaRepository vendaRepo)
    {
        _repo = repo;
        _vendaRepo = vendaRepo;
    }

    public Task<PaginacaoDto<NfeListDto>> ObterTodosAsync(FiltroConsultaDto filtro)
        => _repo.ObterTodosAsync(filtro);

    public Task<NfeDto?> ObterPorIdAsync(int id)
        => _repo.ObterPorIdAsync(id);

    public Task<IEnumerable<VendaParaNfeDto>> ObterVendasDisponiveisAsync()
        => _repo.ObterVendasDisponiveisAsync();

    public async Task<(bool sucesso, string mensagem, int id)> GerarAsync(int vendaId, int emitenteId, int? transportadoraId)
    {
        try
        {
            var venda = await _vendaRepo.ObterPorIdAsync(vendaId);
            if (venda == null)
                return (false, "Venda não encontrada.", 0);
            if (venda.StatusVenda != "FINALIZADA")
                return (false, "Apenas vendas finalizadas podem gerar Nota Fiscal.", 0);
            if (!venda.ClienteId.HasValue)
                return (false, "A venda não possui cliente vinculado.", 0);

            var itensOrigem = await _repo.ObterItensVendaParaNfeAsync(vendaId);
            if (itensOrigem.Count == 0)
                return (false, "A venda não possui itens.", 0);

            const short serie = 1;
            var numero = await _repo.ProximoNumeroAsync(emitenteId, serie);

            var nfeDto = new NfeDto
            {
                Numero = numero,
                Serie = serie,
                DataEmissao = DateTime.Now,
                EmitenteId = emitenteId,
                ClienteId = venda.ClienteId.Value,
                VendaId = vendaId,
                TransportadoraId = transportadoraId,
                StatusNfe = "EMITIDA",
                ValorProdutos = venda.ValorSubtotal,
                ValorDesconto = venda.ValorSubtotal - venda.ValorTotal,
                ValorTotal = venda.ValorTotal
            };

            var itensNfe = DistribuirAjusteProporcional(itensOrigem, venda.ValorTotal);

            var novoId = await _repo.InserirAsync(nfeDto, itensNfe);
            return (true, $"Nota Fiscal nº {numero} gerada com sucesso!", novoId);
        }
        catch (Exception ex) { return (false, $"Erro ao gerar Nota Fiscal: {ex.Message}", 0); }
    }

    public async Task<(bool sucesso, string mensagem)> CancelarAsync(int id)
    {
        try
        {
            await _repo.AlterarStatusAsync(id, "CANCELADA");
            return (true, "Nota Fiscal cancelada.");
        }
        catch (Exception ex) { return (false, $"Erro: {ex.Message}"); }
    }
    private static List<NfeProdutoDto> DistribuirAjusteProporcional(List<VendaItemParaNfeDto> itensOrigem, decimal valorTotalFinal)
    {
        var somaNetItens = itensOrigem.Sum(i => i.ValorTotal);
        var ajusteTotal = valorTotalFinal - somaNetItens;

        var resultado = new List<NfeProdutoDto>();
        decimal ajusteAcumulado = 0;

        for (int idx = 0; idx < itensOrigem.Count; idx++)
        {
            var origem = itensOrigem[idx];
            decimal ajusteItem;

            if (idx == itensOrigem.Count - 1)
            {
                ajusteItem = ajusteTotal - ajusteAcumulado;
            }
            else
            {
                var proporcao = somaNetItens > 0 ? origem.ValorTotal / somaNetItens : 0;
                ajusteItem = Math.Round(ajusteTotal * proporcao, 2);
                ajusteAcumulado += ajusteItem;
            }

            var totalFinalItem = origem.ValorTotal + ajusteItem;
            var descontoFinalItem = (origem.ValorUnitario * origem.Quantidade) - totalFinalItem;

            resultado.Add(new NfeProdutoDto
            {
                ProdutoVariacaoId = origem.ProdutoVariacaoId,
                DescricaoItem = Truncar($"{origem.NomeProduto} - {origem.Cor}/{origem.Tamanho}", 100),
                UnidadeMedidaId = origem.UnidadeMedidaId,
                Quantidade = origem.Quantidade,
                ValorUnitario = origem.ValorUnitario,
                ValorDesconto = descontoFinalItem,
                ValorTotal = totalFinalItem
            });
        }

        return resultado;
    }

    private static string Truncar(string s, int max) => s.Length <= max ? s : s[..max];
}