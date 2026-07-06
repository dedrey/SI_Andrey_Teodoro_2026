using Microsoft.AspNetCore.Components;
using MudBlazor;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Modals;
using SI_Andrey_Teodoro_2026.Services.Interfaces;

namespace SI_Andrey_Teodoro_2026.Pages;

public partial class VeiculosPage : BasePage<VeiculoListDto, VeiculoDto>
{
    [Inject] private IVeiculoService VeiculoService { get; set; } = null!;
    protected override string NomeEntidade => "Veículo";

    protected override async Task OnInitializedAsync()
    {
        try { await CarregarDados(); }
        catch (Exception ex) { Snackbar.Add($"Erro ao carregar: {ex.Message}", Severity.Error); }
    }

    protected override async Task CarregarDados()
    {
        try { _carregando = true; _resultado = await VeiculoService.ObterTodosAsync(_filtro); }
        catch (Exception ex) { Snackbar.Add($"Erro de banco: {ex.Message}", Severity.Error); _resultado = new(); }
        finally { _carregando = false; }
    }

    private async Task AbrirModalCadastro()
    {
        var opts = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Medium, FullWidth = true };
        var dialog = await DialogService.ShowAsync<ModalCadastroVeiculo>("Novo Veículo", opts);
        if ((await dialog.Result) is { Canceled: false }) await CarregarDados();
    }

    private async Task Visualizar(int id)
    {
        var dto = await VeiculoService.ObterPorIdAsync(id);
        if (dto == null) { Snackbar.Add("Veículo não encontrado.", Severity.Warning); return; }
        var param = new DialogParameters<ModalCadastroVeiculo> { { x => x.DtoEdicao, dto }, { x => x.SomenteLeitura, true } };
        var opts = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Medium, FullWidth = true };
        var dialog = await DialogService.ShowAsync<ModalCadastroVeiculo>("Detalhes do Veículo", param, opts);
        if ((await dialog.Result) is { Canceled: false }) await CarregarDados();
    }

    private Task AlterarStatus(int id, string placa, bool ativoAtual)
        => ConfirmarAlteracaoStatus(id, placa, ativoAtual, VeiculoService.AlterarStatusAsync, CarregarDados);
}