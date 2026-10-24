using Microsoft.AspNetCore.Components;
using MudBlazor;
using SI_Andrey_Teodoro_2026.Components.Shared;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Modals;
using SI_Andrey_Teodoro_2026.Services.Interfaces;

namespace SI_Andrey_Teodoro_2026.Pages;

public partial class NotasFiscaisPage : BaseListagemPage<NfeListDto>
{
    [Inject] private INfeService NfeService { get; set; } = null!;

    protected override FiltroConsultaDto CriarFiltro() => new() { StatusFiltro = "todos", OrdenarPor = "data" };

    protected override async Task OnInitializedAsync() => await CarregarDados();

    protected override Task<PaginacaoDto<NfeListDto>> BuscarDados(FiltroConsultaDto filtro) => NfeService.ObterTodosAsync(filtro);

    private async Task AbrirModalGeracao()
    {
        var opts = new DialogOptions { CloseOnEscapeKey = false, BackdropClick = false, MaxWidth = MaxWidth.Medium, FullWidth = true };
        var dialog = await DialogService.ShowAsync<ModalGerarNfe>("Gerar Nota Fiscal", opts);
        var result = await dialog.Result;
        if (result is { Canceled: false })
        {
            await CarregarDados();
            if (result.Data is int novoId)
                await Visualizar(novoId);
        }
    }

    private async Task Visualizar(int id)
    {
        var opts = new DialogOptions { CloseOnEscapeKey = false, BackdropClick = false, MaxWidth = MaxWidth.Medium, FullWidth = true };
        var param = new DialogParameters<ModalVisualizarNfe> { { x => x.Id, id } };
        await DialogService.ShowAsync<ModalVisualizarNfe>("Nota Fiscal", param, opts);
    }

    private async Task Cancelar(int id)
    {
        var param = new DialogParameters<ConfirmDialog>
        {
            { x => x.Titulo, "Confirmar cancelamento" },
            { x => x.Mensagem, "Deseja cancelar esta Nota Fiscal? A venda voltará a ficar disponível para gerar uma nova NF." },
            { x => x.TextoBotao, "Cancelar NF" },
            { x => x.CorBotao, Color.Error }
        };
        var dialog = await DialogService.ShowAsync<ConfirmDialog>("Confirmar", param,
            new DialogOptions { CloseOnEscapeKey = false, BackdropClick = false, MaxWidth = MaxWidth.Small });
        if ((await dialog.Result) is { Canceled: false })
        {
            var (sucesso, mensagem) = await NfeService.CancelarAsync(id);
            Snackbar.Add(mensagem, sucesso ? Severity.Success : Severity.Error);
            if (sucesso) await CarregarDados();
        }
    }
}