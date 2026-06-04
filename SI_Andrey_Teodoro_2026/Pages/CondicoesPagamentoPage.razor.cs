using Microsoft.AspNetCore.Components;
using MudBlazor;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Modals;
using SI_Andrey_Teodoro_2026.Services.Interfaces;
namespace SI_Andrey_Teodoro_2026.Pages;
public partial class CondicoesPagamentoPage : BasePage<CondicaoPagamentoListDto, CondicaoPagamentoDto>
{
    [Inject] private ICondicaoPagamentoService CondicaoPagamentoService { get; set; } = null!;
    [Inject] private IMetodoPagamentoService MetodoPagamentoService { get; set; } = null!;
    protected override string NomeEntidade => "Condição de pagamento";
    private bool _metodonNaoSelecionado;
    private List<MetodoPagamentoListDto> _metodos = new();
    private string _parcelasTexto = "1";
    private string _entradaTexto = "";
    private string _descontoTexto = "";
    private string _acrescimoTexto = "";
    private string _multaTexto = "";
    private string _jurosTexto = "";
    protected override async Task OnInitializedAsync()
    {
        try
        {
            _metodos = (await MetodoPagamentoService.ObterTodosAtivosAsync()).ToList();
            await CarregarDados();
        }
        catch (Exception ex) { Snackbar.Add($"Erro ao carregar: {ex.Message}", Severity.Error); }
    }
    protected override async Task CarregarDados()
    {
        try
        {
            _carregando = true;
            _resultado = await CondicaoPagamentoService.ObterTodosAsync(_filtro);
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
        _dto = new() { NumeroParcelas = 1 };
        _parcelasTexto = "1";
        _entradaTexto = "";
        _descontoTexto = "";
        _acrescimoTexto = "";
        _multaTexto = "";
        _jurosTexto = "";
        _metodonNaoSelecionado = false;
        _form?.ResetAsync();
    }
    private async Task Editar(int id)
    {
        var c = await CondicaoPagamentoService.ObterPorIdAsync(id);
        if (c == null) { Snackbar.Add("Condição de pagamento não encontrada.", Severity.Warning); return; }
        _dto = c;
        _parcelasTexto = c.NumeroParcelas.ToString();
        _entradaTexto = c.EntradaMinimaPercentual > 0 ? c.EntradaMinimaPercentual.ToString("N2") : "";
        _descontoTexto = c.DescontoPercentual > 0 ? c.DescontoPercentual.ToString("N2") : "";
        _acrescimoTexto = c.AcrescimoPercentual > 0 ? c.AcrescimoPercentual.ToString("N2") : "";
        _multaTexto = c.MultaPercentual > 0 ? c.MultaPercentual.ToString("N2") : "";
        _jurosTexto = c.TaxaJurosPercentual > 0 ? c.TaxaJurosPercentual.ToString("N2") : "";
        _metodonNaoSelecionado = false;
        StateHasChanged();
    }
    private async Task Salvar()
    {
        if (_dto.MetodoPagamentoId == 0)
        {
            _metodonNaoSelecionado = true;
            Snackbar.Add("⚠️ Selecione um Método de Pagamento antes de salvar.", Severity.Warning);
            return;
        }
        _metodonNaoSelecionado = false;
        await _form.ValidateAsync();
        if (!_formValido) return;
        _salvando = true;
        var (sucesso, mensagem, _) = await CondicaoPagamentoService.SalvarAsync(_dto);
        _salvando = false;
        Snackbar.Add(mensagem, sucesso ? Severity.Success : Severity.Error);
        if (sucesso) { LimparFormulario(); await CarregarDados(); }
    }
    private async Task AbrirModalMetodoPagamento()
    {
        var opts = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small, FullWidth = true };
        var dialog = await DialogService.ShowAsync<ModalCadastroMetodoPagamento>("Novo Método de Pagamento", opts);
        var result = await dialog.Result;
        if (result is { Canceled: false })
        {
            _metodos = (await MetodoPagamentoService.ObterTodosAtivosAsync()).ToList();
            if (result.Data is int novoId) _dto.MetodoPagamentoId = novoId;
        }
    }
    private Task AlterarStatus(int id, string nome, bool ativoAtual)
        => ConfirmarAlteracaoStatus(id, nome, ativoAtual,
               CondicaoPagamentoService.AlterarStatusAsync, CarregarDados);
}