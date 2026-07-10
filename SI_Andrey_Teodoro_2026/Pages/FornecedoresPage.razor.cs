using Microsoft.AspNetCore.Components;
using MudBlazor;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Modals;
using SI_Andrey_Teodoro_2026.Services.Interfaces;

namespace SI_Andrey_Teodoro_2026.Pages;

public partial class FornecedoresPage : BasePage<FornecedorListDto, FornecedorDto>
{
    [Inject] private IFornecedorService FornecedorService { get; set; } = null!;
    protected override string NomeEntidade => "Fornecedor";

    protected override async Task OnInitializedAsync()
    {
        try { await CarregarDados(); }
        catch (Exception ex) { Snackbar.Add($"Erro ao carregar: {ex.Message}", Severity.Error); }
    }

    protected override async Task CarregarDados()
    {
        try { _carregando = true; _resultado = await FornecedorService.ObterTodosAsync(_filtro); }
        catch (Exception ex) { Snackbar.Add($"Erro de banco: {ex.Message}", Severity.Error); _resultado = new(); }
        finally { _carregando = false; }
    }

    private async Task AbrirModalCadastro()
    {
        var opts = new DialogOptions { CloseOnEscapeKey = true, BackdropClick = false, MaxWidth = MaxWidth.Large, FullWidth = true };
        var dialog = await DialogService.ShowAsync<ModalCadastroFornecedor>("Novo Fornecedor", opts);
        if ((await dialog.Result) is { Canceled: false }) await CarregarDados();
    }

    private Task AlterarStatus(int id, string nome, bool ativoAtual)
        => ConfirmarAlteracaoStatus(id, nome, ativoAtual, FornecedorService.AlterarStatusAsync, CarregarDados);
    private async Task Visualizar(int id)
    {
        var dto = await FornecedorService.ObterPorIdAsync(id);
        if (dto == null) { Snackbar.Add("Fornecedor não encontrado.", Severity.Warning); return; }
        var param = new DialogParameters<ModalCadastroFornecedor> { { x => x.DtoEdicao, dto }, { x => x.SomenteLeitura, true } };
        var opts = new DialogOptions { CloseOnEscapeKey = true, BackdropClick = false, MaxWidth = MaxWidth.Large, FullWidth = true };
        var dialog = await DialogService.ShowAsync<ModalCadastroFornecedor>("Detalhes do Fornecedor", param, opts);
        if ((await dialog.Result) is { Canceled: false }) await CarregarDados();
    }
}