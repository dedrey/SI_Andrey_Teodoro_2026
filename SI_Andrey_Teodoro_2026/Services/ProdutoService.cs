using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Repositories.Interfaces;
using SI_Andrey_Teodoro_2026.Services.Interfaces;

namespace SI_Andrey_Teodoro_2026.Services;

public class ProdutoService : BaseService<ProdutoDto, ProdutoListDto>, IProdutoService
{
    private readonly IProdutoRepository _repo;

    public ProdutoService(IProdutoRepository repo) => _repo = repo;

    protected override string NomeEntidade => "Produto";

    public Task<PaginacaoDto<ProdutoListDto>> ObterTodosAsync(FiltroConsultaDto filtro)
        => _repo.ObterTodosAsync(filtro);

    public Task<IEnumerable<ProdutoListDto>> ObterTodosAtivosAsync()
        => _repo.ObterTodosAtivosAsync();

    public Task<PaginacaoDto<ProdutoVariacaoListDto>> ObterVariacoesAsync(FiltroConsultaDto filtro)
        => _repo.ObterVariacoesAsync(filtro);

    public Task<List<int>> ObterVariacoesComMovimentoAsync(int produtoId) => _repo.ObterVariacoesComMovimentoAsync(produtoId);

    public async Task<ProdutoDto?> ObterPorIdAsync(int id)
    {
        var p = await _repo.ObterPorIdAsync(id);
        if (p == null) return null;
        var variacoes = await _repo.ObterVariacoesPorProdutoAsync(id);
        return new ProdutoDto
        {
            Id = p.Id,
            IdOriginal = p.Id,
            Produto = p.NomeProduto,
            Descricao = p.Descricao,
            CodigoBarras = p.CodigoBarras,
            CategoriaId = p.CategoriaId,
            NomeCategoria = p.NomeCategoria,
            MarcaId = p.MarcaId,
            NomeMarca = p.NomeMarca,
            UnidadeMedidaId = p.UnidadeMedidaId,
            SiglaUnidade = p.SiglaUnidade,
            FornecedorId = p.FornecedorId,
            NomeFornecedor = p.NomeFornecedor,
            NumeroNfUltimaEntrada = p.NumeroNfUltimaEntrada,
            Ativo = p.Ativo,
            AtualizadoEm = p.AtualizadoEm,
            NomeAtualizadoPor = p.NomeAtualizadoPor,
            Variacoes = variacoes
        };
    }

    public Task<int> ObterProximoIdAsync()
        => _repo.ObterProximoIdAsync();

    public async Task<(bool sucesso, string mensagem, int id)> SalvarAsync(ProdutoDto dto)
    {
        try
        {
            var erroId = await ValidarIdAsync(dto.Id, dto.IdOriginal, _repo.ExisteIdAsync);
            if (erroId != null) return (false, erroId, 0);

            dto.Produto = dto.Produto.Trim().ToUpperInvariant();
            dto.Descricao = dto.Descricao?.Trim().ToUpperInvariant();
            dto.CodigoBarras = string.IsNullOrWhiteSpace(dto.CodigoBarras) ? null : dto.CodigoBarras.Trim();

            if (dto.Produto.Length < 2) return (false, "Informe o nome do produto (mínimo 2 caracteres).", 0);
            if (dto.CategoriaId == 0) return (false, "Selecione uma categoria.", 0);
            if (dto.MarcaId == 0) return (false, "Selecione uma marca.", 0);
            if (dto.UnidadeMedidaId == 0) return (false, "Selecione uma unidade de medida.", 0);

            int? ignorarProduto = dto.IdOriginal > 0 ? dto.IdOriginal : null;
            if (await _repo.ExisteNomeAsync(dto.Produto, ignorarProduto))
                return (false, $"Já existe um produto com o nome '{dto.Produto}'.", 0);

            if (!string.IsNullOrWhiteSpace(dto.CodigoBarras) && await _repo.ExisteCodigoBarrasAsync(dto.CodigoBarras, ignorarProduto))
                return (false, $"Código de barras '{dto.CodigoBarras}' já está em uso em outro produto.", 0);

            if (dto.IdOriginal > 0)
            {
                var atuais = (await _repo.ObterVariacoesPorProdutoAsync(dto.IdOriginal)).ToDictionary(v => v.IdOriginal);
                foreach (var nova in dto.Variacoes.Where(v => v.IdOriginal > 0 && !v.Removida))
                {
                    if (atuais.TryGetValue(nova.IdOriginal, out var atual)
                        && (nova.CorId != atual.CorId || nova.TamanhoId != atual.TamanhoId))
                        return (false, "Cor e tamanho de variação já cadastrada não podem ser alterados.", 0);
                }

                var comMovimento = await _repo.ObterVariacoesComMovimentoAsync(dto.IdOriginal);
                foreach (var id in comMovimento)
                {
                    var nova = dto.Variacoes.FirstOrDefault(v => v.IdOriginal == id);
                    if (nova == null || !atuais.TryGetValue(id, out var atual)) continue;
                    if (nova.Removida)
                        return (false, $"{atual.Cor}/{atual.Tamanho}: variação com movimentação não pode ser removida.", 0);
                }
            }

            var variacoesValidas = dto.Variacoes.Where(v => !v.Removida).ToList();
            if (variacoesValidas.Count == 0)
                return (false, "Adicione ao menos uma variação (cor/tamanho).", 0);

            var combinacoesNoLote = new HashSet<(int cor, int tamanho)>();
            foreach (var v in variacoesValidas)
            {
                if (v.CorId == 0) return (false, "Selecione a cor de uma variação.", 0);
                if (v.TamanhoId == 0) return (false, "Selecione o tamanho de uma variação.", 0);
                if (v.Preco <= 0) return (false, "Variação: preço de venda deve ser maior que zero.", 0);

                if (v.PrecoCusto > 0 && v.Preco < v.PrecoCusto && !v.PermiteVendaAbaixoCusto)
                    return (false,
                        $"{v.Cor}/{v.Tamanho}: preço de venda (R$ {v.Preco:N2}) não pode ser menor que o custo " +
                        $"(R$ {v.PrecoCusto:N2}). A última compra desta variação foi há menos de 90 dias.", 0);

                if (!combinacoesNoLote.Add((v.CorId, v.TamanhoId)))
                    return (false, "Há duas variações com a mesma cor e tamanho nesta tela — remova a duplicada.", 0);

                int? ignorarVar = v.IdOriginal > 0 ? v.IdOriginal : null;
                int produtoIdRef = dto.IdOriginal > 0 ? dto.IdOriginal : -1;
                if (await _repo.ExisteVariacaoAsync(produtoIdRef, v.CorId, v.TamanhoId, ignorarVar))
                    return (false, "Já existe uma variação com essa cor e tamanho neste produto.", 0);
            }

            var produtoId = await _repo.SalvarComVariacoesAsync(dto, variacoesValidas);

            return (true, dto.IdOriginal == 0
                ? "Produto cadastrado com sucesso!"
                : "Produto atualizado com sucesso!", produtoId);
        }
        catch (Exception ex) { return (false, Erro(ex).mensagem, 0); }
    }

    public async Task<(bool sucesso, string mensagem)> AlterarStatusAsync(int id, bool ativar)
    {
        try { await _repo.AlterarStatusAsync(id, ativar); return SucessoStatus(ativar); }
        catch (Exception ex) { return ErroStatus(ex); }
    }
}