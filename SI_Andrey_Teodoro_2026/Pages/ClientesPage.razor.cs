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
    protected override async Task CarregarDados()
    {
        try { _carregando = true; _resultado = await ClienteService.ObterTodosAsync(_filtro); }
        catch (Exception ex) { Snackbar.Add($"Erro: {ex.Message}", Severity.Error); _resultado = new(); }
        finally { _carregando = false; }
    }
    private async Task AbrirModalCadastro()
    {
        var opts = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Large, FullWidth = true };
        var dialog = await DialogService.ShowAsync<ModalCadastroCliente>("Novo Cliente", opts);
        if ((await dialog.Result) is { Canceled: false }) await CarregarDados();
    }
    private async Task Editar(int id)
    {
        var dto = await ClienteService.ObterPorIdAsync(id);
        if (dto == null) { Snackbar.Add("Cliente não encontrado.", Severity.Warning); return; }
        var param = new DialogParameters<ModalCadastroCliente> { { x => x.DtoEdicao, dto } };
        var opts = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Large, FullWidth = true };
        var dialog = await DialogService.ShowAsync<ModalCadastroCliente>("Editar Cliente", param, opts);
        if ((await dialog.Result) is { Canceled: false }) await CarregarDados();
    }
    private Task AlterarStatus(int id, string nome, bool ativoAtual)
        => ConfirmarAlteracaoStatus(id, nome, ativoAtual, ClienteService.AlterarStatusAsync, CarregarDados);

    private static string FormatDoc(string? doc, string tipo)
    {
        if (string.IsNullOrWhiteSpace(doc)) return "—";
        var d = new string(doc.Where(char.IsDigit).ToArray());
        if (tipo == "PF" && d.Length == 11) return $"{d[..3]}.{d[3..6]}.{d[6..9]}-{d[9..]}";
        if (tipo == "PJ" && d.Length == 14) return $"{d[..2]}.{d[2..5]}.{d[5..8]}/{d[8..12]}-{d[12..]}";
        return doc;
    }
    private async Task Visualizar(int id)
    {
        var opts = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Medium, FullWidth = true };
        var param = new DialogParameters<ModalVisualizarCliente> { { x => x.Id, id } };
        await DialogService.ShowAsync<ModalVisualizarCliente>("Detalhes do Cliente", param, opts);
    }
}