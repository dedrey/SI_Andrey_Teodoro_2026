using Microsoft.AspNetCore.Components;
using MudBlazor;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Modals;
using SI_Andrey_Teodoro_2026.Services.Interfaces;

namespace SI_Andrey_Teodoro_2026.Pages;

public partial class EmitentesPage : BasePage<EmitenteListDto, EmitenteDto>
{
    [Inject] private IEmitenteService EmitenteService { get; set; } = null!;
    [Inject] private ICidadeService CidadeService { get; set; } = null!;

    protected override string NomeEntidade => "Emitente";

    protected List<CidadeListDto> _cidades = new();
    private bool _cidadeNaoSelecionada = false;
    private bool _regimeNaoSelecionado = false;
    private string _cnpjTexto = "";

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

    protected override async Task CarregarDados()
    {
        try
        {
            _carregando = true;
            _resultado = await EmitenteService.ObterTodosAsync(_filtro);
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
        _dto = new();
        _cnpjTexto = "";
        _cidadeNaoSelecionada = false;
        _regimeNaoSelecionado = false;
        _form?.ResetAsync();
    }

    private async Task Editar(int id)
    {
        var e = await EmitenteService.ObterPorIdAsync(id);
        if (e == null) { Snackbar.Add("Emitente não encontrado.", Severity.Warning); return; }
        _dto = e;
        _cnpjTexto = e.Cnpj;
        _cidadeNaoSelecionada = false;
        _regimeNaoSelecionado = false;
        StateHasChanged();
    }

    private async Task Salvar()
    {
        if (string.IsNullOrWhiteSpace(_dto.RegimeTributario))
        {
            _regimeNaoSelecionado = true;
            Snackbar.Add("⚠️ Selecione o Regime Tributário antes de salvar.", Severity.Warning);
            return;
        }
        _regimeNaoSelecionado = false;

        await _form.ValidateAsync();
        if (!_formValido) return;
        _salvando = true;
        var (sucesso, mensagem, _) = await EmitenteService.SalvarAsync(_dto);
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
               EmitenteService.AlterarStatusAsync, CarregarDados);
}