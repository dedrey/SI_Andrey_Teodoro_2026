using Microsoft.AspNetCore.Components;
using MudBlazor;
using SI_Andrey_Teodoro_2026.Components;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Modals;
using SI_Andrey_Teodoro_2026.Services.Interfaces;

namespace SI_Andrey_Teodoro_2026.Pages;

public partial class EstadosPage : BasePage<EstadoListDto, EstadoDto>
{
    [Inject] private IEstadoService EstadoService { get; set; } = null!;
    [Inject] private IPaisService PaisService { get; set; } = null!;

    protected override string NomeEntidade => "Estado";

    private List<PaisListDto> _paises = new();
    private bool _paisNaoSelecionado = false;

    protected override void InicializarDto() => _dto = new EstadoDto();

    protected override async Task OnInitializedAsync()
    {
        try { await CarregarDados(); }
        catch (Exception ex) { Snackbar.Add($"Erro ao carregar: {ex.Message}", Severity.Error); }
    }

    private async Task CarregarPaises()
        => _paises = (await PaisService.ObterTodosAtivosAsync()).ToList();

    protected override async Task CarregarDados()
    {
        try
        {
            _carregando = true;
            _resultado = await EstadoService.ObterTodosAsync(_filtro);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Erro de banco: {ex.Message}", Severity.Error);
            _resultado = new();
        }
        finally { _carregando = false; }
    }

    private async Task OnPaisAlterado(PaisListDto? pais)
    {
        _dto.PaisId = pais?.Id ?? 0;
        _paisNaoSelecionado = false;
        await Task.CompletedTask;
    }

    private void LimparFormulario()
    {
        _dto = new();
        _paisNaoSelecionado = false;
        _form?.ResetAsync();
    }

    private async Task Editar(int id)
    {
        var e = await EstadoService.ObterPorIdAsync(id);
        if (e == null) { Snackbar.Add("Estado não encontrado.", Severity.Warning); return; }
        _dto = e;
        _paisNaoSelecionado = false;
        StateHasChanged();
    }

    private async Task Salvar()
    {
        if (_dto.PaisId == 0)
        {
            _paisNaoSelecionado = true;
            Snackbar.Add("⚠️ Selecione um País.", Severity.Warning);
            return;
        }
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
        if (result is { Canceled: false })
        {
            await CarregarPaises();
            if (result.Data is int id) _dto.PaisId = id;
        }
    }

    private Task AlterarStatus(int id, string nome, bool ativoAtual)
        => ConfirmarAlteracaoStatus(id, nome, ativoAtual, EstadoService.AlterarStatusAsync, CarregarDados);
}