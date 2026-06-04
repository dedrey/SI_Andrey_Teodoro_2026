using Microsoft.AspNetCore.Components;
using MudBlazor;
using SI_Andrey_Teodoro_2026.Components.Shared;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Modals;
using SI_Andrey_Teodoro_2026.Services.Interfaces;
namespace SI_Andrey_Teodoro_2026.Pages;
public partial class ProdutosPage : BasePage<ProdutoListDto, ProdutoDto>
{
    [Inject] private IProdutoService ProdutoService { get; set; } = null!;
    [Inject] private ICategoriaService CategoriaService { get; set; } = null!;
    [Inject] private IMarcaService MarcaService { get; set; } = null!;
    [Inject] private IUnidadeMedidaService UnidadeMedidaService { get; set; } = null!;
    protected override string NomeEntidade => "Produto";
    protected List<CategoriaListDto> _categorias = new();
    protected List<MarcaListDto> _marcas = new();
    protected List<UnidadeMedidaListDto> _unidades = new();
    protected override async Task OnInitializedAsync()
    {
        try
        {
            var catTask = CategoriaService.ObterTodosAtivosAsync();
            var marTask = MarcaService.ObterTodosAtivosAsync();
            var uniTask = UnidadeMedidaService.ObterTodosAtivosAsync();
            await Task.WhenAll(catTask, marTask, uniTask);
            _categorias = (await catTask).ToList();
            _marcas = (await marTask).ToList();
            _unidades = (await uniTask).ToList();
            await CarregarDados();
        }
        catch (Exception ex) { Snackbar.Add($"Erro ao carregar: {ex.Message}", Severity.Error); }
    }
    protected override async Task CarregarDados()
    {
        try
        {
            _carregando = true;
            _resultado = await ProdutoService.ObterTodosAsync(_filtro);
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
        _precoTextos.Clear();
        _form?.ResetAsync();
    }
    private async Task Editar(int id)
    {
        var p = await ProdutoService.ObterPorIdAsync(id);
        if (p == null) { Snackbar.Add("Produto não encontrado.", Severity.Warning); return; }
        _dto = p;
        _precoTextos.Clear();
        foreach (var v in _dto.Variacoes)
            _precoTextos[v] = v.Preco > 0 ? v.Preco.ToString("N2") : "";
        StateHasChanged();
    }
    private async Task Salvar()
    {
        if (_dto.CategoriaId == 0) { Snackbar.Add("⚠️ Selecione uma Categoria.", Severity.Warning); return; }
        if (_dto.MarcaId == 0) { Snackbar.Add("⚠️ Selecione uma Marca.", Severity.Warning); return; }
        if (_dto.UnidadeMedidaId == 0) { Snackbar.Add("⚠️ Selecione uma Unidade de Medida.", Severity.Warning); return; }

        var variacoesAtivas = _dto.Variacoes.Where(v => !v.Removida).ToList();
        foreach (var v in variacoesAtivas)
        {
            if (string.IsNullOrWhiteSpace(v.Cor)) { Snackbar.Add("Preencha a cor de todas as variações.", Severity.Warning); return; }
            if (string.IsNullOrWhiteSpace(v.Tamanho)) { Snackbar.Add("Preencha o tamanho de todas as variações.", Severity.Warning); return; }
            if (v.Preco <= 0) { Snackbar.Add($"Variação {v.Cor}/{v.Tamanho}: preço inválido.", Severity.Warning); return; }
        }
        await _form.ValidateAsync();
        if (!_formValido) return;
        _salvando = true;
        var (sucesso, mensagem, _) = await ProdutoService.SalvarAsync(_dto);
        _salvando = false;
        Snackbar.Add(mensagem, sucesso ? Severity.Success : Severity.Error);
        if (sucesso) { LimparFormulario(); await CarregarDados(); }
    }
    private async Task AbrirModalCategoria()
    {
        var opts = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small, FullWidth = true };
        var dialog = await DialogService.ShowAsync<ModalCadastroCategoria>("Nova Categoria", opts);
        var result = await dialog.Result;
        if (result is { Canceled: false })
        {
            _categorias = (await CategoriaService.ObterTodosAtivosAsync()).ToList();
            if (result.Data is int novoId) _dto.CategoriaId = novoId;
        }
    }
    private async Task AbrirModalMarca()
    {
        var opts = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small, FullWidth = true };
        var dialog = await DialogService.ShowAsync<ModalCadastroMarca>("Nova Marca", opts);
        var result = await dialog.Result;
        if (result is { Canceled: false })
        {
            _marcas = (await MarcaService.ObterTodosAtivosAsync()).ToList();
            if (result.Data is int novoId) _dto.MarcaId = novoId;
        }
    }
    private async Task AbrirModalUnidade()
    {
        var opts = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small, FullWidth = true };
        var dialog = await DialogService.ShowAsync<ModalCadastroUnidadeMedida>("Nova Unidade de Medida", opts);
        var result = await dialog.Result;
        if (result is { Canceled: false })
        {
            _unidades = (await UnidadeMedidaService.ObterTodosAtivosAsync()).ToList();
            if (result.Data is int novoId) _dto.UnidadeMedidaId = novoId;
        }
    }
    private void AdicionarVariacao()
    {
        _dto.Variacoes.Add(new ProdutoVariacaoDto());
        StateHasChanged();
    }
    private void RemoverVariacao(ProdutoVariacaoDto v)
    {
        if (v.IdOriginal == 0) { _dto.Variacoes.Remove(v); _precoTextos.Remove(v); }
        else v.Removida = true;
        StateHasChanged();
    }
    private async Task AlterarEstoqueInline(ProdutoVariacaoDto v, int delta)
    {
        var novaQtd = v.QuantidadeEstoque + delta;
        if (novaQtd < 0) return;
        var (sucesso, mensagem) = await ProdutoService.AtualizarEstoqueAsync(v.IdOriginal, novaQtd);
        if (sucesso)
        {
            v.QuantidadeEstoque = novaQtd;
            if (_variacoesCache.ContainsKey(_dto.Id))
            {
                var varCache = _variacoesCache[_dto.Id];
                if (varCache != null)
                {
                    var item = varCache.FirstOrDefault(x => x.IdOriginal == v.IdOriginal);
                    if (item != null) item.QuantidadeEstoque = novaQtd;
                }
            }
            StateHasChanged();
        }
        else Snackbar.Add(mensagem, Severity.Error);
    }
    private async Task AlterarStatusVariacao(ProdutoVariacaoDto v)
    {
        var param = new DialogParameters<ConfirmDialog>
        {
            { x => x.Titulo,     $"Confirmar {(v.Ativo ? "desativar" : "ativar")}" },
            { x => x.Mensagem,   $"Deseja realmente {(v.Ativo ? "desativar" : "ativar")} a variação {v.Cor}/{v.Tamanho}?" },
            { x => x.TextoBotao, v.Ativo ? "Desativar" : "Ativar" },
            { x => x.CorBotao,   v.Ativo ? Color.Error : Color.Success }
        };
        var dialog = await DialogService.ShowAsync<ConfirmDialog>("Confirmar",
            param, new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small });
        if ((await dialog.Result) is { Canceled: false })
        {
            var (sucesso, mensagem) = await ProdutoService.AlterarStatusVariacaoAsync(v.IdOriginal, !v.Ativo);
            Snackbar.Add(mensagem, sucesso ? Severity.Success : Severity.Error);
            if (sucesso) { v.Ativo = !v.Ativo; StateHasChanged(); }
        }
    }
    private Task AlterarStatus(int id, string nome, bool ativoAtual)
        => ConfirmarAlteracaoStatus(id, nome, ativoAtual,
               ProdutoService.AlterarStatusAsync, CarregarDados);
}