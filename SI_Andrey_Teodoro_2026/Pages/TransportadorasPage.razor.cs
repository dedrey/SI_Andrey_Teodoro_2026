using Microsoft.AspNetCore.Components;
using MudBlazor;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Modals;
using SI_Andrey_Teodoro_2026.Services.Interfaces;

namespace SI_Andrey_Teodoro_2026.Pages;

public partial class TransportadorasPage : BasePage<TransportadoraListDto, TransportadoraDto>
{
    [Inject] private ITransportadoraService TransportadoraService { get; set; } = null!;

    protected override string NomeEntidade => "Transportadora";

    protected override async Task OnInitializedAsync()
    {
        await CarregarDados();
    }

    protected override async Task CarregarDados()
    {
        try
        {
            _carregando = true;
            _resultado = await TransportadoraService.ObterTodosAsync(_filtro);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Erro: {ex.Message}", Severity.Error);
            _resultado = new();
        }
        finally
        {
            _carregando = false;
            StateHasChanged();
        }
    }

    private async Task AbrirModalCadastro()
    {
        var opts = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Medium, FullWidth = true };
        var dialog = await DialogService.ShowAsync<ModalCadastroTransportadora>("Nova Transportadora", opts);
        if ((await dialog.Result) is { Canceled: false })
            await CarregarDados();
    }

    private async Task AbrirModalEdicao(int id)
    {
        var dto = await TransportadoraService.ObterPorIdAsync(id);
        if (dto == null) { Snackbar.Add("Transportadora não encontrada.", Severity.Warning); return; }

        var param = new DialogParameters<ModalCadastroTransportadora> { { x => x.DtoInicial, dto } };
        var opts = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Medium, FullWidth = true };
        var dialog = await DialogService.ShowAsync<ModalCadastroTransportadora>("Editar Transportadora", param, opts);
        if ((await dialog.Result) is { Canceled: false })
            await CarregarDados();
    }

    private Task AlterarStatus(int id, string nome, bool ativoAtual)
        => ConfirmarAlteracaoStatus(id, nome, ativoAtual,
               TransportadoraService.AlterarStatusAsync, CarregarDados);
}