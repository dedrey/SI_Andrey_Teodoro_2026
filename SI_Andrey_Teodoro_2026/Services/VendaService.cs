using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Repositories.Interfaces;
using SI_Andrey_Teodoro_2026.Services.Interfaces;

namespace SI_Andrey_Teodoro_2026.Services;

public class VendaService : BaseService<VendaDto, VendaListDto>, IVendaService
{
    private readonly IVendaRepository _repo;
    private readonly ICondicaoPagamentoRepository _condicaoRepo;

    public VendaService(IVendaRepository repo, ICondicaoPagamentoRepository condicaoRepo)
    {
        _repo = repo;
        _condicaoRepo = condicaoRepo;
    }

    protected override string NomeEntidade => "Venda";

    public Task<PaginacaoDto<VendaListDto>> ObterTodosAsync(FiltroConsultaDto filtro)
        => _repo.ObterTodosAsync(filtro);

    public async Task<VendaDto?> ObterPorIdAsync(int id)
    {
        var v = await _repo.ObterPorIdAsync(id);
        if (v == null) return null;
        var itens = await _repo.ObterItensPorVendaAsync(id);
        return new VendaDto
        {
            Id = v.Id,
            IdOriginal = v.Id,
            ClienteId = v.ClienteId,
            NomeCliente = v.NomeCliente,
            CondicaoPagamentoId = v.CondicaoPagamentoId,
            NomeCondicao = v.NomeCondicao ?? string.Empty,
            ValorSubtotal = v.ValorSubtotal,
            ValorDesconto = v.ValorDesconto,
            ValorTotal = v.ValorTotal,
            StatusVenda = v.StatusVenda,
            CriadoEm = v.CriadoEm,
            AtualizadoEm = v.AtualizadoEm,
            Itens = itens.Select(i => new VendaItemDto
            {
                Id = i.Id,
                VendaId = i.VendaId,
                ProdutoVariacaoId = i.ProdutoVariacaoId,
                NomeProduto = i.NomeProduto,
                Cor = i.Cor,
                Tamanho = i.Tamanho,
                Quantidade = i.Quantidade,
                ValorUnitario = i.ValorUnitario,
                ValorDesconto = i.ValorDesconto
            }).ToList()
        };
    }

    public Task<List<VendaItemListDto>> ObterItensAsync(int vendaId)
        => _repo.ObterItensPorVendaAsync(vendaId);

    public async Task<(bool sucesso, string mensagem, int id)> SalvarAsync(VendaDto dto)
    {
        try
        {
            var itensValidos = dto.Itens.Where(i => !i.Removido).ToList();

            if (itensValidos.Count == 0)
                return (false, "Adicione pelo menos um item à venda.", 0);

            foreach (var item in itensValidos)
            {
                if (item.ProdutoVariacaoId == 0)
                    return (false, "Selecione a variação de todos os itens.", 0);
                if (item.Quantidade <= 0)
                    return (false, $"{item.NomeProduto} {item.Cor}/{item.Tamanho}: quantidade deve ser maior que zero.", 0);
                if (item.ValorUnitario <= 0)
                    return (false, $"{item.NomeProduto} {item.Cor}/{item.Tamanho}: valor unitário deve ser maior que zero.", 0);
                if (item.ValorDesconto < 0 || item.ValorDesconto > item.ValorUnitario * item.Quantidade)
                    return (false, $"{item.NomeProduto}: desconto não pode ser maior que o valor do item.", 0);
            }
            dto.ValorSubtotal = itensValidos.Sum(i => i.ValorUnitario * i.Quantidade);
            var descItens = itensValidos.Sum(i => i.ValorDesconto);

            decimal descCond = 0, acrescCond = 0, jurosCond = 0;
            if (dto.CondicaoPagamentoId.HasValue)
            {
                var cond = await _condicaoRepo.ObterPorIdAsync(dto.CondicaoPagamentoId.Value);
                if (cond != null)
                {
                    descCond = cond.DescontoPercentual > 0 ? Math.Round(dto.ValorSubtotal * cond.DescontoPercentual / 100, 2) : 0m;
                    acrescCond = cond.AcrescimoPercentual > 0 ? Math.Round(dto.ValorSubtotal * cond.AcrescimoPercentual / 100, 2) : 0m;
                    jurosCond = cond.TaxaJurosPercentual > 0 ? Math.Round(dto.ValorSubtotal * cond.TaxaJurosPercentual / 100, 2) : 0m;
                }
            }

            dto.ValorDesconto = descItens + descCond;
            dto.ValorTotal = dto.ValorSubtotal - dto.ValorDesconto + acrescCond + jurosCond;

            if (dto.IdOriginal == 0)
            {
                var novoId = await _repo.InserirAsync(dto);
                foreach (var item in itensValidos)
                    await _repo.InserirItemAsync(item, novoId);
                return (true, "Venda criada com sucesso!", novoId);
            }
            var vendaAtual = await _repo.ObterPorIdAsync(dto.IdOriginal);
            if (vendaAtual?.StatusVenda != "ABERTA")
                return (false, "Apenas vendas em aberto podem ser editadas.", 0);

            await _repo.AtualizarAsync(dto);
            await _repo.RemoverItensAsync(dto.IdOriginal);
            foreach (var item in itensValidos)
                await _repo.InserirItemAsync(item, dto.IdOriginal);

            return (true, "Venda atualizada com sucesso!", dto.Id);
        }
        catch (Exception ex) { return (false, Erro(ex).mensagem, 0); }
    }

    public async Task<(bool sucesso, string mensagem)> FinalizarAsync(int vendaId)
    {
        try
        {
            var venda = await _repo.ObterPorIdAsync(vendaId);
            if (venda == null) return (false, "Venda não encontrada.");
            if (venda.StatusVenda != "ABERTA") return (false, "Apenas vendas em aberto podem ser finalizadas.");

            var itens = await _repo.ObterItensPorVendaAsync(vendaId);
            if (itens.Count == 0) return (false, "A venda não possui itens.");
            foreach (var item in itens)
            {
                var estoque = await _repo.ObterEstoqueAtualAsync(item.ProdutoVariacaoId);
                if (estoque < item.Quantidade)
                    return (false, $"{item.NomeProduto} {item.Cor}/{item.Tamanho}: estoque insuficiente. Disponível: {estoque} un.");
            }
            var movId = await _repo.InserirMovimentacaoSaidaAsync(vendaId);
            foreach (var item in itens)
            {
                await _repo.InserirMovimentacaoItemAsync(movId, item.ProdutoVariacaoId, item.Quantidade, item.ValorUnitario);
                await _repo.AtualizarEstoqueAsync(item.ProdutoVariacaoId, -item.Quantidade);
            }
            await _repo.AtualizarStatusAsync(vendaId, "FINALIZADA", movId);
            if (venda.ClienteId.HasValue && venda.CondicaoPagamentoId.HasValue)
            {
                var condicao = await _condicaoRepo.ObterPorIdAsync(venda.CondicaoPagamentoId.Value);
                if (condicao != null && condicao.NumeroParcelas > 0)
                {
                    var valorParcela = Math.Round(venda.ValorTotal / condicao.NumeroParcelas, 2);
                    var diferenca = venda.ValorTotal - (valorParcela * condicao.NumeroParcelas);

                    for (int p = 1; p <= condicao.NumeroParcelas; p++)
                    {
                        var vencimento = DateTime.Today.AddMonths(p);
                        var valor = p == condicao.NumeroParcelas
                            ? valorParcela + diferenca
                            : valorParcela;
                        var descricao = condicao.NumeroParcelas == 1
                            ? $"Venda #{vendaId}"
                            : $"Venda #{vendaId} — Parcela {p}/{condicao.NumeroParcelas}";

                        await _repo.InserirContaReceberAsync(venda.ClienteId.Value, vendaId, descricao, vencimento, valor);
                    }
                }
            }

            return (true, "Venda finalizada com sucesso! Estoque baixado e contas a receber geradas.");
        }
        catch (Exception ex) { return (false, $"Erro ao finalizar venda: {ex.Message}"); }
    }

    public async Task<(bool sucesso, string mensagem)> CancelarAsync(int vendaId, string motivo)
    {
        try
        {
            var venda = await _repo.ObterPorIdAsync(vendaId);
            if (venda == null) return (false, "Venda não encontrada.");
            if (venda.StatusVenda == "CANCELADA") return (false, "Venda já está cancelada.");
            if (venda.StatusVenda == "FINALIZADA" && venda.MovimentacaoId.HasValue)
            {
                var itens = await _repo.ObterItensPorVendaAsync(vendaId);
                foreach (var item in itens)
                    await _repo.AtualizarEstoqueAsync(item.ProdutoVariacaoId, +item.Quantidade);
            }

            await _repo.AtualizarStatusAsync(vendaId, "CANCELADA", motivoCancelamento: motivo);
            return (true, "Venda cancelada com sucesso!");
        }
        catch (Exception ex) { return (false, $"Erro ao cancelar venda: {ex.Message}"); }
    }
}