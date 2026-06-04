using Microsoft.AspNetCore.Components;
using MudBlazor;
using SI_Andrey_Teodoro_2026.DTOs;
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
        try
        {
            _carregando = true;
            _resultado = await MarcaService.ObterTodosAsync(_filtro);
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
        var m = await MarcaService.ObterPorIdAsync(id);
        if (m == null) { Snackbar.Add("Marca não encontrada.", Severity.Warning); return; }
        _dto = m;
        StateHasChanged();
    }
    private async Task Salvar()
    {
        await _form.ValidateAsync();
        if (!_formValido) return;
        _salvando = true;
        var (sucesso, mensagem, _) = await MarcaService.SalvarAsync(_dto);
        _salvando = false;
        Snackbar.Add(mensagem, sucesso ? Severity.Success : Severity.Error);
        if (sucesso) { LimparFormulario(); await CarregarDados(); }
    }
    private Task AlterarStatus(int id, string nome, bool ativoAtual)
        => ConfirmarAlteracaoStatus(id, nome, ativoAtual,
               MarcaService.AlterarStatusAsync, CarregarDados);
}