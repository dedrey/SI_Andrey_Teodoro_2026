using Microsoft.AspNetCore.Components;
using MudBlazor;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Modals;
using SI_Andrey_Teodoro_2026.Services.Interfaces;

namespace SI_Andrey_Teodoro_2026.Pages;

public partial class MetodosPagamentoPage : BasePage<MetodoPagamentoListDto, MetodoPagamentoDto>
{
    [Inject] private IMetodoPagamentoService MetodoPagamentoService { get; set; } = null!;
    protected override string NomeEntidade => "Método de pagamento";

    protected override async Task OnInitializedAsync()
    {
        try { await CarregarDados(); }
        catch (Exception ex) { Snackbar.Add($"Erro ao carregar: {ex.Message}", Severity.Error); }
    }

    protected override async Task CarregarDados()
    {
        try { _carregando = true; _resultado = await MetodoPagamentoService.ObterTodosAsync(_filtro); }
        catch (Exception ex) { Snackbar.Add($"Erro de banco: {ex.Message}", Severity.Error); _resultado = new(); }
        finally { _carregando = false; }
    }

    private async Task AbrirModalCadastro()
    {
        var opts = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small, FullWidth = true };
        var dialog = await DialogService.ShowAsync<ModalCadastroMetodoPagamento>("Novo(a) Método de pagamento", opts);
        if ((await dialog.Result) is { Canceled: false }) await CarregarDados();
    }

    private async Task Visualizar(int id)
    {
        var dto = await MetodoPagamentoService.ObterPorIdAsync(id);
        if (dto == null) { Snackbar.Add("Método de pagamento não encontrado(a).", Severity.Warning); return; }
        var param = new DialogParameters<ModalCadastroMetodoPagamento> { { x => x.DtoEdicao, dto }, { x => x.SomenteLeitura, true } };
        var opts = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small, FullWidth = true };
        var dialog = await DialogService.ShowAsync<ModalCadastroMetodoPagamento>("Detalhes do Método de Pagamento", param, opts);
        if ((await dialog.Result) is { Canceled: false }) await CarregarDados();
    }

    private Task AlterarStatus(int id, string nome, bool ativoAtual)
        => ConfirmarAlteracaoStatus(id, nome, ativoAtual, MetodoPagamentoService.AlterarStatusAsync, CarregarDados);
}