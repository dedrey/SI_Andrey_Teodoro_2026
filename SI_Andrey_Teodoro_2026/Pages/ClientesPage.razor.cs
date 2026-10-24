using Microsoft.AspNetCore.Components;
using MudBlazor;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Modals;
using SI_Andrey_Teodoro_2026.Services.Interfaces;

namespace SI_Andrey_Teodoro_2026.Pages;

public partial class ClientesPage : BasePage<ClienteListDto, ClienteDto>
{
    [Inject] private IClienteService ClienteService { get; set; } = null!;
    protected override string NomeEntidade => "Cliente";

    protected override async Task OnInitializedAsync()
    {
        try { await CarregarDados(); }
        catch (Exception ex) { Snackbar.Add($"Erro ao carregar: {ex.Message}", Severity.Error); }
    }
    protected override Task<PaginacaoDto<ClienteListDto>> BuscarDados(FiltroConsultaDto filtro) => ClienteService.ObterTodosAsync(filtro);
    private async Task AbrirModalCadastro()
    {
        var opts = new DialogOptions { CloseOnEscapeKey = false, BackdropClick = false, MaxWidth = MaxWidth.Large, FullWidth = true };
        var dialog = await DialogService.ShowAsync<ModalCadastroCliente>("Novo Cliente", opts);
        if ((await dialog.Result) is { Canceled: false }) await CarregarDados();
    }
    private Task AlterarStatus(int id, string nome, bool ativoAtual)
        => ConfirmarAlteracaoStatus(id, nome, ativoAtual, ClienteService.AlterarStatusAsync, CarregarDados);

    private async Task Visualizar(int id)
    {
        var dto = await ClienteService.ObterPorIdAsync(id);
        if (dto == null) { Snackbar.Add("Cliente não encontrado.", Severity.Warning); return; }
        var param = new DialogParameters<ModalCadastroCliente> { { x => x.DtoEdicao, dto }, { x => x.SomenteLeitura, true } };
        var opts = new DialogOptions { CloseOnEscapeKey = false, BackdropClick = false, MaxWidth = MaxWidth.Large, FullWidth = true };
        var dialog = await DialogService.ShowAsync<ModalCadastroCliente>("Detalhes do Cliente", param, opts);
        if ((await dialog.Result) is { Canceled: false }) await CarregarDados();
    }
}