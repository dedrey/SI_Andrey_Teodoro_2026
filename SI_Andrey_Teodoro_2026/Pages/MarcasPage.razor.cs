using Microsoft.AspNetCore.Components;
using MudBlazor;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Modals;
using SI_Andrey_Teodoro_2026.Services.Interfaces;

namespace SI_Andrey_Teodoro_2026.Pages;

public partial class MarcasPage : BasePage<MarcaListDto, MarcaDto>
{
    [Inject] private IMarcaService MarcaService { get; set; } = null!;
    protected override string NomeEntidade => "Marca";

    protected override async Task OnInitializedAsync()
    {
        try { await CarregarDados(); }
        catch (Exception ex) { Snackbar.Add($"Erro ao carregar: {ex.Message}", Severity.Error); }
    }

    protected override async Task CarregarDados()
    {
        try { _carregando = true; _resultado = await MarcaService.ObterTodosAsync(_filtro); }
        catch (Exception ex) { Snackbar.Add($"Erro de banco: {ex.Message}", Severity.Error); _resultado = new(); }
        finally { _carregando = false; }
    }

    private async Task AbrirModalCadastro()
    {
        var opts = new DialogOptions { CloseOnEscapeKey = true, BackdropClick = false, MaxWidth = MaxWidth.Small, FullWidth = true };
        var dialog = await DialogService.ShowAsync<ModalCadastroMarca>("Novo(a) Marca", opts);
        if ((await dialog.Result) is { Canceled: false }) await CarregarDados();
    }

    private async Task Visualizar(int id)
    {
        var dto = await MarcaService.ObterPorIdAsync(id);
        if (dto == null) { Snackbar.Add("Marca não encontrado(a).", Severity.Warning); return; }
        var param = new DialogParameters<ModalCadastroMarca> { { x => x.DtoEdicao, dto }, { x => x.SomenteLeitura, true } };
        var opts = new DialogOptions { CloseOnEscapeKey = true, BackdropClick = false, MaxWidth = MaxWidth.Small, FullWidth = true };
        var dialog = await DialogService.ShowAsync<ModalCadastroMarca>("Detalhes da Marca", param, opts);
        if ((await dialog.Result) is { Canceled: false }) await CarregarDados();
    }

    private Task AlterarStatus(int id, string nome, bool ativoAtual)
        => ConfirmarAlteracaoStatus(id, nome, ativoAtual, MarcaService.AlterarStatusAsync, CarregarDados);
}