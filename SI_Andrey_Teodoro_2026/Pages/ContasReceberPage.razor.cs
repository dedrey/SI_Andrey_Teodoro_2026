using Microsoft.AspNetCore.Components;
using MudBlazor;
using SI_Andrey_Teodoro_2026.Components.Shared;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Modals;
using SI_Andrey_Teodoro_2026.Services.Interfaces;

namespace SI_Andrey_Teodoro_2026.Pages;

public partial class ContasReceberPage : BaseListagemPage<ContaReceberListDto>
{
    [Inject] private IContaReceberService ContaReceberService { get; set; } = null!;

    protected override FiltroConsultaDto CriarFiltro() => new() { StatusFiltro = "todos", OrdenarPor = "vencimento" };

    protected override async Task OnInitializedAsync() => await CarregarDados();

    protected override Task<PaginacaoDto<ContaReceberListDto>> BuscarDados(FiltroConsultaDto filtro) => ContaReceberService.ObterTodosAsync(filtro);

    private async Task AbrirModalCadastro()
    {
        var opts = new DialogOptions { CloseOnEscapeKey = false, BackdropClick = false, MaxWidth = MaxWidth.Medium, FullWidth = true };
        var dialog = await DialogService.ShowAsync<ModalCadastroContaReceber>("Nova Conta a Receber", opts);
        if ((await dialog.Result) is { Canceled: false }) await CarregarDados();
    }

    private async Task Editar(int id)
    {
        var dto = await ContaReceberService.ObterPorIdAsync(id);
        if (dto == null) return;
        var param = new DialogParameters<ModalCadastroContaReceber> { { x => x.DtoEdicao, dto } };
        var opts = new DialogOptions { CloseOnEscapeKey = false, BackdropClick = false, MaxWidth = MaxWidth.Medium, FullWidth = true };
        var dialog = await DialogService.ShowAsync<ModalCadastroContaReceber>(
            dto.Status == "ABERTA" && !dto.VendaId.HasValue ? "Editar Conta a Receber" : "Detalhes da Conta", param, opts);
        if ((await dialog.Result) is { Canceled: false }) await CarregarDados();
    }

    private async Task VisualizarVenda(int vendaId)
    {
        var param = new DialogParameters<ModalDetalhesContaReceberVenda> { { x => x.VendaId, vendaId } };
        var opts = new DialogOptions { CloseOnEscapeKey = false, BackdropClick = false, MaxWidth = MaxWidth.Medium, FullWidth = true };
        var dialog = await DialogService.ShowAsync<ModalDetalhesContaReceberVenda>($"Venda #{vendaId}", param, opts);
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
        var opts = new DialogOptions { CloseOnEscapeKey = false, BackdropClick = false, MaxWidth = MaxWidth.Small, FullWidth = true };
        var dialog = await DialogService.ShowAsync<ModalRegistrarRecebimento>("Registrar Recebimento", param, opts);
        if ((await dialog.Result) is { Canceled: false }) await CarregarDados();
    }

    private async Task AbrirAnexarComprovante(int baixaId)
    {
        var param = new DialogParameters<ModalAnexarComprovante> { { x => x.BaixaId, baixaId } };
        var opts = new DialogOptions { CloseOnEscapeKey = false, BackdropClick = false, MaxWidth = MaxWidth.Small, FullWidth = true };
        var dialog = await DialogService.ShowAsync<ModalAnexarComprovante>("Anexar Comprovante", param, opts);
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
            new DialogOptions { CloseOnEscapeKey = false, BackdropClick = false, MaxWidth = MaxWidth.Small });
        if ((await dialog.Result) is { Canceled: false })
        {
            var (sucesso, mensagem) = await ContaReceberService.CancelarAsync(id);
            Snackbar.Add(mensagem, sucesso ? Severity.Success : Severity.Error);
            if (sucesso) await CarregarDados();
        }
    }
}