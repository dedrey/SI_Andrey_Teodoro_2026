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

    protected override string NomeEntidade => "Cliente";

    protected List<CidadeListDto> _cidades = new();
    private string _docTexto = "";
    private string _limiteCreditoTexto = "";

    protected override async Task OnInitializedAsync()
    {
        try
        {
            _cidades = (await CidadeService.ObterTodosAtivosSemPaginacaoAsync()).ToList();
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
        finally { _carregando = false; StateHasChanged(); }
    }

    private void OnTipoPessoaAlterado(string tipo)
    {
        _dto.TipoPessoa = tipo;
        _docTexto = "";
        _dto.CpfCnpj = "";
    }

    private void OnEstrangeiroAlterado(bool valor)
    {
        _dto.Estrangeiro = valor;
        _dto.CidadeId = null;
        _dto.CpfCnpj = "";
        _docTexto = "";
    }

    private void OnDocAlterado(string? v)
    {
        if (string.IsNullOrEmpty(v)) { _docTexto = ""; _dto.CpfCnpj = ""; return; }
        var d = new string(v.Where(char.IsDigit).ToArray());
        if (_dto.TipoPessoa == "PF")
        {
            if (d.Length > 11) d = d[..11];
            _docTexto = d.Length == 11 ? $"{d[..3]}.{d[3..6]}.{d[6..9]}-{d[9..]}" : d;
        }
        else
        {
            if (d.Length > 14) d = d[..14];
            _docTexto = d.Length == 14 ? $"{d[..2]}.{d[2..5]}.{d[5..8]}/{d[8..12]}-{d[12..]}" : d;
        }
        _dto.CpfCnpj = d;
    }

    private void OnLimiteCreditoAlterado(string? v)
    {
        if (string.IsNullOrEmpty(v)) { _limiteCreditoTexto = ""; _dto.LimiteCredito = 0; return; }
        var f = new string(v.Where(c => char.IsDigit(c) || c == ',' || c == '.').ToArray());
        _limiteCreditoTexto = f;
        _dto.LimiteCredito = decimal.TryParse(f.Replace(",", "."),
            System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out decimal d) ? d : 0m;
    }

    private void LimparFormulario()
    {
        _dto = new() { TipoPessoa = "PF" };
        _docTexto = "";
        _limiteCreditoTexto = "";
        _form?.ResetAsync();
    }

    private async Task Editar(int id)
    {
        var c = await ClienteService.ObterPorIdAsync(id);
        if (c == null) { Snackbar.Add("Cliente não encontrado.", Severity.Warning); return; }
        _dto = c;
        _docTexto = c.CpfCnpj ?? "";
        _limiteCreditoTexto = c.LimiteCredito > 0 ? c.LimiteCredito.ToString("N2") : "";
        StateHasChanged();
    }

    private async Task Salvar()
    {
        await _form.ValidateAsync();
        if (!_formValido) return;

        if (!_dto.Estrangeiro && (string.IsNullOrWhiteSpace(_dto.CpfCnpj) ||
            (_dto.TipoPessoa == "PF" && _dto.CpfCnpj.Length != 11) ||
            (_dto.TipoPessoa == "PJ" && _dto.CpfCnpj.Length != 14)))
        {
            Snackbar.Add(_dto.TipoPessoa == "PF"
                ? "CPF inválido. Informe os 11 dígitos."
                : "CNPJ inválido. Informe os 14 dígitos.", Severity.Warning);
            return;
        }

        if (_dto.CidadeId == null)
        {
            Snackbar.Add("Selecione uma cidade.", Severity.Warning);
            return;
        }

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
            _cidades = (await CidadeService.ObterTodosAtivosSemPaginacaoAsync()).ToList();
            if (result.Data is int novoId)
            {
                _dto.CidadeId = novoId;
            }
        }
    }

    private Task AlterarStatus(int id, string nome, bool ativoAtual)
        => ConfirmarAlteracaoStatus(id, nome, ativoAtual,
               ClienteService.AlterarStatusAsync, CarregarDados);
}