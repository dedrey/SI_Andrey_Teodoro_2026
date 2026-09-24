using System.Data;
using Dapper;
using SI_Andrey_Teodoro_2026.Data;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Models;
using SI_Andrey_Teodoro_2026.Repositories.Interfaces;

namespace SI_Andrey_Teodoro_2026.Repositories;

public class CompraRepository : BaseRepository, ICompraRepository
{
    public CompraRepository(DbConnectionFactory factory) : base(factory) { }

    protected override string Tabela => "compras";

    // ════════════════════════ LEITURAS ════════════════════════

    public async Task<PaginacaoDto<CompraListDto>> ObterTodosAsync(FiltroConsultaDto filtro)
    {
        using var conn = _factory.CreateConnection();
        var where = new List<string>();

        if (!string.IsNullOrWhiteSpace(filtro.Busca))
            where.Add(@"(f.razaosocial LIKE @Busca OR c.numero_nf LIKE @Busca OR CAST(c.id AS CHAR) = @BuscaExata)");

        where.Add(filtro.StatusFiltro switch
        {
            "LANCADO" => "c.status_compra = 'LANCADO'",
            "CANCELADO" => "c.status_compra = 'CANCELADO'",
            _ => "1=1"
        });

        var whereClause = where.Count > 0 ? "WHERE " + string.Join(" AND ", where) : "";
        var orderBy = filtro.OrdenarPor switch
        {
            "id" => "c.id",
            "total" => "c.valor_total DESC",
            _ => "c.criado_em DESC"
        };

        var sqlCount = $@"SELECT COUNT(*) FROM compras c
                          LEFT JOIN fornecedores f ON f.id = c.fornecedor_id
                          {whereClause}";

        var sqlData = $@"SELECT c.id,
                                COALESCE(f.razaosocial, 'Sem fornecedor') AS NomeFornecedor,
                                c.numero_nf              AS NumeroNf,
                                COUNT(ci.id)              AS TotalItens,
                                c.valor_subtotal          AS ValorSubtotal,
                                c.valor_frete             AS ValorFrete,
                                c.valor_outros_acrescimos AS ValorOutrosAcrescimos,
                                c.valor_desconto          AS ValorDesconto,
                                c.valor_total             AS ValorTotal,
                                c.data_emissao            AS DataEmissao,
                                c.data_chegada            AS DataChegada,
                                c.status_compra           AS StatusCompra,
                                c.motivo_cancelamento     AS MotivoCancelamento,
                                c.criado_em                AS CriadoEm
                         FROM compras c
                         LEFT JOIN fornecedores   f  ON f.id = c.fornecedor_id
                         LEFT JOIN compras_itens  ci ON ci.compra_id = c.id
                         {whereClause}
                         GROUP BY c.id, f.razaosocial, c.numero_nf, c.valor_subtotal, c.valor_frete,
                                  c.valor_outros_acrescimos, c.valor_desconto, c.valor_total,
                                  c.data_emissao, c.data_chegada, c.status_compra,
                                  c.motivo_cancelamento, c.criado_em
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
        var itens = await conn.QueryAsync<CompraListDto>(sqlData, param);
        return new PaginacaoDto<CompraListDto>
        {
            Itens = itens.ToList(),
            TotalItens = total,
            Pagina = filtro.Pagina,
            TamanhoPagina = filtro.TamanhoPagina
        };
    }

    public async Task<Compra?> ObterPorIdAsync(int id)
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Compra>(
            @"SELECT c.id,
                     c.fornecedor_id            AS FornecedorId,
                     COALESCE(f.razaosocial, '') AS NomeFornecedor,
                     c.numero_nf                AS NumeroNf,
                     c.data_emissao              AS DataEmissao,
                     c.data_chegada              AS DataChegada,
                     c.condicao_pagamento_id     AS CondicaoPagamentoId,
                     cp.condicao_pagamento       AS NomeCondicaoPagamento,
                     c.valor_subtotal            AS ValorSubtotal,
                     c.valor_frete               AS ValorFrete,
                     c.valor_outros_acrescimos   AS ValorOutrosAcrescimos,
                     c.valor_desconto            AS ValorDesconto,
                     c.valor_total               AS ValorTotal,
                     c.status_compra             AS StatusCompra,
                     c.motivo_cancelamento       AS MotivoCancelamento,
                     c.criado_em                 AS CriadoEm,
                     c.atualizado_em             AS AtualizadoEm
              FROM compras c
              LEFT JOIN fornecedores       f  ON f.id  = c.fornecedor_id
              LEFT JOIN condicoes_pagamentos cp ON cp.id = c.condicao_pagamento_id
              WHERE c.id = @id", new { id });
    }

    public async Task<List<CompraItemListDto>> ObterItensPorCompraAsync(int compraId)
    {
        using var conn = _factory.CreateConnection();
        var result = await conn.QueryAsync<CompraItemListDto>(
            @"SELECT ci.id,
                     ci.compra_id              AS CompraId,
                     ci.produto_variacao_id    AS ProdutoVariacaoId,
                     p.produto                 AS NomeProduto,
                     pv.cor                    AS Cor,
                     pv.tamanho                AS Tamanho,
                     ci.quantidade,
                     ci.valor_unitario         AS ValorUnitario,
                     ci.valor_desconto         AS ValorDesconto,
                     ci.valor_total            AS ValorTotal,
                     ci.custo_unitario_efetivo AS CustoUnitarioEfetivo
              FROM compras_itens ci
              INNER JOIN produto_variacoes pv ON pv.id = ci.produto_variacao_id
              INNER JOIN produtos          p  ON p.id  = pv.produto_id
              WHERE ci.compra_id = @compraId
              ORDER BY p.produto, pv.cor, pv.tamanho", new { compraId });
        return result.ToList();
    }

    // ════════════════════ ESCRITAS (transacionais) ════════════════════

    public async Task<int> InserirAsync(CompraDto dto, IDbTransaction tx)
    {
        // Id via AUTO_INCREMENT: ProximoIdAsync() usa outra conexão e não enxerga a transação
        return await tx.Connection!.ExecuteScalarAsync<int>(
            @"INSERT INTO compras
                (fornecedor_id, numero_nf, data_emissao, data_chegada, condicao_pagamento_id,
                 valor_subtotal, valor_frete, valor_outros_acrescimos, valor_desconto, valor_total, status_compra)
              VALUES
                (@FornecedorId, @NumeroNf, @DataEmissao, @DataChegada, @CondicaoPagamentoId,
                 @ValorSubtotal, @ValorFrete, @ValorOutrosAcrescimos, @ValorDesconto, @ValorTotal, 'LANCADO');
              SELECT LAST_INSERT_ID();",
            new
            {
                dto.FornecedorId,
                dto.NumeroNf,
                dto.DataEmissao,
                dto.DataChegada,
                dto.CondicaoPagamentoId,
                dto.ValorSubtotal,
                dto.ValorFrete,
                dto.ValorOutrosAcrescimos,
                dto.ValorDesconto,
                dto.ValorTotal
            }, tx);
    }

    public async Task InserirItemAsync(CompraItemDto item, int compraId, IDbTransaction tx)
    {
        await tx.Connection!.ExecuteAsync(
            @"INSERT INTO compras_itens (compra_id, produto_variacao_id, quantidade,
                                         valor_unitario, valor_desconto, valor_total, custo_unitario_efetivo)
              VALUES (@CompraId, @ProdutoVariacaoId, @Quantidade,
                      @ValorUnitario, @ValorDesconto, @ValorTotalItem, @CustoUnitarioEfetivo)",
            new
            {
                CompraId = compraId,
                item.ProdutoVariacaoId,
                item.Quantidade,
                item.ValorUnitario,
                item.ValorDesconto,
                ValorTotalItem = item.ValorTotal,
                item.CustoUnitarioEfetivo
            }, tx);
    }

    public async Task AtualizarStatusAsync(int compraId, string status, IDbTransaction tx, string? motivoCancelamento = null)
    {
        await tx.Connection!.ExecuteAsync(
            @"UPDATE compras
              SET status_compra       = @status,
                  motivo_cancelamento = COALESCE(@motivoCancelamento, motivo_cancelamento),
                  atualizado_em       = NOW()
              WHERE id = @compraId",
            new { compraId, status, motivoCancelamento }, tx);
    }

    public async Task<bool> AtualizarEstoqueAsync(int variacaoId, int delta, IDbTransaction tx)
    {
        // "quantidade >= -@delta" em vez de "quantidade + @delta >= 0":
        // se a coluna for UNSIGNED, a soma negativa estouraria erro no próprio WHERE
        var linhas = await tx.Connection!.ExecuteAsync(
            @"UPDATE estoque
              SET quantidade = quantidade + @delta, atualizado_em = NOW()
              WHERE produto_variacao_id = @variacaoId
                AND quantidade >= -@delta",
            new { delta, variacaoId }, tx);
        return linhas > 0;
    }

    public async Task RecalcularCustoVariacaoAsync(int variacaoId, IDbTransaction tx)
    {
        await tx.Connection!.ExecuteAsync(
            @"UPDATE produto_variacoes
              SET preco_custo = COALESCE((
                      SELECT ci.custo_unitario_efetivo
                      FROM compras_itens ci
                      INNER JOIN compras c ON c.id = ci.compra_id
                      WHERE ci.produto_variacao_id = @variacaoId
                        AND c.status_compra = 'LANCADO'
                      ORDER BY c.data_emissao DESC, c.id DESC
                      LIMIT 1), 0),
                  data_ultima_compra = (
                      SELECT c.data_emissao
                      FROM compras_itens ci
                      INNER JOIN compras c ON c.id = ci.compra_id
                      WHERE ci.produto_variacao_id = @variacaoId
                        AND c.status_compra = 'LANCADO'
                      ORDER BY c.data_emissao DESC, c.id DESC
                      LIMIT 1)
              WHERE id = @variacaoId",
            new { variacaoId }, tx);
    }
}