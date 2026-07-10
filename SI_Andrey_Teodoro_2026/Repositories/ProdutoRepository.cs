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
                      OR p.codigo_barras LIKE @Busca
                      OR CAST(p.id AS CHAR) = @BuscaExata)");
        where.Add(filtro.StatusFiltro switch
        {
            "ativos" => "p.ativo = TRUE",
            "inativos" => "p.ativo = FALSE",
            _ => "1=1"
        });
        var whereClause = "WHERE " + string.Join(" AND ", where);
        var orderBy = filtro.OrdenarPor switch { "id" => "p.id", "data" => "p.criado_em", _ => "p.produto" };

        var sqlCount = $@"SELECT COUNT(*)
                          FROM produtos p
                          INNER JOIN categorias      c ON c.id = p.categoria_id
                          INNER JOIN marcas          m ON m.id = p.marca_id
                          INNER JOIN unidades_medida u ON u.id = p.unidade_medida_id
                          {whereClause}";

        var sqlData = $@"SELECT p.id,
                                 p.produto            AS Produto,
                                 p.descricao          AS Descricao,
                                 p.codigo_barras       AS CodigoBarras,
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
                          GROUP BY p.id, p.produto, p.descricao, p.codigo_barras, p.categoria_id, c.categoria,
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
                     p.codigo_barras      AS CodigoBarras,
                     p.preco_compra       AS PrecoCompra,
                     p.frete              AS Frete,
                     p.preco_custo        AS PrecoCusto,
                     p.categoria_id       AS CategoriaId,
                     c.categoria          AS NomeCategoria,
                     p.marca_id           AS MarcaId,
                     m.marca              AS NomeMarca,
                     p.unidade_medida_id  AS UnidadeMedidaId,
                     u.unidade_medida     AS SiglaUnidade,
                     -- Fornecedor e NF vêm da última entrada vinculada às variações do produto
                     (SELECT f2.razaosocial
                      FROM movimentacoes_estoque       me
                      INNER JOIN movimentacoes_estoque_itens mei ON mei.movimentacao_id = me.id
                      INNER JOIN produto_variacoes     pv2       ON pv2.id = mei.produto_variacao_id
                      INNER JOIN fornecedores          f2        ON f2.id  = me.fornecedor_id
                      WHERE pv2.produto_id = p.id
                        AND me.tipo_movimentacao = 'ENTRADA'
                        AND me.fornecedor_id IS NOT NULL
                      ORDER BY me.criado_em DESC LIMIT 1) AS NomeFornecedor,
                     (SELECT me2.numero_nf
                      FROM movimentacoes_estoque       me2
                      INNER JOIN movimentacoes_estoque_itens mei2 ON mei2.movimentacao_id = me2.id
                      INNER JOIN produto_variacoes     pv3        ON pv3.id = mei2.produto_variacao_id
                      WHERE pv3.produto_id = p.id
                        AND me2.tipo_movimentacao = 'ENTRADA'
                        AND me2.numero_nf IS NOT NULL
                      ORDER BY me2.criado_em DESC LIMIT 1) AS NumeroNfUltimaEntrada,
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
                     pv.id                    AS IdOriginal,
                     pv.produto_id            AS ProdutoId,
                     pv.cor_id                AS CorId,
                     co.nome                  AS Cor,
                     pv.tamanho_id            AS TamanhoId,
                     ta.nome                  AS Tamanho,
                     pv.preco                 AS Preco,
                     pv.data_ultima_compra    AS DataUltimaCompra,
                     pv.ativo                 AS Ativo,
                     COALESCE(e.quantidade, 0) AS QuantidadeEstoque,
                     pv.atualizado_em         AS AtualizadoEm,
                     ua.nome                  AS NomeAtualizadoPor
              FROM produto_variacoes pv
              LEFT JOIN cores    co ON co.id = pv.cor_id
              LEFT JOIN tamanhos ta ON ta.id = pv.tamanho_id
              LEFT JOIN estoque  e  ON e.produto_variacao_id = pv.id
              LEFT JOIN usuarios ua ON ua.id = pv.atualizado_por
              WHERE pv.produto_id = @produtoId
              ORDER BY co.nome, ta.ordem, ta.nome", new { produtoId });
        return result.ToList();
    }

    public async Task<int> InserirAsync(ProdutoDto dto)
    {
        using var conn = _factory.CreateConnection();
        var proximoId = await ProximoIdAsync();
        await conn.ExecuteAsync(
            @"INSERT INTO produtos (id, produto, descricao, codigo_barras, preco_compra, frete, preco_custo,
                                    categoria_id, marca_id, unidade_medida_id, ativo)
              VALUES (@ProximoId, @Produto, @Descricao, @CodigoBarras, @PrecoCompra, @Frete, @PrecoCusto,
                      @CategoriaId, @MarcaId, @UnidadeMedidaId, @Ativo)",
            new
            {
                ProximoId = proximoId,
                dto.Produto,
                dto.Descricao,
                dto.CodigoBarras,
                dto.PrecoCompra,
                dto.Frete,
                dto.PrecoCusto,
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
                  codigo_barras     = @CodigoBarras,
                  preco_compra      = @PrecoCompra,
                  frete             = @Frete,
                  preco_custo       = @PrecoCusto,
                  categoria_id      = @CategoriaId,
                  marca_id          = @MarcaId,
                  unidade_medida_id = @UnidadeMedidaId,
                  atualizado_em     = NOW()
              WHERE id = @IdOriginal",
            new
            {
                dto.Id,
                dto.IdOriginal,
                dto.Produto,
                dto.Descricao,
                dto.CodigoBarras,
                dto.PrecoCompra,
                dto.Frete,
                dto.PrecoCusto,
                dto.CategoriaId,
                dto.MarcaId,
                dto.UnidadeMedidaId
            });
    }

    public Task AlterarStatusAsync(int id, bool ativo) => AlterarStatusBaseAsync(id, ativo);

    public async Task<bool> ExisteNomeAsync(string nome, int? idOriginalIgnorar = null)
    {
        using var conn = _factory.CreateConnection();
        var sql = idOriginalIgnorar.HasValue
            ? "SELECT COUNT(*) FROM produtos WHERE produto = @nome AND id <> @idOriginalIgnorar"
            : "SELECT COUNT(*) FROM produtos WHERE produto = @nome";
        return await conn.ExecuteScalarAsync<int>(sql, new { nome, idOriginalIgnorar }) > 0;
    }

    public async Task<bool> ExisteCodigoBarrasAsync(string codigoBarras, int? idIgnorar = null)
    {
        using var conn = _factory.CreateConnection();
        var sql = idIgnorar.HasValue
            ? "SELECT COUNT(*) FROM produtos WHERE codigo_barras = @codigoBarras AND id <> @idIgnorar"
            : "SELECT COUNT(*) FROM produtos WHERE codigo_barras = @codigoBarras";
        return await conn.ExecuteScalarAsync<int>(sql, new { codigoBarras, idIgnorar }) > 0;
    }

    /// Salva o produto inteiro (dados do produto + todas as variações + estoque) numa única
    /// transação: ou tudo é gravado, ou nada é — evita o cenário de "salvou 2 de 6 variações
    /// e travou no meio" quando alguma variação dá erro (ex: cor/tamanho duplicado).
    public async Task<int> SalvarComVariacoesAsync(ProdutoDto dto, List<ProdutoVariacaoDto> variacoes)
    {
        using var conn = _factory.CreateConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();
        try
        {
            int produtoId;
            if (dto.IdOriginal == 0)
            {
                var proximoId = await ProximoIdAsync();
                await conn.ExecuteAsync(
                    @"INSERT INTO produtos (id, produto, descricao, codigo_barras, preco_compra, frete, preco_custo,
                                            categoria_id, marca_id, unidade_medida_id, ativo)
                      VALUES (@ProximoId, @Produto, @Descricao, @CodigoBarras, @PrecoCompra, @Frete, @PrecoCusto,
                              @CategoriaId, @MarcaId, @UnidadeMedidaId, @Ativo)",
                    new
                    {
                        ProximoId = proximoId,
                        dto.Produto,
                        dto.Descricao,
                        dto.CodigoBarras,
                        dto.PrecoCompra,
                        dto.Frete,
                        dto.PrecoCusto,
                        dto.CategoriaId,
                        dto.MarcaId,
                        dto.UnidadeMedidaId,
                        dto.Ativo
                    }, tx);
                produtoId = proximoId;
            }
            else
            {
                produtoId = dto.Id;
                await conn.ExecuteAsync(
                    @"UPDATE produtos
                      SET id                = @Id,
                          produto           = @Produto,
                          descricao         = @Descricao,
                          codigo_barras     = @CodigoBarras,
                          preco_compra      = @PrecoCompra,
                          frete             = @Frete,
                          preco_custo       = @PrecoCusto,
                          categoria_id      = @CategoriaId,
                          marca_id          = @MarcaId,
                          unidade_medida_id = @UnidadeMedidaId,
                          atualizado_em     = NOW()
                      WHERE id = @IdOriginal",
                    new
                    {
                        dto.Id,
                        dto.IdOriginal,
                        dto.Produto,
                        dto.Descricao,
                        dto.CodigoBarras,
                        dto.PrecoCompra,
                        dto.Frete,
                        dto.PrecoCusto,
                        dto.CategoriaId,
                        dto.MarcaId,
                        dto.UnidadeMedidaId
                    }, tx);
            }

            foreach (var v in variacoes)
            {
                v.ProdutoId = produtoId;
                if (v.IdOriginal == 0)
                {
                    var proximoVarId = await conn.ExecuteScalarAsync<int>(
                        @"SELECT MIN(seq) FROM (SELECT 1 AS seq UNION ALL SELECT id + 1 FROM produto_variacoes) t
                          WHERE seq NOT IN (SELECT id FROM produto_variacoes)", transaction: tx);
                    await conn.ExecuteAsync(
                        @"INSERT INTO produto_variacoes (id, produto_id, cor, cor_id, tamanho, tamanho_id, preco, ativo)
                          VALUES (@ProximoVarId, @ProdutoId, @Cor, @CorId, @Tamanho, @TamanhoId, @Preco, @Ativo)",
                        new
                        {
                            ProximoVarId = proximoVarId,
                            v.ProdutoId,
                            v.Cor,
                            v.CorId,
                            v.Tamanho,
                            v.TamanhoId,
                            v.Preco,
                            v.Ativo
                        }, tx);
                    await conn.ExecuteAsync(
                        "INSERT IGNORE INTO estoque (produto_variacao_id, quantidade) VALUES (@variacaoId, 0)",
                        new { variacaoId = proximoVarId }, tx);
                }
                else
                {
                    await conn.ExecuteAsync(
                        @"UPDATE produto_variacoes
                          SET id            = @Id,
                              cor           = @Cor,
                              cor_id        = @CorId,
                              tamanho       = @Tamanho,
                              tamanho_id    = @TamanhoId,
                              preco         = @Preco,
                              atualizado_em = NOW()
                          WHERE id = @IdOriginal",
                        new { v.Id, v.IdOriginal, v.Cor, v.CorId, v.Tamanho, v.TamanhoId, v.Preco }, tx);
                }
            }

            tx.Commit();
            return produtoId;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    public async Task AlterarStatusVariacaoAsync(int id, bool ativo)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE produto_variacoes SET ativo = @ativo, atualizado_em = NOW() WHERE id = @id",
            new { ativo, id });
    }

    public async Task<bool> ExisteVariacaoAsync(int produtoId, int corId, int tamanhoId, int? idIgnorar = null)
    {
        using var conn = _factory.CreateConnection();
        var sql = idIgnorar.HasValue
            ? "SELECT COUNT(*) FROM produto_variacoes WHERE produto_id = @produtoId AND cor_id = @corId AND tamanho_id = @tamanhoId AND id <> @idIgnorar"
            : "SELECT COUNT(*) FROM produto_variacoes WHERE produto_id = @produtoId AND cor_id = @corId AND tamanho_id = @tamanhoId";
        return await conn.ExecuteScalarAsync<int>(sql, new { produtoId, corId, tamanhoId, idIgnorar }) > 0;
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

    public async Task AtualizarDataUltimaCompraAsync(int variacaoId, DateTime data)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE produto_variacoes SET data_ultima_compra = @data WHERE id = @variacaoId",
            new { variacaoId, data });
    }
}