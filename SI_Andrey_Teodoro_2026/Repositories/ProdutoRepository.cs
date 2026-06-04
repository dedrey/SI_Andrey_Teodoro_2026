using Dapper;
using SI_Andrey_Teodoro_2026.Data;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Models;
using SI_Andrey_Teodoro_2026.Repositories.Interfaces;

namespace SI_Andrey_Teodoro_2026.Repositories;

public class ProdutoRepository : BaseRepository, IProdutoRepository
{
    public ProdutoRepository(DbConnectionFactory factory) : base(factory) { }

    protected override string Tabela => "produtos";

    public async Task<PaginacaoDto<ProdutoListDto>> ObterTodosAsync(FiltroConsultaDto filtro)
    {
        using var conn = _factory.CreateConnection();
        var where = new List<string>();
        if (!string.IsNullOrWhiteSpace(filtro.Busca))
            where.Add(@"(p.produto  LIKE @Busca
                      OR c.categoria LIKE @Busca
                      OR m.marca     LIKE @Busca
                      OR CAST(p.id AS CHAR) = @BuscaExata)");
        where.Add(filtro.StatusFiltro switch
        {
            "ativos" => "p.ativo = TRUE",
            "inativos" => "p.ativo = FALSE",
            _ => "1=1"
        });
        var whereClause = "WHERE " + string.Join(" AND ", where);
        var orderBy = filtro.OrdenarPor switch
        {
            "id" => "p.id",
            "data" => "p.criado_em",
            _ => "p.produto"
        };

        var sqlCount = $@"SELECT COUNT(*)
                          FROM produtos p
                          INNER JOIN categorias      c ON c.id = p.categoria_id
                          INNER JOIN marcas          m ON m.id = p.marca_id
                          INNER JOIN unidades_medida u ON u.id = p.unidade_medida_id
                          {whereClause}";

        var sqlData = $@"SELECT p.id,
                                 p.produto            AS Produto,
                                 p.descricao          AS Descricao,
                                 p.categoria_id       AS CategoriaId,
                                 c.categoria          AS NomeCategoria,
                                 p.marca_id           AS MarcaId,
                                 m.marca              AS NomeMarca,
                                 p.unidade_medida_id  AS UnidadeMedidaId,
                                 u.unidade_medida     AS SiglaUnidade,
                                 COUNT(DISTINCT pv.id)          AS TotalVariacoes,
                                 COALESCE(SUM(e.quantidade), 0) AS TotalEstoque,
                                 p.ativo,
                                 p.criado_em AS CriadoEm
                          FROM produtos p
                          INNER JOIN categorias      c  ON c.id  = p.categoria_id
                          INNER JOIN marcas          m  ON m.id  = p.marca_id
                          INNER JOIN unidades_medida u  ON u.id  = p.unidade_medida_id
                          LEFT  JOIN produto_variacoes pv ON pv.produto_id = p.id AND pv.ativo = TRUE
                          LEFT  JOIN estoque          e  ON e.produto_variacao_id = pv.id
                          {whereClause}
                          GROUP BY p.id, p.produto, p.descricao, p.categoria_id, c.categoria,
                                   p.marca_id, m.marca, p.unidade_medida_id, u.unidade_medida,
                                   p.ativo, p.criado_em
                          ORDER BY {orderBy}
                          LIMIT @Limit OFFSET @Offset";

        var param = new
        {
            Busca = $"%{filtro.Busca}%",
            BuscaExata = filtro.Busca,
            Limit = filtro.TamanhoPagina,
            Offset = (filtro.Pagina - 1) * filtro.TamanhoPagina
        };

        var total = await conn.ExecuteScalarAsync<int>(sqlCount, param);
        var itens = await conn.QueryAsync<ProdutoListDto>(sqlData, param);
        return new PaginacaoDto<ProdutoListDto>
        {
            Itens = itens.ToList(),
            TotalItens = total,
            Pagina = filtro.Pagina,
            TamanhoPagina = filtro.TamanhoPagina
        };
    }

    public async Task<IEnumerable<ProdutoListDto>> ObterTodosAtivosAsync()
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryAsync<ProdutoListDto>(
            @"SELECT p.id, p.produto AS Produto, u.unidade_medida AS SiglaUnidade
              FROM produtos p
              INNER JOIN unidades_medida u ON u.id = p.unidade_medida_id
              WHERE p.ativo = TRUE ORDER BY p.produto");
    }

    public async Task<Produto?> ObterPorIdAsync(int id)
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Produto>(
            @"SELECT p.id,
                     p.produto            AS NomeProduto,
                     p.descricao          AS Descricao,
                     p.categoria_id       AS CategoriaId,
                     c.categoria          AS NomeCategoria,
                     p.marca_id           AS MarcaId,
                     m.marca              AS NomeMarca,
                     p.unidade_medida_id  AS UnidadeMedidaId,
                     u.unidade_medida     AS SiglaUnidade,
                     p.ativo,
                     p.criado_em          AS CriadoEm,
                     p.atualizado_em      AS AtualizadoEm,
                     ua.nome              AS NomeAtualizadoPor
              FROM produtos p
              INNER JOIN categorias      c  ON c.id  = p.categoria_id
              INNER JOIN marcas          m  ON m.id  = p.marca_id
              INNER JOIN unidades_medida u  ON u.id  = p.unidade_medida_id
              LEFT  JOIN usuarios        ua ON ua.id = p.atualizado_por
              WHERE p.id = @id", new { id });
    }

    public async Task<List<ProdutoVariacaoDto>> ObterVariacoesPorProdutoAsync(int produtoId)
    {
        using var conn = _factory.CreateConnection();
        var result = await conn.QueryAsync<ProdutoVariacaoDto>(
            @"SELECT pv.id,
                     pv.id            AS IdOriginal,
                     pv.produto_id    AS ProdutoId,
                     pv.cor           AS Cor,
                     pv.tamanho       AS Tamanho,
                     pv.codigo_barras AS CodigoBarras,
                     pv.preco         AS Preco,
                     pv.ativo         AS Ativo,
                     COALESCE(e.quantidade, 0) AS QuantidadeEstoque,
                     pv.atualizado_em AS AtualizadoEm,
                     ua.nome          AS NomeAtualizadoPor
              FROM produto_variacoes pv
              LEFT JOIN estoque  e  ON e.produto_variacao_id = pv.id
              LEFT JOIN usuarios ua ON ua.id = pv.atualizado_por
              WHERE pv.produto_id = @produtoId
              ORDER BY pv.cor, pv.tamanho", new { produtoId });
        return result.ToList();
    }

    public async Task<int> InserirAsync(ProdutoDto dto)
    {
        using var conn = _factory.CreateConnection();
        var proximoId = await ProximoIdAsync();
        await conn.ExecuteAsync(
            @"INSERT INTO produtos (id, produto, descricao, categoria_id, marca_id, unidade_medida_id, ativo)
              VALUES (@ProximoId, @Produto, @Descricao, @CategoriaId, @MarcaId, @UnidadeMedidaId, @Ativo)",
            new
            {
                ProximoId = proximoId,
                dto.Produto,
                dto.Descricao,
                dto.CategoriaId,
                dto.MarcaId,
                dto.UnidadeMedidaId,
                dto.Ativo
            });
        return proximoId;
    }

    public async Task AtualizarAsync(ProdutoDto dto)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            @"UPDATE produtos
              SET id                = @Id,
                  produto           = @Produto,
                  descricao         = @Descricao,
                  categoria_id      = @CategoriaId,
                  marca_id          = @MarcaId,
                  unidade_medida_id = @UnidadeMedidaId,
                  atualizado_em     = NOW()
              WHERE id = @IdOriginal", dto);
    }

    public Task AlterarStatusAsync(int id, bool ativo)
        => AlterarStatusBaseAsync(id, ativo);

    public async Task<bool> ExisteNomeAsync(string nome, int? idOriginalIgnorar = null)
    {
        using var conn = _factory.CreateConnection();
        var sql = idOriginalIgnorar.HasValue
            ? "SELECT COUNT(*) FROM produtos WHERE produto = @nome AND id <> @idOriginalIgnorar"
            : "SELECT COUNT(*) FROM produtos WHERE produto = @nome";
        return await conn.ExecuteScalarAsync<int>(sql, new { nome, idOriginalIgnorar }) > 0;
    }

    public async Task<int> InserirVariacaoAsync(ProdutoVariacaoDto dto)
    {
        using var conn = _factory.CreateConnection();
        using var connId = _factory.CreateConnection();
        var proximoId = await connId.ExecuteScalarAsync<int>(
            @"SELECT MIN(seq) FROM (SELECT 1 AS seq UNION ALL SELECT id + 1 FROM produto_variacoes) t
              WHERE seq NOT IN (SELECT id FROM produto_variacoes)");
        await conn.ExecuteAsync(
            @"INSERT INTO produto_variacoes (id, produto_id, cor, tamanho, codigo_barras, preco, ativo)
              VALUES (@ProximoId, @ProdutoId, @Cor, @Tamanho, @CodigoBarras, @Preco, @Ativo)",
            new
            {
                ProximoId = proximoId,
                dto.ProdutoId,
                dto.Cor,
                dto.Tamanho,
                dto.CodigoBarras,
                dto.Preco,
                dto.Ativo
            });
        return proximoId;
    }

    public async Task AtualizarVariacaoAsync(ProdutoVariacaoDto dto)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            @"UPDATE produto_variacoes
              SET id            = @Id,
                  cor           = @Cor,
                  tamanho       = @Tamanho,
                  codigo_barras = @CodigoBarras,
                  preco         = @Preco,
                  atualizado_em = NOW()
              WHERE id = @IdOriginal", dto);
    }

    public async Task AlterarStatusVariacaoAsync(int id, bool ativo)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE produto_variacoes SET ativo = @ativo, atualizado_em = NOW() WHERE id = @id",
            new { ativo, id });
    }

    public async Task<bool> ExisteVariacaoAsync(int produtoId, string cor, string tamanho, int? idIgnorar = null)
    {
        using var conn = _factory.CreateConnection();
        var sql = idIgnorar.HasValue
            ? "SELECT COUNT(*) FROM produto_variacoes WHERE produto_id = @produtoId AND cor = @cor AND tamanho = @tamanho AND id <> @idIgnorar"
            : "SELECT COUNT(*) FROM produto_variacoes WHERE produto_id = @produtoId AND cor = @cor AND tamanho = @tamanho";
        return await conn.ExecuteScalarAsync<int>(sql, new { produtoId, cor, tamanho, idIgnorar }) > 0;
    }

    public async Task<bool> ExisteCodigoBarrasAsync(string codigoBarras, int? idIgnorar = null)
    {
        using var conn = _factory.CreateConnection();
        var sql = idIgnorar.HasValue
            ? "SELECT COUNT(*) FROM produto_variacoes WHERE codigo_barras = @codigoBarras AND id <> @idIgnorar"
            : "SELECT COUNT(*) FROM produto_variacoes WHERE codigo_barras = @codigoBarras";
        return await conn.ExecuteScalarAsync<int>(sql, new { codigoBarras, idIgnorar }) > 0;
    }

    public async Task InserirEstoqueAsync(int variacaoId)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            "INSERT IGNORE INTO estoque (produto_variacao_id, quantidade) VALUES (@variacaoId, 0)",
            new { variacaoId });
    }

    public async Task AtualizarEstoqueAsync(int variacaoId, int quantidade)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE estoque SET quantidade = @quantidade, atualizado_em = NOW() WHERE produto_variacao_id = @variacaoId",
            new { variacaoId, quantidade });
    }
}