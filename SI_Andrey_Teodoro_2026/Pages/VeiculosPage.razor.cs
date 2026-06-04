using Microsoft.AspNetCore.Components;
using MudBlazor;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Modals;
using SI_Andrey_Teodoro_2026.Services.Interfaces;

namespace SI_Andrey_Teodoro_2026.Pages;

public partial class VeiculosPage : BasePage<VeiculoListDto, VeiculoDto>
{
    [Inject] private IVeiculoService VeiculoService { get; set; } = null!;
    [Inject] private ITransportadoraService TransportadoraService { get; set; } = null!;
    [Inject] private IEstadoService EstadoService { get; set; } = null!;

    protected override string NomeEntidade => "Veículo";

    private List<TransportadoraListDto> _transportadoras = new();
    private List<string> _ufs = new();
    private string _placaTexto = "";

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var transportadorasTask = TransportadoraService.ObterTodosAtivosAsync();
            var estadosTask = EstadoService.ObterTodosAtivosAsync();
            await Task.WhenAll(transportadorasTask, estadosTask);

            _transportadoras = (await transportadorasTask).ToList();
            _ufs = (await estadosTask)
                .Select(e => e.Uf)
                .Distinct()
                .OrderBy(u => u)
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
            _resultado = await VeiculoService.ObterTodosAsync(_filtro);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Erro de banco: {ex.Message}", Severity.Error);
            _resultado = new();
        }
        finally { _carregando = false; }
    }

    private void OnPlacaAlterada(string? v)
    {
        if (string.IsNullOrEmpty(v)) { _placaTexto = ""; _dto.Placa = ""; return; }
        var limpa = new string(v.Where(char.IsLetterOrDigit).ToArray()).ToUpper();
        if (limpa.Length > 7) limpa = limpa[..7];
        _placaTexto = limpa;
        _dto.Placa = limpa;
    }

    private void LimparFormulario()
    {
        _dto = new();
        _placaTexto = "";
        _form?.ResetAsync();
    }

    private async Task Editar(int id)
    {
        var v = await VeiculoService.ObterPorIdAsync(id);
        if (v == null) { Snackbar.Add("Veículo não encontrado.", Severity.Warning); return; }
        _dto = v;
        _placaTexto = v.Placa;
        StateHasChanged();
    }

    private async Task Salvar()
    {
        await _form.ValidateAsync();
        if (!_formValido) return;
        _salvando = true;
        var (sucesso, mensagem, _) = await VeiculoService.SalvarAsync(_dto);
        _salvando = false;
        Snackbar.Add(mensagem, sucesso ? Severity.Success : Severity.Error);
        if (sucesso) { LimparFormulario(); await CarregarDados(); }
    }

    private async Task AbrirModalTransportadora()
    {
        var opts = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Medium, FullWidth = true };
        var dialog = await DialogService.ShowAsync<ModalCadastroTransportadora>("Nova Transportadora", opts);
        var result = await dialog.Result;
        if (result is { Canceled: false })
        {
            _transportadoras = (await TransportadoraService.ObterTodosAtivosAsync()).ToList();
            if (result.Data is int novoId)
            {
                _transportadoraSelecionada = _transportadoras.FirstOrDefault(t => t.Id == novoId);
                _dto.TransportadoraId = novoId;
            }
        }
    }

    private Task AlterarStatus(int id, string placa, bool ativoAtual)
        => ConfirmarAlteracaoStatus(id, placa, ativoAtual,
               VeiculoService.AlterarStatusAsync, CarregarDados);
}