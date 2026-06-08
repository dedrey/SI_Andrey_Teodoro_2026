using Microsoft.AspNetCore.Components;
using MudBlazor;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Services.Interfaces;

namespace SI_Andrey_Teodoro_2026.Pages;

public partial class MovimentacoesEstoquePage : BasePage<MovimentacaoEstoqueListDto, MovimentacaoEstoqueDto>
{
    [Inject] private IMovimentacaoEstoqueService MovimentacaoService { get; set; } = null!;
    [Inject] private IProdutoService ProdutoService { get; set; } = null!;

    protected override string NomeEntidade => "Movimentação de estoque";

    // declarado aqui para ser acessível tanto no .razor quanto no LimparFormulario
    private readonly Dictionary<MovimentacaoEstoqueItemDto, string> _qtdRealTextos = new();

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var produtos = await ProdutoService.ObterTodosAtivosAsync();
            _todosProdutos = produtos.ToList();
            await CarregarDados();
        }
        catch (Exception ex) { Snackbar.Add($"Erro ao carregar: {ex.Message}", Severity.Error); }
    }

    protected override async Task CarregarDados()
    {
        try
        {
            _carregando = true;
            _resultado = await MovimentacaoService.ObterTodosAsync(_filtro);
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
        _dto = new MovimentacaoEstoqueDto();
        _valorTextos.Clear();
        _produtosSelecionados.Clear();
        _variacoesPorItem.Clear();
        _qtdRealTextos.Clear();
        _form?.ResetAsync();
    }

    private async Task Salvar()
    {
        _salvando = true;
        var (sucesso, mensagem, _) = await MovimentacaoService.SalvarAsync(_dto);
        _salvando = false;
        Snackbar.Add(mensagem, sucesso ? Severity.Success : Severity.Error);
        if (sucesso)
        {
            _detalhesCache.Clear();
            _expandidos.Clear();
            LimparFormulario();
            await CarregarDados();
        }
    }
}