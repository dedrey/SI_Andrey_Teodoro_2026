using Microsoft.AspNetCore.Components;
using MudBlazor;
using SI_Andrey_Teodoro_2026.Components.Shared;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Services.Interfaces;

namespace SI_Andrey_Teodoro_2026.Pages;

public partial class PaisesPage : ComponentBase
{
    [Inject] private IPaisService PaisService { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;
    [Inject] private IDialogService DialogService { get; set; } = null!;

    private PaginacaoDto<PaisListDto>? _resultado;
    private FiltroConsultaDto _filtro = new();
    private PaisDto _dto = new();
    private MudForm _form = null!;
    private bool _formValido;
    private bool _carregando;
    private bool _salvando;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            await CarregarDados();
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Erro ao carregar: {ex.Message}", Severity.Error);
        }
    }

    private async Task CarregarDados()
    {
        try
        {
            _carregando = true;
            _resultado = await PaisService.ObterTodosAsync(_filtro);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Erro de banco: {ex.Message}", Severity.Error);
            _resultado = new PaginacaoDto<PaisListDto>();
        }
        finally
        {
            _carregando = false;
        }
    }

    private async Task Pesquisar()
    {
        _filtro.Pagina = 1;
        await CarregarDados();
    }

    private async Task LimparFiltros()
    {
        _filtro = new FiltroConsultaDto();
        await CarregarDados();
    }

    private async Task MudarPagina(int p) { _filtro.Pagina = p; await CarregarDados(); }
    private void LimparFormulario()
    {
        _dto = new PaisDto();
        _form?.ResetAsync();
    }

    private async Task Editar(int id)
    {
        var pais = await PaisService.ObterPorIdAsync(id);
        if (pais == null) { Snackbar.Add("País não encontrado.", Severity.Warning); return; }
        _dto = pais;
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

        if (sucesso)
        {
            _dto = new PaisDto();
            await _form.ResetAsync();
            await CarregarDados();
        }
    }

    private async Task AlterarStatus(int id, string nome, bool ativoAtual)
    {
        var acao = ativoAtual ? "desativar" : "ativar";
        var param = new DialogParameters<ConfirmDialog>
        {
            { x => x.Titulo,     $"Confirmar {acao}" },
            { x => x.Mensagem,   $"Deseja realmente {acao} o país \"{nome}\"?" },
            { x => x.TextoBotao, ativoAtual ? "Desativar" : "Ativar" },
            { x => x.CorBotao,   ativoAtual ? Color.Error : Color.Success }
        };
        var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small };
        var dialog = await DialogService.ShowAsync<ConfirmDialog>("Confirmar", param, options);
        var result = await dialog.Result;

        if (result is { Canceled: false })
        {
            var (sucesso, mensagem) = await PaisService.AlterarStatusAsync(id, !ativoAtual);
            Snackbar.Add(mensagem, sucesso ? Severity.Success : Severity.Error);
            if (sucesso) await CarregarDados();
        }
    }
}
