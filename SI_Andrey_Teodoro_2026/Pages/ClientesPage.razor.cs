using Microsoft.AspNetCore.Components;
using MudBlazor;
using SI_Andrey_Teodoro_2026.Components.Shared;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Modals;
using SI_Andrey_Teodoro_2026.Services.Interfaces;

namespace SI_Andrey_Teodoro_2026.Pages;

public partial class ClientesPage : ComponentBase
{
    [Inject] private IClienteService ClienteService { get; set; } = null!;
    [Inject] private ICidadeService CidadeService { get; set; } = null!;
    [Inject] private IPaisService PaisService { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;
    [Inject] private IDialogService DialogService { get; set; } = null!;

    private PaginacaoDto<ClienteListDto>? _resultado;
    private FiltroConsultaDto _filtro = new();
    private ClienteDto _dto = new();
    private MudForm _form = null!;
    private bool _formValido;
    private bool _carregando;
    private bool _salvando;

    private List<CidadeListDto> _cidades = new();
    private List<PaisListDto> _paises = new();

    private string _docTexto = "";

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var cidadesTask = CidadeService.ObterTodosAtivosSemPaginacaoAsync();
            var paisesTask = PaisService.ObterTodosAtivosAsync();
            await Task.WhenAll(cidadesTask, paisesTask);
            _cidades = (await cidadesTask).ToList();
            _paises = (await paisesTask).ToList();
            await CarregarDados();
        }
        catch (Exception ex) { Snackbar.Add($"Erro ao carregar: {ex.Message}", Severity.Error); }
    }

    private async Task CarregarDados()
    {
        try { _carregando = true; _resultado = await ClienteService.ObterTodosAsync(_filtro); }
        catch (Exception ex) { Snackbar.Add($"Erro de banco: {ex.Message}", Severity.Error); _resultado = new(); }
        finally { _carregando = false; }
    }

    private async Task Pesquisar() { _filtro.Pagina = 1; await CarregarDados(); }
    private async Task LimparFiltros() { _filtro = new(); await CarregarDados(); }
    private async Task MudarPagina(int p) { _filtro.Pagina = p; await CarregarDados(); }

    private void OnTipoPessoaAlterado(string tipo)
    {
        _dto.TipoPessoa = tipo;
        _docTexto = "";
        _dto.CpfCnpj = null;
    }

    private void OnEstrangeiroAlterado(bool valor)
    {
        _dto.Estrangeiro = valor;
        _docTexto = "";
        _dto.CpfCnpj = null;
        _dto.DocumentoEstrangeiro = null;
        _dto.PaisOrigem = null;
    }

    private void OnDocAlterado(string v)
    {
        if (_dto.Estrangeiro)
        {
            _docTexto = v.ToUpper();
            _dto.DocumentoEstrangeiro = _docTexto;
        }
        else if (_dto.TipoPessoa == "PF")
        {
            _docTexto = AplicarMascaraCpf(v);
            _dto.CpfCnpj = _docTexto;
        }
        else
        {
            _docTexto = AplicarMascaraCnpj(v);
            _dto.CpfCnpj = _docTexto;
        }
    }

    private void LimparFormulario()
    {
        _dto = new();
        _docTexto = "";
        _limiteCreditoTexto = "";
        _form?.ResetAsync();
    }

    private async Task AbrirModalCidade()
    {
        var opts = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Medium, FullWidth = true };
        var dialog = await DialogService.ShowAsync<ModalCadastroCidade>("Nova Cidade", opts);
        var result = await dialog.Result;
        if (result is { Canceled: false })
        {
            _cidades = (await CidadeService.ObterTodosAtivosSemPaginacaoAsync()).ToList();
            if (result.Data is int novoId)
            {
                _cidadeSelecionada = _cidades.FirstOrDefault(c => c.Id == novoId);
                _dto.CidadeId = novoId;
            }
        }
    }

    private async Task Editar(int id)
    {
        var c = await ClienteService.ObterPorIdAsync(id);
        if (c == null) { Snackbar.Add("Cliente não encontrado.", Severity.Warning); return; }
        _dto = c;
        _docTexto = c.Estrangeiro ? c.DocumentoEstrangeiro ?? "" : c.CpfCnpj ?? "";
        _limiteCreditoTexto = c.LimiteCredito > 0 ? c.LimiteCredito.ToString("N2") : "";
        StateHasChanged();
    }

    private async Task Salvar()
    {
        await _form.ValidateAsync();
        if (!_formValido) return;
        _salvando = true;
        var (sucesso, mensagem, _) = await ClienteService.SalvarAsync(_dto);
        _salvando = false;
        Snackbar.Add(mensagem, sucesso ? Severity.Success : Severity.Error);
        if (sucesso) { LimparFormulario(); await CarregarDados(); }
    }

    private async Task AlterarStatus(int id, string nome, bool ativoAtual)
    {
        var param = new DialogParameters<ConfirmDialog>
        {
            { x => x.Titulo,     $"Confirmar {(ativoAtual ? "desativar" : "ativar")}" },
            { x => x.Mensagem,   $"Deseja realmente {(ativoAtual ? "desativar" : "ativar")} o cliente \"{nome}\"?" },
            { x => x.TextoBotao, ativoAtual ? "Desativar" : "Ativar" },
            { x => x.CorBotao,   ativoAtual ? Color.Error : Color.Success }
        };
        var dialog = await DialogService.ShowAsync<ConfirmDialog>("Confirmar",
            param, new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small });
        if ((await dialog.Result) is { Canceled: false })
        {
            var (sucesso, mensagem) = await ClienteService.AlterarStatusAsync(id, !ativoAtual);
            Snackbar.Add(mensagem, sucesso ? Severity.Success : Severity.Error);
            if (sucesso) await CarregarDados();
        }
    }
    private static string AplicarMascaraCpf(string? v)
    {
        if (string.IsNullOrEmpty(v)) return "";
        var d = new string(v.Where(char.IsDigit).ToArray());
        if (d.Length > 11) d = d[..11];
        return d.Length switch { 0 => "", <= 3 => d, <= 6 => $"{d[..3]}.{d[3..]}", <= 9 => $"{d[..3]}.{d[3..6]}.{d[6..]}", _ => $"{d[..3]}.{d[3..6]}.{d[6..9]}-{d[9..]}" };
    }
    private static string AplicarMascaraCnpj(string? v)
    {
        if (string.IsNullOrEmpty(v)) return "";
        var d = new string(v.Where(char.IsDigit).ToArray());
        if (d.Length > 14) d = d[..14];
        return d.Length switch { 0 => "", <= 2 => d, <= 5 => $"{d[..2]}.{d[2..]}", <= 8 => $"{d[..2]}.{d[2..5]}.{d[5..]}", <= 12 => $"{d[..2]}.{d[2..5]}.{d[5..8]}/{d[8..]}", _ => $"{d[..2]}.{d[2..5]}.{d[5..8]}/{d[8..12]}-{d[12..]}" };
    }
}