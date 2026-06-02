using Microsoft.AspNetCore.Components;
using MudBlazor;
using SI_Andrey_Teodoro_2026.Components;
using SI_Andrey_Teodoro_2026.Components.Shared;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Services.Interfaces;
using SI_Andrey_Teodoro_2026.Modals;

namespace SI_Andrey_Teodoro_2026.Pages;

public partial class ProdutosPage : ComponentBase
{
    [Inject] private IProdutoService ProdutoService { get; set; } = null!;
    [Inject] private ICategoriaService CategoriaService { get; set; } = null!;
    [Inject] private IMarcaService MarcaService { get; set; } = null!;
    [Inject] private IUnidadeMedidaService UnidadeMedidaService { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;
    [Inject] private IDialogService DialogService { get; set; } = null!;

    private PaginacaoDto<ProdutoListDto>? _resultado;
    private FiltroConsultaDto _filtro = new();
    private ProdutoDto _dto = new();
    private MudForm _form = null!;
    private bool _formValido;
    private bool _carregando;
    private bool _salvando;

    private List<CategoriaListDto> _categorias = new();
    private List<MarcaListDto> _marcas = new();
    private List<UnidadeMedidaListDto> _unidades = new();

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var catTask = CategoriaService.ObterTodosAtivosAsync();
            var marTask = MarcaService.ObterTodosAtivosAsync();
            var uniTask = UnidadeMedidaService.ObterTodosAtivosAsync();
            await Task.WhenAll(catTask, marTask, uniTask);
            _categorias = (await catTask).ToList();
            _marcas = (await marTask).ToList();
            _unidades = (await uniTask).ToList();
            await CarregarDados();
        }
        catch (Exception ex) { Snackbar.Add($"Erro ao carregar: {ex.Message}", Severity.Error); }
    }

    private async Task CarregarDados()
    {
        try
        {
            _carregando = true;
            _resultado = await ProdutoService.ObterTodosAsync(_filtro);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Erro de banco: {ex.Message}", Severity.Error);
            _resultado = new();
        }
        finally { _carregando = false; }
    }

    private async Task Pesquisar() { _filtro.Pagina = 1; await CarregarDados(); }
    private async Task LimparFiltros() { _filtro = new(); await CarregarDados(); }
    private async Task MudarPagina(int p) { _filtro.Pagina = p; await CarregarDados(); }

    private void LimparFormulario()
    {
        _dto = new();
        _precoTextos.Clear();
        _form?.ResetAsync();
    }

    private async Task AbrirModalCategoria()
    {
        var opts = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small, FullWidth = true };
        var dialog = await DialogService.ShowAsync<ModalCadastroCategoria>("Nova Categoria", opts);
        var result = await dialog.Result;
        if (result is { Canceled: false })
        {
            _categorias = (await CategoriaService.ObterTodosAtivosAsync()).ToList();
            if (result.Data is int novoId) _dto.CategoriaId = novoId;
        }
    }

    private async Task AbrirModalMarca()
    {
        var opts = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small, FullWidth = true };
        var dialog = await DialogService.ShowAsync<ModalCadastroMarca>("Nova Marca", opts);
        var result = await dialog.Result;
        if (result is { Canceled: false })
        {
            _marcas = (await MarcaService.ObterTodosAtivosAsync()).ToList();
            if (result.Data is int novoId) _dto.MarcaId = novoId;
        }
    }

    private async Task AbrirModalUnidade()
    {
        var opts = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small, FullWidth = true };
        var dialog = await DialogService.ShowAsync<ModalCadastroUnidadeMedida>("Nova Unidade de Medida", opts);
        var result = await dialog.Result;
        if (result is { Canceled: false })
        {
            _unidades = (await UnidadeMedidaService.ObterTodosAtivosAsync()).ToList();
            if (result.Data is int novoId) _dto.UnidadeMedidaId = novoId;
        }
    }

    private void AdicionarVariacao()
    {
        var nova = new ProdutoVariacaoDto();
        _dto.Variacoes.Add(nova);
        StateHasChanged();
    }

    private void RemoverVariacao(ProdutoVariacaoDto v)
    {
        if (v.IdOriginal == 0)
        {
            _dto.Variacoes.Remove(v);
            _precoTextos.Remove(v);
        }
        else
        {
            v.Removida = true;
        }
        StateHasChanged();
    }

    private async Task AlterarEstoqueInline(ProdutoVariacaoDto v, int delta)
    {
        var novaQtd = v.QuantidadeEstoque + delta;
        if (novaQtd < 0) return;

        var (sucesso, mensagem) = await ProdutoService.AtualizarEstoqueAsync(v.IdOriginal, novaQtd);
        if (sucesso)
        {
            v.QuantidadeEstoque = novaQtd;
            StateHasChanged();
        }
        else Snackbar.Add(mensagem, Severity.Error);
    }

    private async Task AlterarStatusVariacao(ProdutoVariacaoDto v)
    {
        var ativar = !v.Ativo;
        var param = new DialogParameters<ConfirmDialog>
        {
            { x => x.Titulo,     $"Confirmar {(v.Ativo ? "desativar" : "ativar")}" },
            { x => x.Mensagem,   $"Deseja realmente {(v.Ativo ? "desativar" : "ativar")} a variação {v.Cor}/{v.Tamanho}?" },
            { x => x.TextoBotao, v.Ativo ? "Desativar" : "Ativar" },
            { x => x.CorBotao,   v.Ativo ? Color.Error : Color.Success }
        };
        var dialog = await DialogService.ShowAsync<ConfirmDialog>("Confirmar",
            param, new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small });
        if ((await dialog.Result) is { Canceled: false })
        {
            var (sucesso, mensagem) = await ProdutoService.AlterarStatusVariacaoAsync(v.IdOriginal, ativar);
            Snackbar.Add(mensagem, sucesso ? Severity.Success : Severity.Error);
            if (sucesso) { v.Ativo = ativar; StateHasChanged(); }
        }
    }

    private async Task Editar(int id)
    {
        var p = await ProdutoService.ObterPorIdAsync(id);
        if (p == null) { Snackbar.Add("Produto não encontrado.", Severity.Warning); return; }
        _dto = p;
        _precoTextos.Clear();
        foreach (var v in _dto.Variacoes)
            _precoTextos[v] = v.Preco > 0 ? v.Preco.ToString("N2") : "";
        StateHasChanged();
    }

    private async Task Salvar()
    {
        if (_dto.CategoriaId == 0) { Snackbar.Add("⚠️ Selecione uma Categoria.", Severity.Warning); return; }
        if (_dto.MarcaId == 0) { Snackbar.Add("⚠️ Selecione uma Marca.", Severity.Warning); return; }
        if (_dto.UnidadeMedidaId == 0) { Snackbar.Add("⚠️ Selecione uma Unidade de Medida.", Severity.Warning); return; }

        var variacoesAtivas = _dto.Variacoes.Where(v => !v.Removida).ToList();
        foreach (var v in variacoesAtivas)
        {
            if (string.IsNullOrWhiteSpace(v.Cor)) { Snackbar.Add("Preencha a cor de todas as variações.", Severity.Warning); return; }
            if (string.IsNullOrWhiteSpace(v.Tamanho)) { Snackbar.Add("Preencha o tamanho de todas as variações.", Severity.Warning); return; }
            if (v.Preco <= 0) { Snackbar.Add($"Variação {v.Cor}/{v.Tamanho}: preço inválido.", Severity.Warning); return; }
        }

        await _form.ValidateAsync();
        if (!_formValido) return;

        _salvando = true;
        var (sucesso, mensagem, _) = await ProdutoService.SalvarAsync(_dto);
        _salvando = false;

        Snackbar.Add(mensagem, sucesso ? Severity.Success : Severity.Error);
        if (sucesso) { LimparFormulario(); await CarregarDados(); }
    }

    private async Task AlterarStatus(int id, string nome, bool ativoAtual)
    {
        var param = new DialogParameters<ConfirmDialog>
        {
            { x => x.Titulo,     $"Confirmar {(ativoAtual ? "desativar" : "ativar")}" },
            { x => x.Mensagem,   $"Deseja realmente {(ativoAtual ? "desativar" : "ativar")} o produto \"{nome}\"?" },
            { x => x.TextoBotao, ativoAtual ? "Desativar" : "Ativar" },
            { x => x.CorBotao,   ativoAtual ? Color.Error : Color.Success }
        };
        var dialog = await DialogService.ShowAsync<ConfirmDialog>("Confirmar",
            param, new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small });
        if ((await dialog.Result) is { Canceled: false })
        {
            var (sucesso, mensagem) = await ProdutoService.AlterarStatusAsync(id, !ativoAtual);
            Snackbar.Add(mensagem, sucesso ? Severity.Success : Severity.Error);
            if (sucesso) await CarregarDados();
        }
    }
}