using Microsoft.AspNetCore.Components;
using MudBlazor;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Modals;
using SI_Andrey_Teodoro_2026.Services.Interfaces;

namespace SI_Andrey_Teodoro_2026.Pages;

public partial class ContasPagarPage : BaseListagemPage<ContaPagarListDto>
{
    [Inject] private IContaPagarService ContaPagarService { get; set; } = null!;

    protected override FiltroConsultaDto CriarFiltro() => new() { StatusFiltro = "todos", OrdenarPor = "vencimento" };

    protected override async Task OnInitializedAsync() => await CarregarDados();

    protected override Task<PaginacaoDto<ContaPagarListDto>> BuscarDados(FiltroConsultaDto filtro) => ContaPagarService.ObterTodosAsync(filtro);

    private async Task AbrirModalCadastro()
    {
        var opts = new DialogOptions { CloseOnEscapeKey = false, BackdropClick = false, MaxWidth = MaxWidth.Medium, FullWidth = true };
        var dialog = await DialogService.ShowAsync<ModalCadastroContaPagar>("Nova Conta a Pagar", opts);
        if ((await dialog.Result) is { Canceled: false }) await CarregarDados();
    }

    private async Task Visualizar(int id)
    {
        var dto = await ContaPagarService.ObterPorIdAsync(id);
        if (dto == null) return;
        var param = new DialogParameters<ModalCadastroContaPagar> { { x => x.DtoEdicao, dto }, { x => x.SomenteLeitura, true } };
        var opts = new DialogOptions { CloseOnEscapeKey = false, BackdropClick = false, MaxWidth = MaxWidth.Medium, FullWidth = true };
        var dialog = await DialogService.ShowAsync<ModalCadastroContaPagar>("Detalhes da Conta", param, opts);
        if ((await dialog.Result) is { Canceled: false }) await CarregarDados();
    }
}