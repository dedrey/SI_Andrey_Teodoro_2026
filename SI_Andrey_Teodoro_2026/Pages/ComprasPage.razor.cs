using Microsoft.AspNetCore.Components;
using MudBlazor;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Modals;
using SI_Andrey_Teodoro_2026.Services.Interfaces;

namespace SI_Andrey_Teodoro_2026.Pages;

public partial class ComprasPage : BasePage<CompraListDto, CompraDto>
{
    [Inject] private ICompraService CompraService { get; set; } = null!;

    protected override string NomeEntidade => "Compra";
    private bool _recarregarAposRender = false;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_recarregarAposRender)
        {
            _recarregarAposRender = false;
            _itensCache.Clear();
            _expandidos.Clear();
            await Pesquisar();
            StateHasChanged();
        }
    }

    private readonly HashSet<int> _expandidos = new();
    private readonly Dictionary<int, List<CompraItemListDto>?> _itensCache = new();

    protected override async Task OnInitializedAsync()
    {
        _filtro.StatusFiltro = "todos";
        await CarregarDados();
    }

    protected override async Task CarregarDados()
    {
        try { _carregando = true; _resultado = await CompraService.ObterTodosAsync(_filtro); }
        catch (Exception ex) { Snackbar.Add($"Erro: {ex.Message}", Severity.Error); _resultado = new(); }
        finally { _carregando = false; }
    }

    private async Task AbrirModalCadastro()
    {
        var opts = new DialogOptions { CloseOnEscapeKey = true, BackdropClick = false, MaxWidth = MaxWidth.Large, FullWidth = true };
        var dialog = await DialogService.ShowAsync<ModalCadastroCompra>("Nova Compra", opts);
        if ((await dialog.Result) is { Canceled: false }) { _itensCache.Clear(); _expandidos.Clear(); await CarregarDados(); }
    }

    private async Task AbrirCancelamento(int id)
    {
        var opts = new DialogOptions { CloseOnEscapeKey = true, BackdropClick = false, MaxWidth = MaxWidth.Small, FullWidth = true };
        var param = new DialogParameters<ModalCancelamentoCompra> { { x => x.CompraId, id } };
        var dialog = await DialogService.ShowAsync<ModalCancelamentoCompra>("Cancelar Compra", param, opts);
        var result = await dialog.Result;
        if (result is { Canceled: false } && result.Data is string motivo)
        {
            var (sucesso, mensagem) = await CompraService.CancelarAsync(id, motivo);
            Snackbar.Add(mensagem, sucesso ? Severity.Success : Severity.Error);
            if (sucesso)
            {
                _recarregarAposRender = true;
                StateHasChanged();
            }
        }
    }

    private async Task AbrirVisualizacao(int id)
    {
        var dto = await CompraService.ObterPorIdAsync(id);
        if (dto == null) { Snackbar.Add("Compra não encontrada.", Severity.Warning); return; }
        var opts = new DialogOptions { CloseOnEscapeKey = true, BackdropClick = false, MaxWidth = MaxWidth.Large, FullWidth = true };
        var param = new DialogParameters<ModalCadastroCompra> { { x => x.DtoVisualizacao, dto }, { x => x.SomenteLeitura, true } };
        await DialogService.ShowAsync<ModalCadastroCompra>("Visualizar Compra", param, opts);
    }

    private async Task ToggleItens(int compraId)
    {
        if (_expandidos.Contains(compraId)) { _expandidos.Remove(compraId); return; }
        _expandidos.Add(compraId);
        if (!_itensCache.ContainsKey(compraId))
        {
            _itensCache[compraId] = null; StateHasChanged();
            _itensCache[compraId] = await CompraService.ObterItensAsync(compraId);
            StateHasChanged();
        }
    }
}
