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

    public async Task<(bool sucesso, string mensagem, int id)> SalvarAsync(ProdutoDto dto)
    {
        try
        {
            dto.Produto = dto.Produto.Trim();
            dto.Descricao = dto.Descricao?.Trim();

            if (dto.CategoriaId == 0) return (false, "Selecione uma categoria.", 0);
            if (dto.MarcaId == 0) return (false, "Selecione uma marca.", 0);
            if (dto.UnidadeMedidaId == 0) return (false, "Selecione uma unidade de medida.", 0);

            int? ignorarProduto = dto.IdOriginal > 0 ? dto.IdOriginal : null;
            if (await _repo.ExisteNomeAsync(dto.Produto, ignorarProduto))
                return (false, $"Já existe um produto com o nome '{dto.Produto}'.", 0);

            var variacoesValidas = dto.Variacoes.Where(v => !v.Removida).ToList();
            foreach (var v in variacoesValidas)
            {
                if (v.CorId == 0) return (false, $"Selecione a cor de uma variação.", 0);
                if (v.TamanhoId == 0) return (false, $"Selecione o tamanho de uma variação.", 0);
                if (v.Preco <= 0) return (false, $"Variação: preço de venda deve ser maior que zero.", 0);
                if (v.PrecoCusto < 0) return (false, $"Variação: preço de custo não pode ser negativo.", 0);
                if (v.PrecoCusto > 0 && v.Preco < v.PrecoCusto && !v.PermiteVendaAbaixoCusto)
                    return (false,
                        $"Preço de venda (R$ {v.Preco:N2}) não pode ser menor que o custo " +
                        $"(R$ {v.PrecoCusto:N2}). O produto tem menos de 90 dias sem venda.", 0);

                int? ignorarVar = v.IdOriginal > 0 ? v.IdOriginal : null;
                int produtoIdRef = dto.IdOriginal > 0 ? dto.IdOriginal : -1;

                if (await _repo.ExisteVariacaoAsync(produtoIdRef, v.CorId, v.TamanhoId, ignorarVar))
                    return (false, $"Já existe uma variação com essa cor e tamanho neste produto.", 0);

                if (!string.IsNullOrWhiteSpace(v.CodigoBarras))
                {
                    v.CodigoBarras = v.CodigoBarras.Trim();
                    if (await _repo.ExisteCodigoBarrasAsync(v.CodigoBarras, ignorarVar))
                        return (false, $"Código de barras '{v.CodigoBarras}' já está em uso.", 0);
                }
            }

            int produtoId;
            if (dto.IdOriginal == 0)
                produtoId = await _repo.InserirAsync(dto);
            else
            {
                produtoId = dto.Id;
                await _repo.AtualizarAsync(dto);
            }

            foreach (var v in variacoesValidas)
            {
                v.ProdutoId = produtoId;
                if (v.IdOriginal == 0)
                {
                    var novoVarId = await _repo.InserirVariacaoAsync(v);
                    await _repo.InserirEstoqueAsync(novoVarId);
                }
                else
                    await _repo.AtualizarVariacaoAsync(v);
            }

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

    public async Task<(bool sucesso, string mensagem)> AlterarStatusVariacaoAsync(int id, bool ativar)
    {
        try
        {
            await _repo.AlterarStatusVariacaoAsync(id, ativar);
            return (true, $"Variação {(ativar ? "ativada" : "desativada")} com sucesso!");
        }
        catch (Exception ex) { return (false, $"Erro: {ex.Message}"); }
    }

    public async Task<(bool sucesso, string mensagem)> AtualizarEstoqueAsync(int variacaoId, int quantidade)
    {
        try
        {
            if (quantidade < 0) return (false, "Quantidade não pode ser negativa.");
            await _repo.AtualizarEstoqueAsync(variacaoId, quantidade);
            return (true, "Estoque atualizado com sucesso!");
        }
        catch (Exception ex) { return (false, $"Erro: {ex.Message}"); }
    }
}