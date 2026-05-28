using Microsoft.AspNetCore.Components;
using MudBlazor;
using SI_Andrey_Teodoro_2026.Components;
using SI_Andrey_Teodoro_2026.Components.Shared;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Services.Interfaces;

namespace SI_Andrey_Teodoro_2026.Pages;

public partial class EstadosPage : ComponentBase
{
    [Inject] private IEstadoService EstadoService { get; set; } = null!;
    [Inject] private IPaisService PaisService { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;
    [Inject] private IDialogService DialogService { get; set; } = null!;

    private PaginacaoDto<EstadoListDto>? _resultado;
    private FiltroConsultaDto _filtro = new();
    private EstadoDto _dto = new();
    private MudForm _form = null!;
    private bool _formValido;
    private bool _carregando;
    private bool _salvando;

    private List<PaisListDto> _paises = new();
    private bool _paisNaoSelecionado = false;

    protected override async Task OnInitializedAsync()
    {
        try { await CarregarPaises(); await CarregarDados(); }
        catch (Exception ex) { Snackbar.Add($"Erro ao carregar: {ex.Message}", Severity.Error); }
    }

    private async Task CarregarPaises()
        => _paises = (await PaisService.ObterTodosAtivosAsync()).ToList();
    private async Task OnPaisAlterado(PaisListDto? pais)
    {
        _dto.PaisId = pais?.Id ?? 0;
        _paisNaoSelecionado = false;
    }

    private async Task CarregarDados()
    {
        try { _carregando = true; _resultado = await EstadoService.ObterTodosAsync(_filtro); }
        catch (Exception ex) { Snackbar.Add($"Erro de banco: {ex.Message}", Severity.Error); _resultado = new(); }
        finally { _carregando = false; }
    }

    private async Task Pesquisar() { _filtro.Pagina = 1; await CarregarDados(); }
    private async Task LimparFiltros() { _filtro = new(); await CarregarDados(); }
    private async Task MudarPagina(int p) { _filtro.Pagina = p; await CarregarDados(); }

    private void LimparFormulario()
    {
        _dto = new(); _paisNaoSelecionado = false;
        _form?.ResetAsync();
    }

    private async Task Editar(int id)
    {
        var e = await EstadoService.ObterPorIdAsync(id);
        if (e == null) { Snackbar.Add("Estado não encontrado.", Severity.Warning); return; }
        _dto = e; _paisNaoSelecionado = false;
        StateHasChanged();
    }

    private async Task Salvar()
    {
        if (_dto.PaisId == 0) { _paisNaoSelecionado = true; Snackbar.Add("⚠️ Selecione um País.", Severity.Warning); return; }
        _paisNaoSelecionado = false;
        await _form.ValidateAsync();
        if (!_formValido) return;
        _salvando = true;
        var (sucesso, mensagem, _) = await EstadoService.SalvarAsync(_dto);
        _salvando = false;
        Snackbar.Add(mensagem, sucesso ? Severity.Success : Severity.Error);
        if (sucesso) { _dto = new(); await _form.ResetAsync(); await CarregarDados(); }
    }

    private async Task AbrirModalPais()
    {
        var opts = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Medium, FullWidth = true };
        var dialog = await DialogService.ShowAsync<ModalCadastroPais>("Novo País", opts);
        var result = await dialog.Result;
        if (result is { Canceled: false }) { await CarregarPaises(); if (result.Data is int id) _dto.PaisId = id; }
    }

    private async Task AlterarStatus(int id, string nome, bool ativoAtual)
    {
        var param = new DialogParameters<ConfirmDialog>
        {
            { x => x.Titulo,     $"Confirmar {(ativoAtual ? "desativar" : "ativar")}" },
            { x => x.Mensagem,   $"Deseja realmente {(ativoAtual ? "desativar" : "ativar")} o estado \"{nome}\"?" },
            { x => x.TextoBotao, ativoAtual ? "Desativar" : "Ativar" },
            { x => x.CorBotao,   ativoAtual ? Color.Error : Color.Success }
        };
        var dialog = await DialogService.ShowAsync<ConfirmDialog>("Confirmar",
            param, new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small });
        if ((await dialog.Result) is { Canceled: false })
        {
            var (sucesso, mensagem) = await EstadoService.AlterarStatusAsync(id, !ativoAtual);
            Snackbar.Add(mensagem, sucesso ? Severity.Success : Severity.Error);
            if (sucesso) await CarregarDados();
        }
    }
}