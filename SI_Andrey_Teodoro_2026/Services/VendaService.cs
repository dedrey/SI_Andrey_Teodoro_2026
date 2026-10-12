using System.Data;
using SI_Andrey_Teodoro_2026.Data;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Repositories.Interfaces;
using SI_Andrey_Teodoro_2026.Services.Interfaces;

namespace SI_Andrey_Teodoro_2026.Services;

public class VendaService : BaseService<VendaDto, VendaListDto>, IVendaService
{
    private readonly IVendaRepository _repo;
    private readonly ICondicaoPagamentoRepository _condicaoRepo;
    private readonly IClienteRepository _clienteRepo;
    private readonly IContaReceberRepository _contaReceberRepo;
    private readonly IContaPagarRepository _contaPagarRepo;
    private readonly IMovimentacaoEstoqueRepository _movRepo;
    private readonly DbConnectionFactory _factory;

    public VendaService(IVendaRepository repo, ICondicaoPagamentoRepository condicaoRepo, IClienteRepository clienteRepo,
        IContaReceberRepository contaReceberRepo, IContaPagarRepository contaPagarRepo,
        IMovimentacaoEstoqueRepository movRepo, DbConnectionFactory factory)
    {
        _repo = repo;
        _condicaoRepo = condicaoRepo;
        _clienteRepo = clienteRepo;
        _contaReceberRepo = contaReceberRepo;
        _contaPagarRepo = contaPagarRepo;
        _movRepo = movRepo;
        _factory = factory;
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
            DescontoPercentualAplicado = v.DescontoPercentualAplicado,
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
            var erro = await PrepararAsync(dto);
            if (erro != null) return (false, erro, 0);
            var novo = dto.IdOriginal == 0;
            var id = await GravarAsync(dto);
            return (true, novo ? "Venda criada com sucesso!" : "Venda atualizada com sucesso!", id);
        }
        catch (Exception ex) { return (false, Erro(ex).mensagem, 0); }
    }

    public async Task<(bool sucesso, string mensagem, int id)> SalvarEFinalizarAsync(VendaDto dto)
    {
        int id;
        try
        {
            var erro = await PrepararAsync(dto)
                       ?? await ValidarLimiteCreditoAsync(dto.ClienteId, dto.ValorTotal);
            if (erro != null) return (false, erro, 0);
            id = await GravarAsync(dto);
        }
        catch (Exception ex) { return (false, Erro(ex).mensagem, 0); }

        var (sucesso, mensagem) = await FinalizarAsync(id);
        return sucesso
            ? (true, mensagem, id)
            : (false, $"Venda #{id} gravada como ABERTA, mas não foi finalizada. {mensagem}", id);
    }

    private async Task<string?> PrepararAsync(VendaDto dto)
    {
        if (dto.IdOriginal != 0)
        {
            var vendaAtual = await _repo.ObterPorIdAsync(dto.IdOriginal);
            if (vendaAtual == null)
                return "Venda não encontrada.";
            if (vendaAtual.StatusVenda != "ABERTA")
                return $"Venda {vendaAtual.StatusVenda} não pode ser alterada.";
            var itensGravados = await _repo.ObterItensPorVendaAsync(dto.IdOriginal);
            if (itensGravados.Count > 0
                && (vendaAtual.ClienteId != dto.ClienteId || vendaAtual.CondicaoPagamentoId != dto.CondicaoPagamentoId))
                return "Cliente e condição de pagamento não podem ser alterados em venda com itens gravados.";
        }

        var itensValidos = dto.Itens.Where(i => !i.Removido).ToList();
        if (itensValidos.Count == 0)
            return "Adicione pelo menos um item à venda.";

        foreach (var item in itensValidos)
        {
            if (item.ProdutoVariacaoId == 0)
                return "Selecione a variação de todos os itens.";
            if (item.Quantidade <= 0)
                return $"{item.NomeProduto} {item.Cor}/{item.Tamanho}: quantidade deve ser maior que zero.";
            if (item.ValorUnitario <= 0)
                return $"{item.NomeProduto} {item.Cor}/{item.Tamanho}: valor unitário deve ser maior que zero.";
            if (item.ValorDesconto < 0 || item.ValorDesconto > item.ValorUnitario * item.Quantidade)
                return $"{item.NomeProduto}: desconto não pode ser maior que o valor do item.";

            var custoVariacao = await _repo.ObterCustoVariacaoAsync(item.ProdutoVariacaoId);
            if (custoVariacao.HasValue)
            {
                var (precoCusto, dataUltimaCompra) = custoVariacao.Value;
                if (precoCusto > 0 && item.ValorUnitario < precoCusto)
                {
                    bool fastFashion = dataUltimaCompra.HasValue && (DateTime.Today - dataUltimaCompra.Value).TotalDays > 90;
                    if (!fastFashion)
                        return $"{item.NomeProduto} {item.Cor}/{item.Tamanho}: preço de venda (R$ {item.ValorUnitario:N2}) " +
                               $"abaixo do custo (R$ {precoCusto:N2}). Produto com menos de 90 dias no estoque.";
                }
            }
        }

        var erroEstoque = await ValidarEstoqueAsync(itensValidos.Select(i =>
            (i.ProdutoVariacaoId, i.Quantidade, $"{i.NomeProduto} {i.Cor}/{i.Tamanho}")));
        if (erroEstoque != null) return erroEstoque;

        dto.ValorSubtotal = itensValidos.Sum(i => i.ValorUnitario * i.Quantidade);
        var descItens = itensValidos.Sum(i => i.ValorDesconto);

        if (dto.DescontoPercentualAplicado is < 0 or > 100)
            return "Desconto deve ser entre 0% e 100%.";

        decimal acrescCond = 0, jurosCond = 0;
        if (dto.CondicaoPagamentoId.HasValue)
        {
            var cond = await _condicaoRepo.ObterPorIdAsync(dto.CondicaoPagamentoId.Value);
            if (cond != null)
            {
                acrescCond = cond.AcrescimoPercentual > 0 ? Math.Round(dto.ValorSubtotal * cond.AcrescimoPercentual / 100, 2) : 0m;
                jurosCond = cond.TaxaJurosPercentual > 0 ? Math.Round(dto.ValorSubtotal * cond.TaxaJurosPercentual / 100, 2) : 0m;
            }
        }
        var descCond = dto.DescontoPercentualAplicado > 0
            ? Math.Round(dto.ValorSubtotal * dto.DescontoPercentualAplicado / 100, 2)
            : 0m;
        dto.ValorDesconto = descItens + descCond;
        dto.ValorTotal = dto.ValorSubtotal - dto.ValorDesconto + acrescCond + jurosCond;
        return null;
    }

    private async Task<int> GravarAsync(VendaDto dto)
    {
        var itensValidos = dto.Itens.Where(i => !i.Removido).ToList();
        if (dto.IdOriginal == 0)
        {
            var novoId = await _repo.InserirAsync(dto);
            foreach (var item in itensValidos)
                await _repo.InserirItemAsync(item, novoId);
            return novoId;
        }
        await _repo.AtualizarAsync(dto);
        await _repo.RemoverItensAsync(dto.IdOriginal);
        foreach (var item in itensValidos)
            await _repo.InserirItemAsync(item, dto.IdOriginal);
        return dto.IdOriginal;
    }

    private async Task<string?> ValidarEstoqueAsync(IEnumerable<(int VariacaoId, int Quantidade, string Descricao)> itens)
    {
        foreach (var grupo in itens.GroupBy(i => i.VariacaoId))
        {
            var estoque = await _repo.ObterEstoqueAtualAsync(grupo.Key);
            var total = grupo.Sum(i => i.Quantidade);
            if (total > estoque)
                return $"{grupo.First().Descricao}: quantidade ({total}) maior que o estoque. Disponível: {estoque} un.";
        }
        return null;
    }

    private async Task<string?> ValidarLimiteCreditoAsync(int? clienteId, decimal valorTotal)
    {
        if (!clienteId.HasValue) return null;
        var cliente = await _clienteRepo.ObterPorIdAsync(clienteId.Value);
        if (cliente == null || cliente.LimiteCredito <= 0) return null;
        var saldoDevido = await _clienteRepo.ObterSaldoDevidoAsync(clienteId.Value);
        var totalComEstaVenda = saldoDevido + valorTotal;
        return totalComEstaVenda > cliente.LimiteCredito
            ? $"Limite de crédito insuficiente para {cliente.NomeRazaoSocial}. " +
              $"Limite: R$ {cliente.LimiteCredito:N2} | Já devido: R$ {saldoDevido:N2} | " +
              $"Esta venda: R$ {valorTotal:N2} | Total ficaria: R$ {totalComEstaVenda:N2}."
            : null;
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

            var erro = await ValidarLimiteCreditoAsync(venda.ClienteId, venda.ValorTotal)
                       ?? await ValidarEstoqueAsync(itens.Select(i =>
                           (i.ProdutoVariacaoId, i.Quantidade, $"{i.NomeProduto} {i.Cor}/{i.Tamanho}")));
            if (erro != null) return (false, erro);
            var condicao = venda.ClienteId.HasValue && venda.CondicaoPagamentoId.HasValue
                ? await _condicaoRepo.ObterPorIdAsync(venda.CondicaoPagamentoId.Value)
                : null;
            var parcelas = condicao != null && condicao.NumeroParcelas > 0
                ? await _condicaoRepo.ObterParcelasAsync(venda.CondicaoPagamentoId!.Value)
                : null;

            using var conn = _factory.CreateConnection();
            if (conn.State != ConnectionState.Open) conn.Open();
            using var tx = conn.BeginTransaction();

            var movId = await _movRepo.InserirAsync("SAIDA", $"SAÍDA AUTOMÁTICA — VENDA #{vendaId}", null, tx, vendaId);
            foreach (var item in itens)
            {
                if (!await _movRepo.AtualizarEstoqueAsync(item.ProdutoVariacaoId, -item.Quantidade, tx))
                {
                    var disponivel = await _movRepo.ObterEstoqueAtualAsync(item.ProdutoVariacaoId, tx);
                    return (false, $"{item.NomeProduto} {item.Cor}/{item.Tamanho}: estoque insuficiente. Disponível: {disponivel} un.");
                }
                await _movRepo.InserirItemAsync(movId, item.ProdutoVariacaoId, item.Quantidade, item.ValorUnitario, tx);
            }
            await _repo.AtualizarStatusAsync(vendaId, "FINALIZADA", tx, movimentacaoId: movId);

            if (condicao != null && parcelas != null)
            {
                var valorRestante = venda.ValorTotal;
                if (condicao.EntradaMinimaPercentual > 0)
                {
                    var valorEntrada = Math.Round(venda.ValorTotal * condicao.EntradaMinimaPercentual / 100, 2);
                    valorRestante = venda.ValorTotal - valorEntrada;

                    var descricaoEntrada = $"VENDA #{vendaId} — ENTRADA ({condicao.EntradaMinimaPercentual:N0}%)";
                    await _repo.InserirContaReceberAsync(venda.ClienteId!.Value, vendaId, descricaoEntrada,
                        DateTime.Today, valorEntrada, tx);
                }
                var valorParcela = Math.Round(valorRestante / condicao.NumeroParcelas, 2);
                var diferenca = valorRestante - (valorParcela * condicao.NumeroParcelas);

                for (int p = 1; p <= condicao.NumeroParcelas; p++)
                {
                    var parcelaConfig = parcelas.FirstOrDefault(x => x.NumeroParcela == p);
                    var diasVencimento = parcelaConfig?.DiasVencimento ?? 30;
                    var vencimento = parcelaConfig != null
                        ? DateTime.Today.AddDays(parcelaConfig.DiasVencimento)
                        : DateTime.Today.AddMonths(p);

                    var valor = p == condicao.NumeroParcelas
                        ? valorParcela + diferenca
                        : valorParcela;

                    var descricao = condicao.NumeroParcelas == 1
                        ? $"VENDA #{vendaId}" + (condicao.EntradaMinimaPercentual > 0 ? " — SALDO" : "")
                        : $"VENDA #{vendaId} — PARCELA {p}/{condicao.NumeroParcelas}";
                    bool pagamentoInstantaneo = condicao.NumeroParcelas == 1
                                              && condicao.EntradaMinimaPercentual == 0
                                              && diasVencimento == 0;

                    await _repo.InserirContaReceberAsync(venda.ClienteId!.Value, vendaId, descricao, vencimento, valor, tx, pagamentoInstantaneo);
                }
            }

            tx.Commit();
            return (true, "Venda finalizada com sucesso! Estoque baixado e contas a receber geradas.");
        }
        catch (Exception ex) { return (false, $"Erro ao finalizar venda: {ex.Message}"); }
    }

    public async Task<(string statusVenda, string nomeCliente, decimal totalRecebido)?> ObterResumoCancelamentoAsync(int vendaId)
    {
        var venda = await _repo.ObterPorIdAsync(vendaId);
        if (venda == null) return null;
        var totalRecebido = await _contaReceberRepo.ObterTotalRecebidoDaVendaAsync(vendaId);
        return (venda.StatusVenda, venda.NomeCliente ?? string.Empty, totalRecebido);
    }

    public async Task<(bool sucesso, string mensagem)> CancelarAsync(int vendaId, string motivo)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(motivo))
                return (false, "Informe o motivo do cancelamento.");
            motivo = motivo.Trim().ToUpperInvariant();
            if (motivo.Length > 255)
                return (false, "O motivo do cancelamento deve ter no máximo 255 caracteres.");

            var venda = await _repo.ObterPorIdAsync(vendaId);
            if (venda == null) return (false, "Venda não encontrada.");
            if (venda.StatusVenda == "CANCELADA") return (false, "Venda já está cancelada.");

            var itensSaida = venda.StatusVenda == "FINALIZADA" && venda.MovimentacaoId.HasValue
                ? await _movRepo.ObterItensPorMovimentacaoAsync(venda.MovimentacaoId.Value)
                : new List<MovimentacaoEstoqueItemListDto>();

            using var conn = _factory.CreateConnection();
            if (conn.State != ConnectionState.Open) conn.Open();
            using var tx = conn.BeginTransaction();

            if (itensSaida.Count > 0)
            {
                var obsEstorno = $"ESTORNO — CANCELAMENTO DA VENDA #{vendaId}: {motivo}";
                if (obsEstorno.Length > 200) obsEstorno = obsEstorno[..200];
                var movId = await _movRepo.InserirAsync("ENTRADA", obsEstorno, null, tx, vendaId);
                foreach (var item in itensSaida)
                {
                    await _movRepo.AtualizarEstoqueAsync(item.ProdutoVariacaoId, +item.Quantidade, tx);
                    await _movRepo.InserirItemAsync(movId, item.ProdutoVariacaoId, item.Quantidade, item.ValorUnitario, tx);
                }
            }

            await _repo.AtualizarStatusAsync(vendaId, "CANCELADA", tx, motivoCancelamento: motivo);

            var contas = await _contaReceberRepo.ObterRecebidoPorContaDaVendaAsync(vendaId, tx);
            await _contaReceberRepo.CancelarPorVendaAsync(vendaId, tx);

            decimal totalEstornado = 0;
            for (int i = 0; i < contas.Count; i++)
            {
                var conta = contas[i];
                if (conta.ValorRecebido <= 0) continue;
                await _contaPagarRepo.InserirEstornoVendaAsync(conta.ClienteId, vendaId,
                    $"ESTORNO VENDA #{vendaId} — PARCELA {i + 1}/{contas.Count}",
                    DateTime.Today, conta.ValorRecebido, tx);
                totalEstornado += conta.ValorRecebido;
            }

            tx.Commit();
            return (true, totalEstornado > 0
                ? $"Venda cancelada com sucesso! Estorno de R$ {totalEstornado:N2} gerado em Contas a Pagar."
                : "Venda cancelada com sucesso!");
        }
        catch (Exception ex) { return (false, $"Erro ao cancelar venda: {ex.Message}"); }
    }
}