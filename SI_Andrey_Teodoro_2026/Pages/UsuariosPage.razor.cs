using Microsoft.AspNetCore.Components;
using MudBlazor;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Services.Interfaces;

namespace SI_Andrey_Teodoro_2026.Pages;

public partial class UsuariosPage : BasePage<UsuarioListDto, UsuarioDto>
{
    [Inject] private IUsuarioService UsuarioService { get; set; } = null!;

    protected override string NomeEntidade => "Usuário";

    private string _senhaTexto = "";
    private bool _mostrarSenha = false;

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
            _resultado = await UsuarioService.ObterTodosAsync(_filtro);
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
        _senhaTexto = "";
        _mostrarSenha = false;
        _form?.ResetAsync();
    }

    private async Task Editar(int id)
    {
        var u = await UsuarioService.ObterPorIdAsync(id);
        if (u == null) { Snackbar.Add("Usuário não encontrado.", Severity.Warning); return; }
        _dto = u;
        _senhaTexto = "";
        _mostrarSenha = false;
        StateHasChanged();
    }

    private async Task Salvar()
    {
        await _form.ValidateAsync();
        if (!_formValido) return;
        _salvando = true;
        var (sucesso, mensagem, _) = await UsuarioService.SalvarAsync(_dto);
        _salvando = false;
        Snackbar.Add(mensagem, sucesso ? Severity.Success : Severity.Error);
        if (sucesso) { LimparFormulario(); await CarregarDados(); }
    }

    private Task AlterarStatus(int id, string nome, bool ativoAtual)
        => ConfirmarAlteracaoStatus(id, nome, ativoAtual,
               UsuarioService.AlterarStatusAsync, CarregarDados);
}