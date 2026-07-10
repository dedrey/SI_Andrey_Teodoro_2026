using Microsoft.AspNetCore.Components;
using MudBlazor;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Modals;
using SI_Andrey_Teodoro_2026.Services.Interfaces;

namespace SI_Andrey_Teodoro_2026.Pages;

public partial class VendasPage : BasePage<VendaListDto, VendaDto>
{
    [Inject] private IVendaService VendaService { get; set; } = null!;

    protected override string NomeEntidade => "Venda";
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
    private readonly Dictionary<int, List<VendaItemListDto>?> _itensCache = new();

    protected override async Task OnInitializedAsync()
    {
        _filtro.StatusFiltro = "todos";
        await CarregarDados();
    }

    protected override async Task CarregarDados()
    {
        try { _carregando = true; _resultado = await VendaService.ObterTodosAsync(_filtro); }
        catch (Exception ex) { Snackbar.Add($"Erro: {ex.Message}", Severity.Error); _resultado = new(); }
        finally { _carregando = false; }
    }

    private async Task AbrirModalCadastro()
    {
        var opts = new DialogOptions { CloseOnEscapeKey = true, BackdropClick = false, MaxWidth = MaxWidth.Large, FullWidth = true };
        var dialog = await DialogService.ShowAsync<ModalCadastroVenda>("Nova Venda", opts);
        if ((await dialog.Result) is { Canceled: false }) { _itensCache.Clear(); _expandidos.Clear(); await CarregarDados(); }
    }

    private async Task AbrirCancelamento(int id)
    {
        var opts = new DialogOptions { CloseOnEscapeKey = true, BackdropClick = false, MaxWidth = MaxWidth.Small, FullWidth = true };
        var param = new DialogParameters<ModalCancelamentoVenda> { { x => x.VendaId, id } };
        var dialog = await DialogService.ShowAsync<ModalCancelamentoVenda>("Cancelar Venda", param, opts);
        var result = await dialog.Result;
        if (result is { Canceled: false } && result.Data is string motivo)
        {
            var (sucesso, mensagem) = await VendaService.CancelarAsync(id, motivo);
            Snackbar.Add(mensagem, sucesso ? Severity.Success : Severity.Error);
            if (sucesso)
            {
                _recarregarAposRender = true;
                StateHasChanged();
            }
        }
    }

    private async Task FinalizarVenda(int id)
    {
        var (sucesso, msg) = await VendaService.FinalizarAsync(id);
        Snackbar.Add(msg, sucesso ? Severity.Success : Severity.Error);
        if (sucesso) await CarregarDados();
    }

    private async Task AbrirEdicao(int id)
    {
        var dto = await VendaService.ObterPorIdAsync(id);
        if (dto == null) { Snackbar.Add("Venda não encontrada.", Severity.Warning); return; }
        var opts = new DialogOptions { CloseOnEscapeKey = true, BackdropClick = false, MaxWidth = MaxWidth.Large, FullWidth = true };
        var param = new DialogParameters<ModalCadastroVenda> { { x => x.DtoEdicao, dto } };
        var dialog = await DialogService.ShowAsync<ModalCadastroVenda>("Alterar Venda", param, opts);
        if ((await dialog.Result) is { Canceled: false }) { _itensCache.Remove(id); await CarregarDados(); }
    }

    private async Task ToggleItens(int vendaId)
    {
        if (_expandidos.Contains(vendaId)) { _expandidos.Remove(vendaId); return; }
        _expandidos.Add(vendaId);
        if (!_itensCache.ContainsKey(vendaId))
        {
            _itensCache[vendaId] = null; StateHasChanged();
            _itensCache[vendaId] = await VendaService.ObterItensAsync(vendaId);
            StateHasChanged();
        }
    }
}