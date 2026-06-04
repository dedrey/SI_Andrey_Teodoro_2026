using Microsoft.AspNetCore.Components;
using MudBlazor;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Modals;
using SI_Andrey_Teodoro_2026.Services.Interfaces;

namespace SI_Andrey_Teodoro_2026.Pages;

public partial class ClientesPage : BasePage<ClienteListDto, ClienteDto>
{
    [Inject] private IClienteService ClienteService { get; set; } = null!;
    [Inject] private ICidadeService CidadeService { get; set; } = null!;
    [Inject] private IPaisService PaisService { get; set; } = null!;

    protected override string NomeEntidade => "Cliente";

    protected List<CidadeListDto> _cidades = new();
    protected List<PaisListDto> _paises = new();
    private bool _cidadeNaoSelecionada = false;
    private string _docTexto = "";
    private string _limiteCreditoTexto = "";

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var cidadesTask = CidadeService.ObterTodosAtivosSemPaginacaoAsync();
            var paisesTask = PaisService.ObterTodosAtivosAsync();
            await Task.WhenAll(cidadesTask, paisesTask);

            _cidades = (await cidadesTask)
                .Where(c => c.NomePais.Equals("Brasil", StringComparison.OrdinalIgnoreCase))
                .ToList();
            _paises = (await paisesTask).ToList();

            await CarregarDados();
        }
        catch (Exception ex) { Snackbar.Add($"Erro ao carregar: {ex.Message}", Severity.Error); }
    }

    protected override async Task CarregarDados()
    {
        try
        {
            _carregando = true;
            _resultado = await ClienteService.ObterTodosAsync(_filtro);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Erro de banco: {ex.Message}", Severity.Error);
            _resultado = new();
        }
        finally { _carregando = false; }
    }

    private void OnTipoPessoaAlterado(string tipo)
    {
        _dto.TipoPessoa = tipo;
        _docTexto = "";
        _dto.CpfCnpj = "";
    }

    private void OnDocAlterado(string? v)
    {
        if (string.IsNullOrEmpty(v)) { _docTexto = ""; _dto.CpfCnpj = ""; return; }
        var limpo = new string(v.Where(char.IsDigit).ToArray());
        _docTexto = _dto.TipoPessoa == "PF"
            ? FormatarCpf(limpo)
            : FormatarCnpj(limpo);
        _dto.CpfCnpj = limpo;
    }

    private void OnLimiteCreditoAlterado(string? v)
    {
        if (string.IsNullOrEmpty(v)) { _limiteCreditoTexto = ""; _dto.LimiteCredito = 0; return; }
        var filtrado = new string(v.Where(c => char.IsDigit(c) || c == ',' || c == '.').ToArray());
        _limiteCreditoTexto = filtrado;
        _dto.LimiteCredito = decimal.TryParse(
            filtrado.Replace(",", "."),
            System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture,
            out decimal d) ? d : 0m;
    }

    private void LimparFormulario()
    {
        _dto = new() { TipoPessoa = "PF" };
        _docTexto = "";
        _limiteCreditoTexto = "";
        _cidadeNaoSelecionada = false;
        _form?.ResetAsync();
    }

    private async Task Editar(int id)
    {
        var c = await ClienteService.ObterPorIdAsync(id);
        if (c == null) { Snackbar.Add("Cliente não encontrado.", Severity.Warning); return; }
        _dto = c;
        _docTexto = FormatDoc(c.CpfCnpj ?? "", c.TipoPessoa);
        _docTexto = c.CpfCnpj ?? "";
        _limiteCreditoTexto = c.LimiteCredito > 0 ? c.LimiteCredito.ToString("N2") : "";
        _cidadeNaoSelecionada = false;
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

    private async Task AbrirModalCidade()
    {
        var opts = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Medium, FullWidth = true };
        var dialog = await DialogService.ShowAsync<ModalCadastroCidade>("Nova Cidade", opts);
        var result = await dialog.Result;
        if (result is { Canceled: false })
        {
            _cidades = (await CidadeService.ObterTodosAtivosSemPaginacaoAsync())
                .Where(c => c.NomePais.Equals("Brasil", StringComparison.OrdinalIgnoreCase))
                .ToList();
            if (result.Data is int novoId)
            {
                _cidadeSelecionada = _cidades.FirstOrDefault(c => c.Id == novoId);
                _dto.CidadeId = novoId;
            }
        }
    }

    private Task AlterarStatus(int id, string nome, bool ativoAtual)
        => ConfirmarAlteracaoStatus(id, nome, ativoAtual,
               ClienteService.AlterarStatusAsync, CarregarDados);

    private static string FormatarCpf(string d)
    {
        if (d.Length >= 11) d = d[..11];
        return d.Length switch
        {
            11 => $"{d[..3]}.{d[3..6]}.{d[6..9]}-{d[9..]}",
            _ => d
        };
    }

    private static string FormatarCnpj(string d)
    {
        if (d.Length >= 14) d = d[..14];
        return d.Length switch
        {
            14 => $"{d[..2]}.{d[2..5]}.{d[5..8]}/{d[8..12]}-{d[12..]}",
            _ => d
        };
    }
    private static string FormatarDoc(string doc, string tipo)
    {
        var d = new string(doc.Where(char.IsDigit).ToArray());
        if (tipo == "PF" && d.Length == 11) return $"{d[..3]}.{d[3..6]}.{d[6..9]}-{d[9..]}";
        if (tipo == "PJ" && d.Length == 14) return $"{d[..2]}.{d[2..5]}.{d[5..8]}/{d[8..12]}-{d[12..]}";
        return doc;
    }
}