using Microsoft.AspNetCore.Components;
using MudBlazor;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Modals;
using SI_Andrey_Teodoro_2026.Services.Interfaces;

namespace SI_Andrey_Teodoro_2026.Pages;

public partial class ProdutosPage : BasePage<ProdutoListDto, ProdutoDto>
{
    [Inject] private IProdutoService ProdutoService { get; set; } = null!;
    [Inject] private ICategoriaService CategoriaService { get; set; } = null!;
    [Inject] private IMarcaService MarcaService { get; set; } = null!;
    [Inject] private IUnidadeMedidaService UnidadeMedidaService { get; set; } = null!;

    protected override string NomeEntidade => "Produto";

    private readonly HashSet<int> _expandidos = new();
    private readonly Dictionary<int, List<ProdutoVariacaoDto>?> _variacoesCache = new();

    protected override async Task OnInitializedAsync()
        => await CarregarDados();

    protected override async Task CarregarDados()
    {
        try { _carregando = true; _resultado = await ProdutoService.ObterTodosAsync(_filtro); }
        catch (Exception ex) { Snackbar.Add($"Erro: {ex.Message}", Severity.Error); _resultado = new(); }
        finally { _carregando = false; }
    }

    private async Task AbrirModalCadastro()
    {
        var opts = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Large, FullWidth = true };
        var dialog = await DialogService.ShowAsync<ModalCadastroProduto>("Novo Produto", opts);
        if ((await dialog.Result) is { Canceled: false }) await CarregarDados();
    }

    private async Task Editar(int id)
    {
        var dto = await ProdutoService.ObterPorIdAsync(id);
        if (dto == null) { Snackbar.Add("Produto não encontrado.", Severity.Warning); return; }
        var opts = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Large, FullWidth = true };
        var param = new DialogParameters<ModalCadastroProduto> { { x => x.DtoEdicao, dto } };
        var dialog = await DialogService.ShowAsync<ModalCadastroProduto>("Editar Produto", param, opts);
        if ((await dialog.Result) is { Canceled: false }) { _variacoesCache.Remove(id); await CarregarDados(); }
    }

    private async Task ToggleExpansao(int produtoId)
    {
        if (_expandidos.Contains(produtoId)) { _expandidos.Remove(produtoId); return; }
        _expandidos.Add(produtoId);
        if (!_variacoesCache.ContainsKey(produtoId))
        {
            _variacoesCache[produtoId] = null; StateHasChanged();
            var dto = await ProdutoService.ObterPorIdAsync(produtoId);
            _variacoesCache[produtoId] = dto?.Variacoes.Where(v => !v.Removida).ToList();
            StateHasChanged();
        }
    }


    private Task AlterarStatus(int id, string nome, bool ativoAtual)
        => ConfirmarAlteracaoStatus(id, nome, ativoAtual,
            async (i, a) => await ProdutoService.AlterarStatusAsync(i, a), CarregarDados);
}