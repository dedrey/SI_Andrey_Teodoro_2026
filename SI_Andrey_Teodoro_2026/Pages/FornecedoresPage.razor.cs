using Microsoft.AspNetCore.Components;
using MudBlazor;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Modals;
using SI_Andrey_Teodoro_2026.Services.Interfaces;

namespace SI_Andrey_Teodoro_2026.Pages;

public partial class FornecedoresPage : BasePage<FornecedorListDto, FornecedorDto>
{
    [Inject] private IFornecedorService FornecedorService { get; set; } = null!;
    [Inject] private ICidadeService CidadeService { get; set; } = null!;

    protected override string NomeEntidade => "Fornecedor";

    // _cnpjTexto declarado no FornecedoresPage.razor (@code) — não declarar aqui
    protected List<CidadeListDto> _cidades = new();

    protected override async Task OnInitializedAsync()
    {
        try
        {
            _cidades = (await CidadeService.ObterTodosAtivosSemPaginacaoAsync())
                .Where(c => c.NomePais.Equals("Brasil", StringComparison.OrdinalIgnoreCase))
                .ToList();
            await CarregarDados();
        }
        catch (Exception ex) { Snackbar.Add($"Erro ao carregar: {ex.Message}", Severity.Error); }
    }

    protected override async Task CarregarDados()
    {
        try
        {
            _carregando = true;
            _resultado = await FornecedorService.ObterTodosAsync(_filtro);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Erro de banco: {ex.Message}", Severity.Error);
            _resultado = new();
        }
        finally { _carregando = false; }
    }

    private void LimparFormulario()
    {
        _dto = new();
        _form?.ResetAsync();
    }

    private async Task Editar(int id)
    {
        var f = await FornecedorService.ObterPorIdAsync(id);
        if (f == null) { Snackbar.Add("Fornecedor não encontrado.", Severity.Warning); return; }
        _dto = f;
        StateHasChanged();
    }

    private async Task Salvar()
    {
        await _form.ValidateAsync();
        if (!_formValido) return;
        _salvando = true;
        var (sucesso, mensagem, _) = await FornecedorService.SalvarAsync(_dto);
        _salvando = false;
        Snackbar.Add(mensagem, sucesso ? Severity.Success : Severity.Error);
        if (sucesso) { LimparFormulario(); await CarregarDados(); }
    }

    private async Task AbrirModalCidade()
    {
        var opts = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Medium, FullWidth = true };
        var dialog = await DialogService.ShowAsync<ModalCadastroCidade>("Nova Cidade", opts);
        var result = await dialog.Result;
        if (result is { Canceled: false })
        {
            _cidades = (await CidadeService.ObterTodosAtivosSemPaginacaoAsync())
                .Where(c => c.NomePais.Equals("Brasil", StringComparison.OrdinalIgnoreCase))
                .ToList();
            if (result.Data is int novoId)
                _dto.CidadeId = novoId;
        }
    }

    private Task AlterarStatus(int id, string nome, bool ativoAtual)
        => ConfirmarAlteracaoStatus(id, nome, ativoAtual,
               FornecedorService.AlterarStatusAsync, CarregarDados);
}