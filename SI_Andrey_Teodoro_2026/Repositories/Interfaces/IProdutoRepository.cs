using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Models;

namespace SI_Andrey_Teodoro_2026.Repositories.Interfaces;

public interface IProdutoRepository
{
    Task<PaginacaoDto<ProdutoListDto>> ObterTodosAsync(FiltroConsultaDto filtro);
    Task<IEnumerable<ProdutoListDto>> ObterTodosAtivosAsync();
    Task<Produto?> ObterPorIdAsync(int id);
    Task<List<ProdutoVariacaoDto>> ObterVariacoesPorProdutoAsync(int produtoId);
    Task<int> InserirAsync(ProdutoDto dto);
    Task AtualizarAsync(ProdutoDto dto);
    Task AlterarStatusAsync(int id, bool ativo);
    Task<bool> ExisteNomeAsync(string nome, int? idOriginalIgnorar = null);

    Task<int> InserirVariacaoAsync(ProdutoVariacaoDto dto);
    Task AtualizarVariacaoAsync(ProdutoVariacaoDto dto);
    Task AlterarStatusVariacaoAsync(int id, bool ativo);
    Task<bool> ExisteVariacaoAsync(int produtoId, string cor, string tamanho, int? idIgnorar = null);
    Task<bool> ExisteCodigoBarrasAsync(string codigoBarras, int? idIgnorar = null);

    Task InserirEstoqueAsync(int variacaoId);
    Task AtualizarEstoqueAsync(int variacaoId, int quantidade);
}