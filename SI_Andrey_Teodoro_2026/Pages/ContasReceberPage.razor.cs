using Microsoft.AspNetCore.Components;
using MudBlazor;
using SI_Andrey_Teodoro_2026.Components.Shared;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Modals;
using SI_Andrey_Teodoro_2026.Services.Interfaces;

namespace SI_Andrey_Teodoro_2026.Pages;

public partial class ContasReceberPage : ComponentBase
{
    [Inject] private IContaReceberService ContaReceberService { get; set; } = null!;
    [Inject] private IDialogService DialogService { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;

    private PaginacaoDto<ContaReceberListDto>? _resultado;
    private FiltroConsultaDto _filtro = new() { StatusFiltro = "todos", OrdenarPor = "vencimento" };
    private bool _carregando;

    protected override async Task OnInitializedAsync() => await CarregarDados();

    private async Task CarregarDados()
    {
        _carregando = true;
        _resultado = await ContaReceberService.ObterTodosAsync(_filtro);
        _carregando = false;
    }

    private async Task Pesquisar() { _filtro.Pagina = 1; await CarregarDados(); }
    private async Task LimparFiltros() { _filtro = new() { StatusFiltro = "todos", OrdenarPor = "vencimento" }; await CarregarDados(); }
    private async Task MudarPagina(int pagina) { _filtro.Pagina = pagina; await CarregarDados(); }

    private async Task AbrirModalCadastro()
    {
        var opts = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Medium, FullWidth = true };
        var dialog = await DialogService.ShowAsync<ModalCadastroContaReceber>("Nova Conta a Receber", opts);
        if ((await dialog.Result) is { Canceled: false }) await CarregarDados();
    }

    private async Task Editar(int id)
    {
        var dto = await ContaReceberService.ObterPorIdAsync(id);
        if (dto == null) return;
        var param = new DialogParameters<ModalCadastroContaReceber> { { x => x.DtoEdicao, dto } };
        var opts = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Medium, FullWidth = true };
        var dialog = await DialogService.ShowAsync<ModalCadastroContaReceber>(
            dto.Status == "ABERTA" ? "Editar Conta a Receber" : "Detalhes da Conta", param, opts);
        if ((await dialog.Result) is { Canceled: false }) await CarregarDados();
    }

    private async Task AbrirRegistrarRecebimento(ContaReceberListDto conta)
    {
        var param = new DialogParameters<ModalRegistrarRecebimento>
        {
            { x => x.ContaReceberId, conta.Id },
            { x => x.Descricao, conta.Descricao },
            { x => x.ValorOriginal, conta.ValorOriginal },
            { x => x.ValorSaldo, conta.ValorSaldo }
        };
        var opts = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small, FullWidth = true };
        var dialog = await DialogService.ShowAsync<ModalRegistrarRecebimento>("Registrar Recebimento", param, opts);
        if ((await dialog.Result) is { Canceled: false }) await CarregarDados();
    }

    private async Task Cancelar(int id)
    {
        var param = new DialogParameters<ConfirmDialog>
        {
            { x => x.Titulo, "Confirmar cancelamento" },
            { x => x.Mensagem, "Deseja cancelar esta conta a receber?" },
            { x => x.TextoBotao, "Cancelar Conta" },
            { x => x.CorBotao, Color.Error }
        };
        var dialog = await DialogService.ShowAsync<ConfirmDialog>("Confirmar", param,
            new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small });
        if ((await dialog.Result) is { Canceled: false })
        {
            var (sucesso, mensagem) = await ContaReceberService.CancelarAsync(id);
            Snackbar.Add(mensagem, sucesso ? Severity.Success : Severity.Error);
            if (sucesso) await CarregarDados();
        }
    }
}