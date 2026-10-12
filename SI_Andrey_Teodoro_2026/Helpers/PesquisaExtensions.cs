using MudBlazor;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Services.Interfaces;

namespace SI_Andrey_Teodoro_2026.Helpers;

public static class PesquisaExtensions
{
    private const string Ativos = "ativos";

    public static Task<FornecedorListDto?> PesquisarFornecedorAsync(this IDialogService dialogService, IFornecedorService service, int? selecionado = null, Func<Task<FornecedorListDto?>>? novo = null) =>
        dialogService.PesquisarAsync("Pesquisar Fornecedor", service.ObterTodosAsync, new List<ColunaPesquisa<FornecedorListDto>>
        {
            new("Código", f => f.Id),
            new("Razão Social", f => f.RazaoSocial),
            new("Fantasia", f => f.NomeFantasia),
            new("CPF/CNPJ", f => f.CpfCnpj),
            new("Cidade", f => f.NomeCidade)
        }, Ativos, novo, x => x.Id, selecionado);

    public static Task<TransportadoraListDto?> PesquisarTransportadoraAsync(this IDialogService dialogService, ITransportadoraService service, int? selecionado = null, Func<Task<TransportadoraListDto?>>? novo = null) =>
        dialogService.PesquisarAsync("Pesquisar Transportadora", service.ObterTodosAsync, new List<ColunaPesquisa<TransportadoraListDto>>
        {
            new("Código", t => t.Id),
            new("Razão Social", t => t.RazaoSocial),
            new("Fantasia", t => t.NomeFantasia),
            new("CNPJ", t => t.Cnpj),
            new("Cidade", t => t.NomeCidade)
        }, Ativos, novo, x => x.Id, selecionado);

    public static Task<ClienteListDto?> PesquisarClienteAsync(this IDialogService dialogService, IClienteService service, int? selecionado = null, Func<Task<ClienteListDto?>>? novo = null) =>
        dialogService.PesquisarAsync("Pesquisar Cliente", service.ObterTodosAsync, new List<ColunaPesquisa<ClienteListDto>>
        {
            new("Código", c => c.Id),
            new("Cliente", c => c.NomeRazaoSocial),
            new("Apelido/Fantasia", c => c.ApelidoNomeFantasia),
            new("CPF/CNPJ", c => c.Estrangeiro ? c.DocumentoEstrangeiro : c.CpfCnpj),
            new("Cidade", c => c.NomeCidade)
        }, Ativos, novo, x => x.Id, selecionado);

    public static Task<EmitenteListDto?> PesquisarEmitenteAsync(this IDialogService dialogService, IEmitenteService service, int? selecionado = null, Func<Task<EmitenteListDto?>>? novo = null) =>
        dialogService.PesquisarAsync("Pesquisar Emitente", service.ObterTodosAsync, new List<ColunaPesquisa<EmitenteListDto>>
        {
            new("Código", e => e.Id),
            new("Razão Social", e => e.NomeRazaoSocial),
            new("Fantasia", e => e.ApelidoNomeFantasia),
            new("CNPJ", e => e.Cnpj),
            new("Cidade", e => e.NomeCidade)
        }, Ativos, novo, x => x.Id, selecionado);

    private static Func<FiltroConsultaDto, Task<PaginacaoDto<TItem>>> FiltroAdicional<TItem>(Func<FiltroConsultaDto, Task<PaginacaoDto<TItem>>> buscar, Action<FiltroConsultaDto> aplicar) =>
        filtro => { aplicar(filtro); return buscar(filtro); };

    private static Func<FiltroConsultaDto, Task<PaginacaoDto<TItem>>> FiltrarPorPais<TItem>(Func<FiltroConsultaDto, Task<PaginacaoDto<TItem>>> buscar, int? paisId) =>
        filtro => { filtro.PaisId = paisId is > 0 ? paisId : null; return buscar(filtro); };

    public static Task<CidadeListDto?> PesquisarCidadeAsync(this IDialogService dialogService, ICidadeService service, int? selecionado = null, Func<Task<CidadeListDto?>>? novo = null, int? paisId = null) =>
        dialogService.PesquisarAsync("Pesquisar Cidade",FiltrarPorPais<CidadeListDto>(service.ObterTodosAsync, paisId), new List<ColunaPesquisa<CidadeListDto>>
        {
            new("Código", c => c.Id),
            new("Cidade", c => c.NomeCidade),
            new("UF", c => c.Uf),
            new("País", c => c.NomePais)
        }, Ativos, novo, x => x.Id, selecionado);

    public static Task<EstadoListDto?> PesquisarEstadoAsync(this IDialogService dialogService, IEstadoService service, int? selecionado = null, Func<Task<EstadoListDto?>>? novo = null, int? paisId = null) =>
        dialogService.PesquisarAsync("Pesquisar Estado", FiltrarPorPais<EstadoListDto>(service.ObterTodosAsync, paisId), new List<ColunaPesquisa<EstadoListDto>>
        {
            new("Código", e => e.Id),
            new("Estado", e => e.NomeEstado),
            new("UF", e => e.Uf),
            new("País", e => e.NomePais)
        }, Ativos, novo, x => x.Id, selecionado);

    public static Task<PaisListDto?> PesquisarPaisAsync(this IDialogService dialogService, IPaisService service, int? selecionado = null, Func<Task<PaisListDto?>>? novo = null) =>
        dialogService.PesquisarAsync("Pesquisar País", service.ObterTodosAsync, new List<ColunaPesquisa<PaisListDto>>
        {
            new("Código", p => p.Id),
            new("País", p => p.NomePais),
            new("Sigla", p => p.Sigla),
            new("DDI", p => p.Ddi)
        }, Ativos, novo, x => x.Id, selecionado);

    public static Task<CondicaoPagamentoListDto?> PesquisarCondicaoPagamentoAsync(this IDialogService dialogService, ICondicaoPagamentoService service, int? selecionado = null, Func<Task<CondicaoPagamentoListDto?>>? novo = null) =>
        dialogService.PesquisarAsync("Pesquisar Condição de Pagamento", service.ObterTodosAsync, new List<ColunaPesquisa<CondicaoPagamentoListDto>>
        {
            new("Código", c => c.Id),
            new("Condição", c => c.CondicaoPagamento),
            new("Parcelas", c => c.NumeroParcelas, true),
            new("Método", c => c.NomeMetodoPagamento)
        }, Ativos, novo, x => x.Id, selecionado);

    public static Task<MetodoPagamentoListDto?> PesquisarMetodoPagamentoAsync(this IDialogService dialogService, IMetodoPagamentoService service, int? selecionado = null, Func<Task<MetodoPagamentoListDto?>>? novo = null) =>
        dialogService.PesquisarAsync("Pesquisar Método de Pagamento", service.ObterTodosAsync, new List<ColunaPesquisa<MetodoPagamentoListDto>>
        {
            new("Código", m => m.Id),
            new("Sigla", m => m.Codigo),
            new("Método de Pagamento", m => m.MetodoPagamento)
        }, Ativos, novo, x => x.Id, selecionado);

    public static Task<CategoriaListDto?> PesquisarCategoriaAsync(this IDialogService dialogService, ICategoriaService service, int? selecionado = null, Func<Task<CategoriaListDto?>>? novo = null) =>
        dialogService.PesquisarAsync("Pesquisar Categoria", service.ObterTodosAsync, new List<ColunaPesquisa<CategoriaListDto>>
        {
            new("Código", c => c.Id),
            new("Categoria", c => c.NomeCategoria)
        }, Ativos, novo, x => x.Id, selecionado);

    public static Task<MarcaListDto?> PesquisarMarcaAsync(this IDialogService dialogService, IMarcaService service, int? selecionado = null, Func<Task<MarcaListDto?>>? novo = null) =>
        dialogService.PesquisarAsync("Pesquisar Marca", service.ObterTodosAsync, new List<ColunaPesquisa<MarcaListDto>>
        {
            new("Código", m => m.Id),
            new("Marca", m => m.NomeMarca)
        }, Ativos, novo, x => x.Id, selecionado);

    public static Task<UnidadeMedidaListDto?> PesquisarUnidadeMedidaAsync(this IDialogService dialogService, IUnidadeMedidaService service, int? selecionado = null, Func<Task<UnidadeMedidaListDto?>>? novo = null) =>
        dialogService.PesquisarAsync("Pesquisar Unidade de Medida", service.ObterTodosAsync, new List<ColunaPesquisa<UnidadeMedidaListDto>>
        {
            new("Código", u => u.Id),
            new("Sigla", u => u.Sigla),
            new("Descrição", u => u.Descricao)
        }, Ativos, novo, x => x.Id, selecionado);

    private static List<ColunaPesquisa<CorListDto>> ColunasCor() => new()
    {
        new("Código", c => c.Id),
        new("Cor", c => c.Nome)
    };

    private static List<ColunaPesquisa<TamanhoListDto>> ColunasTamanho() => new()
    {
        new("Código", t => t.Id),
        new("Tamanho", t => t.Nome),
        new("Ordem", t => t.Ordem, true)
    };

    public static Task<CorListDto?> PesquisarCorAsync(this IDialogService dialogService, ICorService service, int? selecionado = null, Func<Task<CorListDto?>>? novo = null) =>
        dialogService.PesquisarAsync("Pesquisar Cor", service.ObterTodosAsync, ColunasCor(), Ativos, novo, x => x.Id, selecionado);

    public static Task<List<CorListDto>?> PesquisarVariasCoresAsync(this IDialogService dialogService, ICorService service, IEnumerable<int>? selecionados, IEnumerable<CorListDto> itensDoCampo, Func<Task<CorListDto?>>? novo = null) =>
        dialogService.PesquisarVariosAsync("Pesquisar Cores", service.ObterTodosAsync, ColunasCor(), x => x.Id, selecionados, itensDoCampo, Ativos, novo);

    public static Task<TamanhoListDto?> PesquisarTamanhoAsync(this IDialogService dialogService, ITamanhoService service, int? selecionado = null, Func<Task<TamanhoListDto?>>? novo = null) =>
        dialogService.PesquisarAsync("Pesquisar Tamanho", service.ObterTodosAsync, ColunasTamanho(), Ativos, novo, x => x.Id, selecionado);

    public static Task<List<TamanhoListDto>?> PesquisarVariosTamanhosAsync(this IDialogService dialogService, ITamanhoService service, IEnumerable<int>? selecionados, IEnumerable<TamanhoListDto> itensDoCampo, Func<Task<TamanhoListDto?>>? novo = null) =>
        dialogService.PesquisarVariosAsync("Pesquisar Tamanhos", service.ObterTodosAsync, ColunasTamanho(), x => x.Id, selecionados, itensDoCampo, Ativos, novo);

    public static Task<ProdutoListDto?> PesquisarProdutoAsync(this IDialogService dialogService, IProdutoService service, int? selecionado = null, Func<Task<ProdutoListDto?>>? novo = null, bool somenteComEstoque = false) =>
        dialogService.PesquisarAsync("Pesquisar Produto", FiltroAdicional<ProdutoListDto>(service.ObterTodosAsync, f => f.SomenteComEstoque = somenteComEstoque), new List<ColunaPesquisa<ProdutoListDto>>
        {
            new("Código", p => p.Id),
            new("Produto", p => p.Produto),
            new("Categoria", p => p.NomeCategoria),
            new("Marca", p => p.NomeMarca),
            new("Fornecedor", p => p.NomeFornecedor),
            new("Estoque", p => $"{p.TotalEstoque} un.", true)
        }, Ativos, novo, x => x.Id, selecionado);

    public static Task<ProdutoVariacaoListDto?> PesquisarVariacaoAsync(this IDialogService dialogService, IProdutoService service, int produtoId, bool somenteComEstoque, int? selecionado = null) =>
        dialogService.PesquisarAsync("Pesquisar Variação", FiltroAdicional<ProdutoVariacaoListDto>(service.ObterVariacoesAsync, f => { f.ProdutoId = produtoId; f.SomenteComEstoque = somenteComEstoque; }), new List<ColunaPesquisa<ProdutoVariacaoListDto>>
        {
            new("Código", v => v.Id),
            new("Cor", v => v.Cor),
            new("Tamanho", v => v.Tamanho),
            new("Estoque", v => $"{v.QuantidadeEstoque} un.", true),
            new("Preço de Venda (R$)", v => v.Preco, true, "N2")
        }, null, null, x => x.Id, selecionado);

    public static Task<VendaParaNfeDto?> PesquisarVendaParaNfeAsync(this IDialogService dialogService, INfeService service, int? selecionado = null, Func<Task<VendaParaNfeDto?>>? novo = null) =>
        dialogService.PesquisarAsync("Pesquisar Venda", service.ObterVendasDisponiveisAsync, new List<ColunaPesquisa<VendaParaNfeDto>>
        {
            new("Código", v => v.Id),
            new("Cliente", v => v.NomeCliente),
            new("Condição", v => v.NomeCondicao),
            new("Data", v => v.CriadoEm, false, "dd/MM/yyyy"),
            new("Total (R$)", v => v.ValorTotal, true, "N2")
        }, null, novo, x => x.Id, selecionado);
}
