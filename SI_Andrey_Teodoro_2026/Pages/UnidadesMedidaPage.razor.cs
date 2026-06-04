using Microsoft.AspNetCore.Components;
using MudBlazor;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Services.Interfaces;
namespace SI_Andrey_Teodoro_2026.Pages;
public partial class UnidadesMedidaPage : BasePage<UnidadeMedidaListDto, UnidadeMedidaDto>
{
    [Inject] private IUnidadeMedidaService UnidadeMedidaService { get; set; } = null!;
    protected override string NomeEntidade => "Unidade de medida";
    protected override async Task OnInitializedAsync()
    {
        try { await CarregarDados(); }
        catch (Exception ex) { Snackbar.Add($"Erro ao carregar: {ex.Message}", Severity.Error); }
    }
    protected override async Task CarregarDados()
    {
        try
        {
            _carregando = true;
            _resultado = await UnidadeMedidaService.ObterTodosAsync(_filtro);
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
        var u = await UnidadeMedidaService.ObterPorIdAsync(id);
        if (u == null) { Snackbar.Add("Unidade de medida não encontrada.", Severity.Warning); return; }
        _dto = u;
        StateHasChanged();
    }
    private async Task Salvar()
    {
        await _form.ValidateAsync();
        if (!_formValido) return;
        _salvando = true;
        var (sucesso, mensagem, _) = await UnidadeMedidaService.SalvarAsync(_dto);
        _salvando = false;
        Snackbar.Add(mensagem, sucesso ? Severity.Success : Severity.Error);
        if (sucesso) { LimparFormulario(); await CarregarDados(); }
    }
    private Task AlterarStatus(int id, string sigla, bool ativoAtual)
        => ConfirmarAlteracaoStatus(id, sigla, ativoAtual,
               UnidadeMedidaService.AlterarStatusAsync, CarregarDados);
}