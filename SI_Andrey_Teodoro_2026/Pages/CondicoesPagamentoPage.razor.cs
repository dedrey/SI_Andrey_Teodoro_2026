using Microsoft.AspNetCore.Components;
using MudBlazor;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Modals;
using SI_Andrey_Teodoro_2026.Services.Interfaces;

namespace SI_Andrey_Teodoro_2026.Pages;

public partial class CondicoesPagamentoPage : BasePage<CondicaoPagamentoListDto, CondicaoPagamentoDto>
{
    [Inject] private ICondicaoPagamentoService CondicaoPagamentoService { get; set; } = null!;
    protected override string NomeEntidade => "Condição de pagamento";

    protected override async Task OnInitializedAsync()
    {
        try { await CarregarDados(); }
        catch (Exception ex) { Snackbar.Add($"Erro ao carregar: {ex.Message}", Severity.Error); }
    }
    protected override Task<PaginacaoDto<CondicaoPagamentoListDto>> BuscarDados(FiltroConsultaDto filtro) => CondicaoPagamentoService.ObterTodosAsync(filtro);
    private async Task AbrirModalCadastro()
    {
        var opts = new DialogOptions { CloseOnEscapeKey = false, BackdropClick = false, MaxWidth = MaxWidth.Medium, FullWidth = true };
        var dialog = await DialogService.ShowAsync<ModalCadastroCondicaoPagamento>("Nova Condição de Pagamento", opts);
        if ((await dialog.Result) is { Canceled: false }) await CarregarDados();
    }
    private async Task Visualizar(int id)
    {
        var dto = await CondicaoPagamentoService.ObterPorIdAsync(id);
        if (dto == null) { Snackbar.Add("Condição não encontrada.", Severity.Warning); return; }
        var param = new DialogParameters<ModalCadastroCondicaoPagamento> { { x => x.DtoEdicao, dto }, { x => x.SomenteLeitura, true } };
        var opts = new DialogOptions { CloseOnEscapeKey = false, BackdropClick = false, MaxWidth = MaxWidth.Medium, FullWidth = true };
        var dialog = await DialogService.ShowAsync<ModalCadastroCondicaoPagamento>("Detalhes da Condição de Pagamento", param, opts);
        if ((await dialog.Result) is { Canceled: false }) await CarregarDados();
    }
    private Task AlterarStatus(int id, string nome, bool ativoAtual)
        => ConfirmarAlteracaoStatus(id, nome, ativoAtual, CondicaoPagamentoService.AlterarStatusAsync, CarregarDados);
}