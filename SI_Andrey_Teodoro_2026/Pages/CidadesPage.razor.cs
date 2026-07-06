using Microsoft.AspNetCore.Components;
using MudBlazor;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Modals;
using SI_Andrey_Teodoro_2026.Services.Interfaces;

namespace SI_Andrey_Teodoro_2026.Pages;

public partial class CidadesPage : BasePage<CidadeListDto, CidadeDto>
{
    [Inject] private ICidadeService CidadeService { get; set; } = null!;
    protected override string NomeEntidade => "Cidade";

    protected override async Task OnInitializedAsync()
    {
        try { await CarregarDados(); }
        catch (Exception ex) { Snackbar.Add($"Erro ao carregar: {ex.Message}", Severity.Error); }
    }

    protected override async Task CarregarDados()
    {
        try { _carregando = true; _resultado = await CidadeService.ObterTodosAsync(_filtro); }
        catch (Exception ex) { Snackbar.Add($"Erro de banco: {ex.Message}", Severity.Error); _resultado = new(); }
        finally { _carregando = false; }
    }

    private async Task AbrirModalCadastro()
    {
        var opts = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Medium, FullWidth = true };
        var dialog = await DialogService.ShowAsync<ModalCadastroCidade>("Nova Cidade", opts);
        if ((await dialog.Result) is { Canceled: false }) await CarregarDados();
    }

    private async Task Visualizar(int id)
    {
        var dto = await CidadeService.ObterPorIdAsync(id);
        if (dto == null) { Snackbar.Add("Cidade não encontrada.", Severity.Warning); return; }
        var param = new DialogParameters<ModalCadastroCidade> { { x => x.DtoEdicao, dto }, { x => x.SomenteLeitura, true } };
        var opts = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Medium, FullWidth = true };
        var dialog = await DialogService.ShowAsync<ModalCadastroCidade>("Detalhes da Cidade", param, opts);
        if ((await dialog.Result) is { Canceled: false }) await CarregarDados();
    }

    private Task AlterarStatus(int id, string nome, bool ativoAtual)
        => ConfirmarAlteracaoStatus(id, nome, ativoAtual, CidadeService.AlterarStatusAsync, CarregarDados);
}