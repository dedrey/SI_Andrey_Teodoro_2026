using Microsoft.AspNetCore.Components;
using MudBlazor;
using SI_Andrey_Teodoro_2026.Components.Shared;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Modals;
using SI_Andrey_Teodoro_2026.Services.Interfaces;

namespace SI_Andrey_Teodoro_2026.Pages;

public partial class ContasPagarPage : ComponentBase
{
    [Inject] private IContaPagarService ContaPagarService { get; set; } = null!;
    [Inject] private IDialogService DialogService { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;

    private PaginacaoDto<ContaPagarListDto>? _resultado;
    private FiltroConsultaDto _filtro = new() { StatusFiltro = "todos", OrdenarPor = "vencimento" };
    private bool _carregando;

    protected override async Task OnInitializedAsync() => await CarregarDados();

    private async Task CarregarDados()
    {
        _carregando = true;
        _resultado = await ContaPagarService.ObterTodosAsync(_filtro);
        _carregando = false;
    }

    private async Task Pesquisar() { _filtro.Pagina = 1; await CarregarDados(); }
    private async Task LimparFiltros() { _filtro = new() { StatusFiltro = "todos", OrdenarPor = "vencimento" }; await CarregarDados(); }
    private async Task MudarPagina(int pagina) { _filtro.Pagina = pagina; await CarregarDados(); }

    private async Task AbrirModalCadastro()
    {
        var opts = new DialogOptions { CloseOnEscapeKey = true, BackdropClick = false, MaxWidth = MaxWidth.Medium, FullWidth = true };
        var dialog = await DialogService.ShowAsync<ModalCadastroContaPagar>("Nova Conta a Pagar", opts);
        if ((await dialog.Result) is { Canceled: false }) await CarregarDados();
    }

    private async Task Editar(int id)
    {
        var dto = await ContaPagarService.ObterPorIdAsync(id);
        if (dto == null) return;
        var param = new DialogParameters<ModalCadastroContaPagar> { { x => x.DtoEdicao, dto } };
        var opts = new DialogOptions { CloseOnEscapeKey = true, BackdropClick = false, MaxWidth = MaxWidth.Medium, FullWidth = true };
        var dialog = await DialogService.ShowAsync<ModalCadastroContaPagar>(
            dto.Status == "ABERTA" ? "Editar Conta a Pagar" : "Detalhes da Conta", param, opts);
        if ((await dialog.Result) is { Canceled: false }) await CarregarDados();
    }

    private async Task AbrirFinalizarPagamento(ContaPagarListDto conta)
    {
        var param = new DialogParameters<ModalFinalizarPagamento>
        {
            { x => x.ContaId, conta.Id },
            { x => x.Descricao, conta.Descricao },
            { x => x.ValorOriginal, conta.ValorOriginal }
        };
        var opts = new DialogOptions { CloseOnEscapeKey = true, BackdropClick = false, MaxWidth = MaxWidth.Small, FullWidth = true };
        var dialog = await DialogService.ShowAsync<ModalFinalizarPagamento>("Finalizar Pagamento", param, opts);
        if ((await dialog.Result) is { Canceled: false }) await CarregarDados();
    }

    private async Task Cancelar(int id)
    {
        var param = new DialogParameters<ConfirmDialog>
        {
            { x => x.Titulo, "Confirmar cancelamento" },
            { x => x.Mensagem, "Deseja cancelar esta conta a pagar?" },
            { x => x.TextoBotao, "Cancelar Conta" },
            { x => x.CorBotao, Color.Error }
        };
        var dialog = await DialogService.ShowAsync<ConfirmDialog>("Confirmar", param,
            new DialogOptions { CloseOnEscapeKey = true, BackdropClick = false, MaxWidth = MaxWidth.Small });
        if ((await dialog.Result) is { Canceled: false })
        {
            var (sucesso, mensagem) = await ContaPagarService.CancelarAsync(id);
            Snackbar.Add(mensagem, sucesso ? Severity.Success : Severity.Error);
            if (sucesso) await CarregarDados();
        }
    }
}