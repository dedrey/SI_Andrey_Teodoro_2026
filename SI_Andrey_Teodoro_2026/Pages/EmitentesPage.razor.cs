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
    protected override async Task CarregarDados()
    {
        try { _carregando = true; _resultado = await EmitenteService.ObterTodosAsync(_filtro); }
        catch (Exception ex) { Snackbar.Add($"Erro: {ex.Message}", Severity.Error); _resultado = new(); }
        finally { _carregando = false; }
    }
    private async Task AbrirModalCadastro()
    {
        var opts = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Large, FullWidth = true };
        var dialog = await DialogService.ShowAsync<ModalCadastroEmitente>("Novo Emitente", opts);
        if ((await dialog.Result) is { Canceled: false }) await CarregarDados();
    }
    private async Task Visualizar(int id)
    {
        var opts = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Medium, FullWidth = true };
        var param = new DialogParameters<ModalVisualizarEmitente> { { x => x.Id, id } };
        await DialogService.ShowAsync<ModalVisualizarEmitente>("Detalhes do Emitente", param, opts);
    }
    private async Task Editar(int id)
    {
        var dto = await EmitenteService.ObterPorIdAsync(id);
        if (dto == null) { Snackbar.Add("Emitente não encontrado.", Severity.Warning); return; }
        var param = new DialogParameters<ModalCadastroEmitente> { { x => x.DtoEdicao, dto } };
        var opts = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Large, FullWidth = true };
        var dialog = await DialogService.ShowAsync<ModalCadastroEmitente>("Editar Emitente", param, opts);
        if ((await dialog.Result) is { Canceled: false }) await CarregarDados();
    }
    private Task AlterarStatus(int id, string nome, bool ativoAtual)
        => ConfirmarAlteracaoStatus(id, nome, ativoAtual, EmitenteService.AlterarStatusAsync, CarregarDados);
    private static string FormatCnpj(string s) { var d = new string(s.Where(char.IsDigit).ToArray()); return d.Length == 14 ? $"{d[..2]}.{d[2..5]}.{d[5..8]}/{d[8..12]}-{d[12..]}" : s; }
}