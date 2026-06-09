using Microsoft.AspNetCore.Components;
using MudBlazor;
using SI_Andrey_Teodoro_2026.Components.Shared;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Services.Interfaces;
using SI_Andrey_Teodoro_2026.Modals;

namespace SI_Andrey_Teodoro_2026.Pages;

public partial class VendasPage : BasePage<VendaListDto, VendaDto>
{
    [Inject] private IVendaService VendaService { get; set; } = null!;
    [Inject] private IProdutoService ProdutoService { get; set; } = null!;
    [Inject] private IClienteService ClienteService { get; set; } = null!;
    [Inject] private ICondicaoPagamentoService CondicaoPagamentoService { get; set; } = null!;

    protected override string NomeEntidade => "Venda";

    protected List<ProdutoListDto> _todosProdutos = new();
    protected List<ClienteListDto> _todosClientes = new();
    protected List<CondicaoPagamentoListDto> _condicoes = new();

    private ClienteListDto? _clienteSelecionado;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var prodTask = ProdutoService.ObterTodosAtivosAsync();
            var cliTask = ClienteService.ObterTodosAtivosAsync();
            var condTask = CondicaoPagamentoService.ObterTodosAtivosAsync();
            await Task.WhenAll(prodTask, cliTask, condTask);
            _todosProdutos = (await prodTask).ToList();
            _todosClientes = (await cliTask).ToList();
            _condicoes = (await condTask).ToList();
            await CarregarDados();
        }
        catch (Exception ex) { Snackbar.Add($"Erro ao carregar: {ex.Message}", Severity.Error); }
    }

    protected override async Task CarregarDados()
    {
        try
        {
            _carregando = true;
            _resultado = await VendaService.ObterTodosAsync(_filtro);
        }
        catch (Exception ex) { Snackbar.Add($"Erro: {ex.Message}", Severity.Error); _resultado = new(); }
        finally { _carregando = false; }
    }

    private void AdicionarItem()
    {
        var item = new VendaItemDto();
        _dto.Itens.Add(item);
        _produtosSelecionados[item] = null;
        _variacoesPorItem[item] = new();
        _valorTextos[item] = "";
        _descontoTextos[item] = "";
        StateHasChanged();
    }

    private void RemoverItem(VendaItemDto item)
    {
        _dto.Itens.Remove(item);
        _produtosSelecionados.Remove(item);
        _variacoesPorItem.Remove(item);
        _valorTextos.Remove(item);
        _descontoTextos.Remove(item);
        AtualizarTotaisDto();
        StateHasChanged();
    }

    private void LimparFormulario()
    {
        _dto = new VendaDto();
        _clienteSelecionado = null;
        _valorTextos.Clear();
        _descontoTextos.Clear();
        _produtosSelecionados.Clear();
        _variacoesPorItem.Clear();
        _form?.ResetAsync();
    }

    private async Task Editar(int id)
    {
        var v = await VendaService.ObterPorIdAsync(id);
        if (v == null) { Snackbar.Add("Venda não encontrada.", Severity.Warning); return; }
        _dto = v;
        _clienteSelecionado = _todosClientes.FirstOrDefault(c => c.Id == v.ClienteId);
        _valorTextos.Clear();
        _descontoTextos.Clear();
        _produtosSelecionados.Clear();
        _variacoesPorItem.Clear();
        foreach (var item in _dto.Itens)
        {
            _valorTextos[item] = item.ValorUnitario > 0 ? item.ValorUnitario.ToString("N2") : "";
            _descontoTextos[item] = item.ValorDesconto > 0 ? item.ValorDesconto.ToString("N2") : "";
            _produtosSelecionados[item] = _todosProdutos.FirstOrDefault(p => p.Id == item.ProdutoId);
            _variacoesPorItem[item] = new();
        }
        StateHasChanged();
    }

    private async Task Salvar()
    {
        _salvando = true;
        var (sucesso, mensagem, _) = await VendaService.SalvarAsync(_dto);
        _salvando = false;
        Snackbar.Add(mensagem, sucesso ? Severity.Success : Severity.Error);
        if (sucesso) { LimparFormulario(); await CarregarDados(); }
    }

    private async Task AbrirModalCliente()
    {
        var opts = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Large, FullWidth = true };
        var dialog = await DialogService.ShowAsync<SI_Andrey_Teodoro_2026.Modals.ModalCadastroCliente>("Novo Cliente", opts);
        var result = await dialog.Result;
        _todosClientes = (await ClienteService.ObterTodosAtivosAsync()).ToList();
        if (result is { Canceled: false, Data: int novoId })
        {
            _clienteSelecionado = _todosClientes.FirstOrDefault(c => c.Id == novoId);
            _dto.ClienteId = novoId;
            _dto.NomeCliente = _clienteSelecionado?.NomeRazaoSocial ?? "";
        }
        StateHasChanged();
    }

    private async Task AbrirModalCondicao()
    {
        var opts = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Medium, FullWidth = true };
        var dialog = await DialogService.ShowAsync<SI_Andrey_Teodoro_2026.Modals.ModalCadastroCondicaoPagamento>("Nova Condição de Pagamento", opts);
        var result = await dialog.Result;
        _condicoes = (await CondicaoPagamentoService.ObterTodosAtivosAsync()).ToList();
        if (result is { Canceled: false, Data: int novoId })
            OnCondicaoAlterada(novoId);
        StateHasChanged();
    }

    private void OnCondicaoAlterada(int? id)
    {
        _dto.CondicaoPagamentoId = id;
        AtualizarTotaisDto();
        StateHasChanged();
    }
    private void AtualizarTotaisDto()
    {
        var subtotal = _dto.Itens.Where(i => !i.Removido).Sum(i => i.ValorUnitario * i.Quantidade);
        var descItens = _dto.Itens.Where(i => !i.Removido).Sum(i => i.ValorDesconto);
        var cond = _condicoes.FirstOrDefault(c => c.Id == _dto.CondicaoPagamentoId);

        var descCond = cond != null && cond.DescontoPercentual > 0 ? Math.Round(subtotal * cond.DescontoPercentual / 100, 2) : 0m;
        var acrescCond = cond != null && cond.AcrescimoPercentual > 0 ? Math.Round(subtotal * cond.AcrescimoPercentual / 100, 2) : 0m;
        var jurosCond = cond != null && cond.TaxaJurosPercentual > 0 ? Math.Round(subtotal * cond.TaxaJurosPercentual / 100, 2) : 0m;

        _dto.ValorSubtotal = subtotal;
        _dto.ValorDesconto = descItens + descCond;
        _dto.ValorTotal = subtotal - _dto.ValorDesconto + acrescCond + jurosCond;
    }

    private async Task ConfirmarFinalizar(int vendaId)
    {
        var param = new DialogParameters<ConfirmDialog>
        {
            { x => x.Titulo,     "Finalizar Venda" },
            { x => x.Mensagem,   $"Deseja finalizar a Venda #{vendaId}? O estoque será baixado e as contas a receber serão geradas automaticamente." },
            { x => x.TextoBotao, "Finalizar" },
            { x => x.CorBotao,   Color.Success }
        };
        var dialog = await DialogService.ShowAsync<ConfirmDialog>("Finalizar",
            param, new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small });
        if ((await dialog.Result) is { Canceled: false })
        {
            var (sucesso, mensagem) = await VendaService.FinalizarAsync(vendaId);
            Snackbar.Add(mensagem, sucesso ? Severity.Success : Severity.Error);
            if (sucesso) { _detalhesCache.Remove(vendaId); await CarregarDados(); }
        }
    }

    private async Task ConfirmarCancelar(int vendaId)
    {
        var venda = _resultado?.Itens.FirstOrDefault(v => v.Id == vendaId);
        var param = new DialogParameters<ModalCancelamentoVenda>
        {
            { x => x.VendaId,        vendaId },
            { x => x.EraFinalizada,  venda?.StatusVenda == "FINALIZADA" }
        };
        var dialog = await DialogService.ShowAsync<ModalCancelamentoVenda>("Cancelar Venda",
            param, new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small, FullWidth = true });
        var result = await dialog.Result;
        if (result is { Canceled: false, Data: string motivo })
        {
            var (sucesso, mensagem) = await VendaService.CancelarAsync(vendaId, motivo);
            Snackbar.Add(mensagem, sucesso ? Severity.Success : Severity.Error);
            if (sucesso) await CarregarDados();
        }
    }
    private decimal RodapeSubtotal => _dto.Itens.Where(i => !i.Removido).Sum(i => i.ValorUnitario * i.Quantidade);
    private decimal RodapeDescItens => _dto.Itens.Where(i => !i.Removido).Sum(i => i.ValorDesconto);
    private CondicaoPagamentoListDto? RodapeCond => _condicoes.FirstOrDefault(c => c.Id == _dto.CondicaoPagamentoId);
    private decimal RodapeDescCond => RodapeCond?.DescontoPercentual > 0 ? Math.Round(RodapeSubtotal * RodapeCond!.DescontoPercentual / 100, 2) : 0m;
    private decimal RodapeAcrescCond => RodapeCond?.AcrescimoPercentual > 0 ? Math.Round(RodapeSubtotal * RodapeCond!.AcrescimoPercentual / 100, 2) : 0m;
    private decimal RodapeJurosCond => RodapeCond?.TaxaJurosPercentual > 0 ? Math.Round(RodapeSubtotal * RodapeCond!.TaxaJurosPercentual / 100, 2) : 0m;
    private decimal RodapeTotalDesc => RodapeDescItens + RodapeDescCond;
    private decimal RodapeTotal => RodapeSubtotal - RodapeTotalDesc + RodapeAcrescCond + RodapeJurosCond;

    private void OnQuantidadeAlterada(VendaItemDto item, string v)
    {
        var s = v ?? "";
        var d = new string(s.Where(char.IsDigit).ToArray());
        var novaQtd = int.TryParse(d, out int n) && n > 0 ? n : 1;

        if (item.EstoqueAtual > 0 && novaQtd > item.EstoqueAtual)
        {
            item.Quantidade = novaQtd;
            Snackbar.Add(
                $"{item.NomeProduto} {item.Cor}/{item.Tamanho}: apenas {item.EstoqueAtual} un. em estoque.",
                Severity.Warning,
                cfg => { cfg.DuplicatesBehavior = SnackbarDuplicatesBehavior.Prevent; cfg.VisibleStateDuration = 3000; });
        }
        else
        {
            item.Quantidade = novaQtd;
        }

        AtualizarTotaisDto();
        StateHasChanged();
    }

    private readonly Dictionary<VendaItemDto, string> _valorTextos = new();
    private readonly Dictionary<VendaItemDto, string> _descontoTextos = new();
    private readonly Dictionary<VendaItemDto, ProdutoListDto?> _produtosSelecionados = new();
    private readonly Dictionary<VendaItemDto, List<ProdutoVariacaoDto>> _variacoesPorItem = new();
    private readonly HashSet<int> _expandidos = new();
    private readonly Dictionary<int, List<VendaItemListDto>?> _detalhesCache = new();
    private string GetValorTexto(Dictionary<VendaItemDto, string> dict, VendaItemDto item)
    {
        if (!dict.ContainsKey(item)) dict[item] = "";
        return dict[item];
    }

    private void OnDecimalAlterado(Dictionary<VendaItemDto, string> dict, VendaItemDto item,
                                   string? texto, Action<decimal> setter)
    {
        if (string.IsNullOrEmpty(texto)) { dict[item] = ""; setter(0); return; }
        var filtrado = new string(texto.Where(c => char.IsDigit(c) || c == ',' || c == '.').ToArray());
        dict[item] = filtrado;
        setter(decimal.TryParse(filtrado.Replace(",", "."),
            System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out decimal d) ? d : 0);
        AtualizarTotaisDto();
        StateHasChanged();
    }

    private async Task OnProdutoAlterado(VendaItemDto item, ProdutoListDto? produto)
    {
        _produtosSelecionados[item] = produto;
        item.ProdutoVariacaoId = 0;
        item.NomeProduto = produto?.Produto ?? "";
        item.ProdutoId = produto?.Id ?? 0;
        item.EstoqueAtual = 0;
        if (produto != null)
        {
            var dto = await ProdutoService.ObterPorIdAsync(produto.Id);
            _variacoesPorItem[item] = dto?.Variacoes.Where(v => v.Ativo && v.QuantidadeEstoque > 0).ToList() ?? new();
        }
        else { _variacoesPorItem[item] = new(); }
        StateHasChanged();
    }

    private void OnVariacaoAlterada(VendaItemDto item, int variacaoId)
    {
        item.ProdutoVariacaoId = variacaoId;
        if (_variacoesPorItem.TryGetValue(item, out var vars))
        {
            var v = vars.FirstOrDefault(x => x.IdOriginal == variacaoId);
            if (v != null)
            {
                item.Cor = v.Cor;
                item.Tamanho = v.Tamanho;
                item.EstoqueAtual = v.QuantidadeEstoque;
                if (item.ValorUnitario == 0)
                {
                    item.ValorUnitario = v.Preco;
                    _valorTextos[item] = v.Preco.ToString("N2");
                }
            }
        }
        StateHasChanged();
    }

    private Task<IEnumerable<ProdutoListDto>> PesquisarProduto(string? busca, CancellationToken ct)
    {
        var r = string.IsNullOrWhiteSpace(busca)
            ? _todosProdutos.Take(50)
            : _todosProdutos.Where(p => p.Produto.Contains(busca, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(r);
    }

    private Task<IEnumerable<ClienteListDto>> PesquisarCliente(string? busca, CancellationToken ct)
    {
        var r = string.IsNullOrWhiteSpace(busca)
            ? _todosClientes.Take(50)
            : _todosClientes.Where(c => c.NomeRazaoSocial.Contains(busca, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(r);
    }

    private async Task ToggleDetalhe(int vendaId)
    {
        if (_expandidos.Contains(vendaId)) { _expandidos.Remove(vendaId); return; }
        _expandidos.Add(vendaId);
        if (!_detalhesCache.ContainsKey(vendaId))
        {
            _detalhesCache[vendaId] = null;
            StateHasChanged();
            _detalhesCache[vendaId] = await VendaService.ObterItensAsync(vendaId);
            StateHasChanged();
        }
    }

}