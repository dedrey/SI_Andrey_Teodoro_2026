using System.Data;
using SI_Andrey_Teodoro_2026.Data;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Repositories.Interfaces;
using SI_Andrey_Teodoro_2026.Services.Interfaces;

namespace SI_Andrey_Teodoro_2026.Services;

public class CompraService : BaseService<CompraDto, CompraListDto>, ICompraService
{
    private readonly ICompraRepository _repo;
    private readonly IContaPagarRepository _contaPagarRepo;
    private readonly ICondicaoPagamentoRepository _condicaoRepo;
    private readonly DbConnectionFactory _factory;

    public CompraService(ICompraRepository repo, IContaPagarRepository contaPagarRepo,
        ICondicaoPagamentoRepository condicaoRepo, DbConnectionFactory factory)
    {
        _repo = repo;
        _contaPagarRepo = contaPagarRepo;
        _condicaoRepo = condicaoRepo;
        _factory = factory;
    }

    protected override string NomeEntidade => "Compra";

    public Task<PaginacaoDto<CompraListDto>> ObterTodosAsync(FiltroConsultaDto filtro)
        => _repo.ObterTodosAsync(filtro);

    public async Task<CompraDto?> ObterPorIdAsync(int id)
    {
        var c = await _repo.ObterPorIdAsync(id);
        if (c == null) return null;
        var itens = await _repo.ObterItensPorCompraAsync(id);
        return new CompraDto
        {
            Id = c.Id,
            IdOriginal = c.Id,
            FornecedorId = c.FornecedorId,
            NomeFornecedor = c.NomeFornecedor,
            NumeroNf = c.NumeroNf,
            DataEmissao = c.DataEmissao,
            DataChegada = c.DataChegada,
            CondicaoPagamentoId = c.CondicaoPagamentoId,
            NomeCondicaoPagamento = c.NomeCondicaoPagamento ?? string.Empty,
            ValorSubtotal = c.ValorSubtotal,
            ValorFrete = c.ValorFrete,
            ValorOutrosAcrescimos = c.ValorOutrosAcrescimos,
            ValorDesconto = c.ValorDesconto,
            ValorTotal = c.ValorTotal,
            StatusCompra = c.StatusCompra,
            MotivoCancelamento = c.MotivoCancelamento,
            CriadoEm = c.CriadoEm,
            AtualizadoEm = c.AtualizadoEm,
            Itens = itens.Select(i => new CompraItemDto
            {
                Id = i.Id,
                CompraId = i.CompraId,
                ProdutoVariacaoId = i.ProdutoVariacaoId,
                NomeProduto = i.NomeProduto,
                Cor = i.Cor,
                Tamanho = i.Tamanho,
                Quantidade = i.Quantidade,
                ValorUnitario = i.ValorUnitario,
                ValorDesconto = i.ValorDesconto,
                CustoUnitarioEfetivo = i.CustoUnitarioEfetivo
            }).ToList()
        };
    }

    public Task<List<CompraItemListDto>> ObterItensAsync(int compraId)
        => _repo.ObterItensPorCompraAsync(compraId);

    public async Task<(bool sucesso, string mensagem, int id)> SalvarAsync(CompraDto dto)
    {
        try
        {
            if (!dto.FornecedorId.HasValue)
                return (false, "Selecione o fornecedor.", 0);

            var itensValidos = dto.Itens.Where(i => !i.Removido).ToList();
            if (itensValidos.Count == 0)
                return (false, "Adicione pelo menos um item à compra.", 0);

            if (!dto.DataEmissao.HasValue)
                return (false, "Informe a data de emissão.", 0);
            if (dto.DataEmissao.Value.Date > DateTime.Today)
                return (false, "A data de emissão não pode ser maior que a data atual.", 0);

            if (!dto.DataChegada.HasValue)
                return (false, "Informe a data de chegada.", 0);
            if (dto.DataChegada.Value.Date < dto.DataEmissao.Value.Date)
                return (false, "A data de chegada não pode ser anterior à data de emissão.", 0);

            if (dto.ValorFrete < 0)
                return (false, "O frete não pode ser negativo.", 0);
            if (dto.ValorOutrosAcrescimos < 0)
                return (false, "Outros acréscimos não podem ser negativos.", 0);

            foreach (var item in itensValidos)
            {
                if (item.ProdutoVariacaoId == 0)
                    return (false, "Selecione a variação de todos os itens.", 0);
                if (item.Quantidade <= 0)
                    return (false, $"{Desc(item)}: quantidade deve ser maior que zero.", 0);
                if (item.ValorUnitario <= 0)
                    return (false, $"{Desc(item)}: custo unitário deve ser maior que zero.", 0);
                if (item.ValorDesconto < 0 || item.ValorDesconto > item.ValorUnitario * item.Quantidade)
                    return (false, $"{Desc(item)}: desconto não pode ser maior que o valor do item.", 0);
            }

            var duplicada = itensValidos.GroupBy(i => i.ProdutoVariacaoId).FirstOrDefault(g => g.Count() > 1);
            if (duplicada != null)
                return (false, $"{Desc(duplicada.First())} foi adicionado mais de uma vez. " +
                               "Junte as quantidades em um único item.", 0);

            dto.ValorSubtotal = itensValidos.Sum(i => i.ValorUnitario * i.Quantidade);
            dto.ValorDesconto = itensValidos.Sum(i => i.ValorDesconto);
            dto.ValorTotal = dto.ValorSubtotal - dto.ValorDesconto + dto.ValorFrete + dto.ValorOutrosAcrescimos;

            RatearCusto(itensValidos, dto.ValorFrete + dto.ValorOutrosAcrescimos);

            var parcelasGerar = new List<(int numero, DateTime vencimento, decimal valor)>();
            var totalParcelas = 0;

            if (dto.CondicaoPagamentoId.HasValue && dto.ValorTotal > 0)
            {
                var condicao = await _condicaoRepo.ObterPorIdAsync(dto.CondicaoPagamentoId.Value);
                if (condicao != null && condicao.NumeroParcelas > 0)
                {
                    totalParcelas = condicao.NumeroParcelas;
                    var valorParcela = Math.Round(dto.ValorTotal / totalParcelas, 2);
                    var diferenca = dto.ValorTotal - (valorParcela * totalParcelas);
                    var configParcelas = await _condicaoRepo.ObterParcelasAsync(dto.CondicaoPagamentoId.Value);

                    for (int p = 1; p <= totalParcelas; p++)
                    {
                        var cfg = configParcelas.FirstOrDefault(x => x.NumeroParcela == p);
                        var vencimento = dto.DataEmissao.Value.AddDays(cfg?.DiasVencimento ?? 30);
                        var valor = p == totalParcelas ? valorParcela + diferenca : valorParcela;
                        parcelasGerar.Add((p, vencimento, valor));
                    }
                }
            }

            using var conn = _factory.CreateConnection();
            if (conn.State != ConnectionState.Open) conn.Open();
            using var tx = conn.BeginTransaction();

            var novoId = await _repo.InserirAsync(dto, tx);

            foreach (var item in itensValidos)
            {
                await _repo.InserirItemAsync(item, novoId, tx);

                if (!await _repo.AtualizarEstoqueAsync(item.ProdutoVariacaoId, +item.Quantidade, tx))
                    return (false, $"{Desc(item)}: variação sem registro de estoque. Compra não foi gravada.", 0);

                await _repo.RecalcularCustoVariacaoAsync(item.ProdutoVariacaoId, tx);
            }

            foreach (var (numero, vencimento, valor) in parcelasGerar)
            {
                var descricao = totalParcelas == 1
                    ? $"Compra #{novoId}" + (string.IsNullOrWhiteSpace(dto.NumeroNf) ? "" : $" — NF {dto.NumeroNf}")
                    : $"Compra #{novoId} — Parcela {numero}/{totalParcelas}";

                await _contaPagarRepo.InserirAutomaticaAsync(dto.FornecedorId, novoId, descricao, vencimento, valor, tx);
            }

            tx.Commit();

            return (true, "Compra lançada com sucesso! Estoque e custo atualizados" +
                (parcelasGerar.Count > 0 ? " e contas a pagar geradas." : "."), novoId);
        }
        catch (Exception ex) { return (false, Erro(ex).mensagem, 0); }
    }

    public async Task<(bool sucesso, string mensagem)> CancelarAsync(int compraId, string motivo)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(motivo))
                return (false, "Informe o motivo do cancelamento.");
            motivo = motivo.Trim();
            if (motivo.Length > 255)
                return (false, "O motivo do cancelamento deve ter no máximo 255 caracteres.");

            var compra = await _repo.ObterPorIdAsync(compraId);
            if (compra == null) return (false, "Compra não encontrada.");
            if (compra.StatusCompra == "CANCELADO") return (false, "Compra já está cancelada.");

            if (await _contaPagarRepo.ExisteParcelaPagaAsync(compraId))
                return (false, "Não é possível cancelar: já existe parcela paga para esta compra.");

            var itens = await _repo.ObterItensPorCompraAsync(compraId);

            using var conn = _factory.CreateConnection();
            if (conn.State != ConnectionState.Open) conn.Open();
            using var tx = conn.BeginTransaction();

            foreach (var item in itens)
            {
                if (!await _repo.AtualizarEstoqueAsync(item.ProdutoVariacaoId, -item.Quantidade, tx))
                    return (false, $"Não é possível cancelar: estoque insuficiente de " +
                                   $"{item.NomeProduto} {item.Cor}/{item.Tamanho} para estornar " +
                                   $"{item.Quantidade} un. (parte já foi vendida).");
            }

            await _contaPagarRepo.CancelarPorCompraAsync(compraId, tx);
            await _repo.AtualizarStatusAsync(compraId, "CANCELADO", tx, motivoCancelamento: motivo);

            foreach (var variacaoId in itens.Select(i => i.ProdutoVariacaoId).Distinct())
                await _repo.RecalcularCustoVariacaoAsync(variacaoId, tx);

            tx.Commit();
            return (true, "Compra cancelada com sucesso! Estoque estornado, custo recalculado e parcelas em aberto canceladas.");
        }
        catch (Exception ex) { return (false, Erro(ex).mensagem); }
    }

    private static void RatearCusto(List<CompraItemDto> itens, decimal acrescimos)
    {
        var baseValor = itens.Sum(i => i.ValorTotal);
        var baseQtd = itens.Sum(i => i.Quantidade);

        foreach (var i in itens)
        {
            var proporcao = baseValor > 0
                ? i.ValorTotal / baseValor
                : (decimal)i.Quantidade / baseQtd;

            var custoTotalItem = i.ValorTotal + acrescimos * proporcao;
            i.CustoUnitarioEfetivo = Math.Round(custoTotalItem / i.Quantidade, 4);
        }
    }

    private static string Desc(CompraItemDto i) => $"{i.NomeProduto} {i.Cor}/{i.Tamanho}";
}