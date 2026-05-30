using Microsoft.AspNetCore.Components;
using MudBlazor;
using SI_Andrey_Teodoro_2026.Components.Shared;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Services.Interfaces;

namespace SI_Andrey_Teodoro_2026.Pages;

public partial class TransportadorasPage : ComponentBase
{
    [Inject] private ITransportadoraService TransportadoraService { get; set; } = null!;
    [Inject] private ICidadeService CidadeService { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;
    [Inject] private IDialogService DialogService { get; set; } = null!;

    private PaginacaoDto<TransportadoraListDto>? _resultado;
    private FiltroConsultaDto _filtro = new();
    private TransportadoraDto _dto = new();
    private MudForm _form = null!;
    private bool _formValido;
    private bool _carregando;
    private bool _salvando;

    private List<CidadeListDto> _cidades = new();

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

    private async Task CarregarDados()
    {
        try { _carregando = true; _resultado = await TransportadoraService.ObterTodosAsync(_filtro); }
        catch (Exception ex) { Snackbar.Add($"Erro de banco: {ex.Message}", Severity.Error); _resultado = new(); }
        finally { _carregando = false; }
    }

    private async Task Pesquisar() { _filtro.Pagina = 1; await CarregarDados(); }
    private async Task LimparFiltros() { _filtro = new(); await CarregarDados(); }
    private async Task MudarPagina(int p) { _filtro.Pagina = p; await CarregarDados(); }

    private void LimparFormulario() { _dto = new(); _form?.ResetAsync(); }

    private async Task Editar(int id)
    {
        var t = await TransportadoraService.ObterPorIdAsync(id);
        if (t == null) { Snackbar.Add("Transportadora não encontrada.", Severity.Warning); return; }
        _dto = t;
        StateHasChanged();
    }

    private async Task Salvar()
    {
        await _form.ValidateAsync();
        if (!_formValido) return;
        _salvando = true;
        var (sucesso, mensagem, _) = await TransportadoraService.SalvarAsync(_dto);
        _salvando = false;
        Snackbar.Add(mensagem, sucesso ? Severity.Success : Severity.Error);
        if (sucesso) { LimparFormulario(); await CarregarDados(); }
    }

    private async Task AlterarStatus(int id, string nome, bool ativoAtual)
    {
        var param = new DialogParameters<ConfirmDialog>
        {
            { x => x.Titulo,     $"Confirmar {(ativoAtual ? "desativar" : "ativar")}" },
            { x => x.Mensagem,   $"Deseja realmente {(ativoAtual ? "desativar" : "ativar")} a transportadora \"{nome}\"?" },
            { x => x.TextoBotao, ativoAtual ? "Desativar" : "Ativar" },
            { x => x.CorBotao,   ativoAtual ? Color.Error : Color.Success }
        };
        var dialog = await DialogService.ShowAsync<ConfirmDialog>("Confirmar",
            param, new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small });
        if ((await dialog.Result) is { Canceled: false })
        {
            var (sucesso, mensagem) = await TransportadoraService.AlterarStatusAsync(id, !ativoAtual);
            Snackbar.Add(mensagem, sucesso ? Severity.Success : Severity.Error);
            if (sucesso) await CarregarDados();
        }
    }
}