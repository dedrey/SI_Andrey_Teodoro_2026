using Microsoft.AspNetCore.Components;
using MudBlazor;
using SI_Andrey_Teodoro_2026.Components;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Services.Interfaces;

namespace SI_Andrey_Teodoro_2026.Pages;

public partial class PaisesPage : BasePage<PaisListDto, PaisDto>
{
    [Inject] private IPaisService PaisService { get; set; } = null!;

    protected override string NomeEntidade => "País";

    protected override void InicializarDto() => _dto = new PaisDto();

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
            _resultado = await PaisService.ObterTodosAsync(_filtro);
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
        var p = await PaisService.ObterPorIdAsync(id);
        if (p == null) { Snackbar.Add("País não encontrado.", Severity.Warning); return; }
        _dto = p;
        StateHasChanged();
    }

    private async Task Salvar()
    {
        await _form.ValidateAsync();
        if (!_formValido) return;
        _salvando = true;
        var (sucesso, mensagem, _) = await PaisService.SalvarAsync(_dto);
        _salvando = false;
        Snackbar.Add(mensagem, sucesso ? Severity.Success : Severity.Error);
        if (sucesso) { _dto = new(); await _form.ResetAsync(); await CarregarDados(); }
    }

    private Task AlterarStatus(int id, string nome, bool ativoAtual)
        => ConfirmarAlteracaoStatus(id, nome, ativoAtual, PaisService.AlterarStatusAsync, CarregarDados);
}