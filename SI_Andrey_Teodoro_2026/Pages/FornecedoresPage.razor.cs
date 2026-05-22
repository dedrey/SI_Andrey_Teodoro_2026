using Microsoft.AspNetCore.Components;
using MudBlazor;
using SI_Andrey_Teodoro_2026.Components.Shared;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Services.Interfaces;

namespace SI_Andrey_Teodoro_2026.Pages;

public partial class FornecedoresPage : ComponentBase
{
    [Inject] private IFornecedorService FornecedorService { get; set; } = null!;
    [Inject] private IPaisService PaisService { get; set; } = null!;
    [Inject] private IEstadoService EstadoService { get; set; } = null!;
    [Inject] private ICidadeService CidadeService { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;
    [Inject] private IDialogService DialogService { get; set; } = null!;

    private PaginacaoDto<FornecedorListDto>? _resultado;
    private FiltroConsultaDto _filtro = new();
    private FornecedorDto _dto = new();
    private MudForm _form = null!;
    private bool _formValido;
    private bool _carregando;
    private bool _salvando;
    private List<PaisListDto> _paises = new();
    private List<EstadoListDto> _estados = new();
    private List<CidadeListDto> _cidadesFiltradas = new();

    protected override async Task OnInitializedAsync()
    {
        try
        {
            await CarregarPaises();
            await CarregarDados();
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Erro ao carregar: {ex.Message}", Severity.Error);
        }
    }

    private async Task CarregarPaises()
        => _paises = (await PaisService.ObterTodosAtivosAsync()).ToList();
    private async Task OnPaisAlterado(int? paisId)
    {
        _dto.PaisId = paisId;
        _dto.EstadoId = null;
        _dto.CidadeId = null;
        _estados = new();
        _cidadesFiltradas = new();

        if (paisId.HasValue && paisId > 0)
            _estados = (await EstadoService.ObterPorPaisAsync(paisId.Value)).ToList();
    }
    private async Task OnEstadoAlterado(int? estadoId)
    {
        _dto.EstadoId = estadoId;
        _dto.CidadeId = null;
        _cidadesFiltradas = new();

        if (estadoId.HasValue && estadoId > 0)
            _cidadesFiltradas = (await CidadeService.ObterPorEstadoAsync(estadoId.Value)).ToList();
    }

    private async Task CarregarDados()
    {
        try
        {
            _carregando = true;
            _resultado = await FornecedorService.ObterTodosAsync(_filtro);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Erro de banco: {ex.Message}", Severity.Error);
            _resultado = new PaginacaoDto<FornecedorListDto>();
        }
        finally { _carregando = false; }
    }

    private async Task Pesquisar() { _filtro.Pagina = 1; await CarregarDados(); }

    private async Task LimparFiltros() { _filtro = new FiltroConsultaDto(); await CarregarDados(); }

    private async Task MudarPagina(int p) { _filtro.Pagina = p; await CarregarDados(); }

    private void LimparFormulario()
    {
        _dto = new FornecedorDto();
        _estados = new();
        _cidadesFiltradas = new();
        _form?.ResetAsync();
    }

    private async Task Editar(int id)
    {
        var f = await FornecedorService.ObterPorIdAsync(id);
        if (f == null) { Snackbar.Add("Fornecedor não encontrado.", Severity.Warning); return; }

        _dto = f;
        _estados = new();
        _cidadesFiltradas = new();

        if (_dto.PaisId.HasValue && _dto.PaisId > 0)
            _estados = (await EstadoService.ObterPorPaisAsync(_dto.PaisId.Value)).ToList();

        if (_dto.EstadoId.HasValue && _dto.EstadoId > 0)
            _cidadesFiltradas = (await CidadeService.ObterPorEstadoAsync(_dto.EstadoId.Value)).ToList();

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

        if (sucesso)
        {
            LimparFormulario();
            await CarregarDados();
        }
    }

    private async Task AlterarStatus(int id, string nome, bool ativoAtual)
    {
        var acao = ativoAtual ? "desativar" : "ativar";
        var param = new DialogParameters<ConfirmDialog>
        {
            { x => x.Titulo,     $"Confirmar {acao}" },
            { x => x.Mensagem,   $"Deseja realmente {acao} o fornecedor \"{nome}\"?" },
            { x => x.TextoBotao, ativoAtual ? "Desativar" : "Ativar" },
            { x => x.CorBotao,   ativoAtual ? Color.Error : Color.Success }
        };
        var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small };
        var dialog = await DialogService.ShowAsync<ConfirmDialog>("Confirmar", param, options);
        var result = await dialog.Result;

        if (result is { Canceled: false })
        {
            var (sucesso, mensagem) = await FornecedorService.AlterarStatusAsync(id, !ativoAtual);
            Snackbar.Add(mensagem, sucesso ? Severity.Success : Severity.Error);
            if (sucesso) await CarregarDados();
        }
    }
}