using Microsoft.AspNetCore.Components;
using MudBlazor;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Modals;
using SI_Andrey_Teodoro_2026.Services.Interfaces;

namespace SI_Andrey_Teodoro_2026.Pages;

public partial class MovimentacoesEstoquePage : BasePage<MovimentacaoEstoqueListDto, MovimentacaoEstoqueDto>
{
    [Inject] private IMovimentacaoEstoqueService MovimentacaoService { get; set; } = null!;

    protected override string NomeEntidade => "Movimentação de estoque";

    private readonly HashSet<int> _expandidos = new();
    private readonly Dictionary<int, List<MovimentacaoEstoqueItemListDto>?> _detalhesCache = new();

    protected override async Task OnInitializedAsync() => await CarregarDados();

    protected override async Task CarregarDados()
    {
        try { _carregando = true; _resultado = await MovimentacaoService.ObterTodosAsync(_filtro); }
        catch (Exception ex) { Snackbar.Add($"Erro: {ex.Message}", Severity.Error); _resultado = new(); }
        finally { _carregando = false; }
    }

    private async Task AbrirModalCadastro()
    {
        var opts = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Large, FullWidth = true };
        var dialog = await DialogService.ShowAsync<ModalCadastroMovimentacaoEstoque>("Nova Movimentação", opts);
        if ((await dialog.Result) is { Canceled: false }) { _detalhesCache.Clear(); _expandidos.Clear(); await CarregarDados(); }
    }

    private async Task ToggleDetalhe(int movId)
    {
        if (_expandidos.Contains(movId)) { _expandidos.Remove(movId); return; }
        _expandidos.Add(movId);
        if (!_detalhesCache.ContainsKey(movId))
        {
            _detalhesCache[movId] = null; StateHasChanged();
            _detalhesCache[movId] = await MovimentacaoService.ObterItensAsync(movId);
            StateHasChanged();
        }
    }
}