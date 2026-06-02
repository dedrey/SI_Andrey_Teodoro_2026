using Microsoft.AspNetCore.Components;
using MudBlazor;
using SI_Andrey_Teodoro_2026.Components;
using SI_Andrey_Teodoro_2026.Components.Shared;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Services.Interfaces;
using SI_Andrey_Teodoro_2026.Modals;

namespace SI_Andrey_Teodoro_2026.Pages;
public partial class CidadesPage : ComponentBase
{
    [Inject] private ICidadeService CidadeService { get; set; } = null!;
    [Inject] private IEstadoService EstadoService { get; set; } = null!;
    [Inject] private IPaisService PaisService { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;
    [Inject] private IDialogService DialogService { get; set; } = null!;

    private PaginacaoDto<CidadeListDto>? _resultado;
    private FiltroConsultaDto _filtro = new();
    private CidadeDto _dto = new();
    private MudForm _form = null!;
    private bool _formValido;
    private bool _carregando;
    private bool _salvando;

    private List<PaisListDto> _paises = new();
    private List<EstadoListDto> _estados = new();

    private bool _paisNaoSelecionado;
    private bool _estadoNaoSelecionado;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            await CarregarPaises();
            await CarregarDados();
        }
        catch (Exception ex) { Snackbar.Add($"Erro ao carregar: {ex.Message}", Severity.Error); }
    }

    private async Task CarregarPaises()
        => _paises = (await PaisService.ObterTodosAtivosAsync()).ToList();

    private async Task OnPaisAlterado(int paisId)
    {
        _dto.PaisId = paisId;
        _dto.EstadoId = 0;
        _paisNaoSelecionado = false;
        _estados = (await EstadoService.ObterPorPaisAsync(paisId)).ToList();
    }

    private async Task CarregarDados()
    {
        try
        {
            _carregando = true;
            _resultado = await CidadeService.ObterTodosAsync(_filtro);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Erro de banco: {ex.Message}", Severity.Error);
            _resultado = new();
        }
        finally { _carregando = false; }
    }

    private async Task Pesquisar() { _filtro.Pagina = 1; await CarregarDados(); }
    private async Task LimparFiltros() { _filtro = new(); await CarregarDados(); }
    private async Task MudarPagina(int p) { _filtro.Pagina = p; await CarregarDados(); }

    private void LimparFormulario()
    {
        _dto = new();
        _dddTexto = "";
        _estados = new();
        _paisNaoSelecionado = false;
        _estadoNaoSelecionado = false;
        _form?.ResetAsync();
    }

    private async Task Editar(int id)
    {
        var c = await CidadeService.ObterPorIdAsync(id);
        if (c == null) { Snackbar.Add("Cidade não encontrada.", Severity.Warning); return; }
        _dto = c;
        _dddTexto = c.Ddd > 0 ? c.Ddd.ToString() : "";
        _paisNaoSelecionado = false;
        _estadoNaoSelecionado = false;

        if (_dto.PaisId > 0)
            _estados = (await EstadoService.ObterPorPaisAsync(_dto.PaisId)).ToList();

        StateHasChanged();
    }

    private async Task Salvar()
    {
        if (_dto.PaisId == 0)
        {
            _paisNaoSelecionado = true;
            Snackbar.Add("⚠️ Selecione um País antes de salvar.", Severity.Warning);
            return;
        }
        _paisNaoSelecionado = false;

        if (_dto.EstadoId == 0)
        {
            _estadoNaoSelecionado = true;
            Snackbar.Add("⚠️ Selecione um Estado antes de salvar.", Severity.Warning);
            return;
        }
        _estadoNaoSelecionado = false;

        await _form.ValidateAsync();
        if (!_formValido) return;

        _salvando = true;
        var (sucesso, mensagem, _) = await CidadeService.SalvarAsync(_dto);
        _salvando = false;

        Snackbar.Add(mensagem, sucesso ? Severity.Success : Severity.Error);
        if (sucesso) { LimparFormulario(); await CarregarDados(); }
    }

    private async Task AbrirModalPais()
    {
        var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Medium, FullWidth = true };
        var dialog = await DialogService.ShowAsync<ModalCadastroPais>("Novo País", options);
        var result = await dialog.Result;
        if (result is { Canceled: false })
        {
            await CarregarPaises();
            if (result.Data is int novoId) await OnPaisAlterado(novoId);
        }
    }

    private async Task AbrirModalEstado()
    {
        var param = new DialogParameters<ModalCadastroEstado> { { x => x.PaisIdInicial, _dto.PaisId } };
        var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Medium, FullWidth = true };
        var dialog = await DialogService.ShowAsync<ModalCadastroEstado>("Novo Estado", param, options);
        var result = await dialog.Result;
        if (result is { Canceled: false, Data: not null })
        {
            _estados = (await EstadoService.ObterPorPaisAsync(_dto.PaisId)).ToList();
            if (result.Data is int novoEstadoId) _dto.EstadoId = novoEstadoId;
        }
    }

    private async Task AlterarStatus(int id, string nome, bool ativoAtual)
    {
        var param = new DialogParameters<ConfirmDialog>
        {
            { x => x.Titulo,     $"Confirmar {(ativoAtual ? "desativar" : "ativar")}" },
            { x => x.Mensagem,   $"Deseja realmente {(ativoAtual ? "desativar" : "ativar")} a cidade \"{nome}\"?" },
            { x => x.TextoBotao, ativoAtual ? "Desativar" : "Ativar" },
            { x => x.CorBotao,   ativoAtual ? Color.Error : Color.Success }
        };
        var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small };
        var dialog = await DialogService.ShowAsync<ConfirmDialog>("Confirmar", param, options);
        var result = await dialog.Result;
        if (result is { Canceled: false })
        {
            var (sucesso, mensagem) = await CidadeService.AlterarStatusAsync(id, !ativoAtual);
            Snackbar.Add(mensagem, sucesso ? Severity.Success : Severity.Error);
            if (sucesso) await CarregarDados();
        }
    }
}