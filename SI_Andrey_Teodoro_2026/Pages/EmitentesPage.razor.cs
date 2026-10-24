using Microsoft.AspNetCore.Components;
using MudBlazor;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Modals;
using SI_Andrey_Teodoro_2026.Services.Interfaces;

namespace SI_Andrey_Teodoro_2026.Pages;

public partial class EmitentesPage : BasePage<EmitenteListDto, EmitenteDto>
{
    [Inject] private IEmitenteService EmitenteService { get; set; } = null!;
    protected override string NomeEntidade => "Emitente";

    protected override async Task OnInitializedAsync()
    {
        try { await CarregarDados(); }
        catch (Exception ex) { Snackbar.Add($"Erro ao carregar: {ex.Message}", Severity.Error); }
    }
    protected override Task<PaginacaoDto<EmitenteListDto>> BuscarDados(FiltroConsultaDto filtro) => EmitenteService.ObterTodosAsync(filtro);
    private async Task AbrirModalCadastro()
    {
        var opts = new DialogOptions { CloseOnEscapeKey = false, BackdropClick = false, MaxWidth = MaxWidth.Large, FullWidth = true };
        var dialog = await DialogService.ShowAsync<ModalCadastroEmitente>("Novo Emitente", opts);
        if ((await dialog.Result) is { Canceled: false }) await CarregarDados();
    }
    private async Task Visualizar(int id)
    {
        var dto = await EmitenteService.ObterPorIdAsync(id);
        if (dto == null) { Snackbar.Add("Emitente não encontrado.", Severity.Warning); return; }
        var param = new DialogParameters<ModalCadastroEmitente> { { x => x.DtoEdicao, dto }, { x => x.SomenteLeitura, true } };
        var opts = new DialogOptions { CloseOnEscapeKey = false, BackdropClick = false, MaxWidth = MaxWidth.Large, FullWidth = true };
        var dialog = await DialogService.ShowAsync<ModalCadastroEmitente>("Detalhes do Emitente", param, opts);
        if ((await dialog.Result) is { Canceled: false }) await CarregarDados();
    }
    private Task AlterarStatus(int id, string nome, bool ativoAtual)
        => ConfirmarAlteracaoStatus(id, nome, ativoAtual, EmitenteService.AlterarStatusAsync, CarregarDados);
}