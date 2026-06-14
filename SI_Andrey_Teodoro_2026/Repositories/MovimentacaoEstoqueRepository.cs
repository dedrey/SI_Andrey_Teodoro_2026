using Dapper;
using SI_Andrey_Teodoro_2026.Data;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Models;
using SI_Andrey_Teodoro_2026.Repositories.Interfaces;

namespace SI_Andrey_Teodoro_2026.Repositories;

public class MovimentacaoEstoqueRepository : BaseRepository, IMovimentacaoEstoqueRepository
{
    public MovimentacaoEstoqueRepository(DbConnectionFactory factory) : base(factory) { }

    protected override string Tabela => "movimentacoes_estoque";

    public async Task<PaginacaoDto<MovimentacaoEstoqueListDto>> ObterTodosAsync(FiltroConsultaDto filtro)
    {
        using var conn = _factory.CreateConnection();
        var where = new List<string>();
        if (!string.IsNullOrWhiteSpace(filtro.Busca))
            where.Add(@"(m.tipo_movimentacao LIKE @Busca
                      OR m.observacao        LIKE @Busca
                      OR m.numero_nf         LIKE @Busca
                      OR f.razaosocial       LIKE @Busca
                      OR CAST(m.id AS CHAR) = @BuscaExata)");
        if (filtro.StatusFiltro is "ENTRADA" or "SAIDA" or "AJUSTE")
            where.Add("m.tipo_movimentacao = @StatusFiltro");
        var whereClause = where.Count > 0 ? "WHERE " + string.Join(" AND ", where) : "";
        var orderBy = filtro.OrdenarPor switch
        {
            "tipo" => "m.tipo_movimentacao",
            "id" => "m.id",
            _ => "m.criado_em DESC"
        };
        var param = new
        {
            Busca = $"%{filtro.Busca}%",
            BuscaExata = filtro.Busca,
            StatusFiltro = filtro.StatusFiltro,
            Limit = filtro.TamanhoPagina,
            Offset = (filtro.Pagina - 1) * filtro.TamanhoPagina
        };
        var total = await conn.ExecuteScalarAsync<int>(
            $@"SELECT COUNT(*) FROM movimentacoes_estoque m
               LEFT JOIN fornecedores f ON f.id = m.fornecedor_id
               {whereClause}", param);
        var itens = await conn.QueryAsync<MovimentacaoEstoqueListDto>(
            $@"SELECT m.id,
                      m.tipo_movimentacao  AS TipoMovimentacao,
                      m.observacao,
                      m.numero_nf          AS NumeroNf,
                      f.razaosocial        AS NomeFornecedor,
                      COUNT(i.id)          AS TotalItens,
                      COALESCE(SUM(i.quantidade), 0)                   AS TotalQuantidade,
                      COALESCE(SUM(i.quantidade * i.valor_unitario), 0) AS ValorTotal,
                      m.criado_em          AS CriadoEm,
                      u.nome               AS NomeCriadoPor
               FROM movimentacoes_estoque m
               LEFT JOIN movimentacoes_estoque_itens i ON i.movimentacao_id = m.id
               LEFT JOIN fornecedores f ON f.id = m.fornecedor_id
               LEFT JOIN usuarios     u ON u.id = m.criado_por
               {whereClause}
               GROUP BY m.id, m.tipo_movimentacao, m.observacao, m.numero_nf,
                        f.razaosocial, m.criado_em, u.nome
               ORDER BY {orderBy} LIMIT @Limit OFFSET @Offset", param);
        return new PaginacaoDto<MovimentacaoEstoqueListDto>
        { Itens = itens.ToList(), TotalItens = total, Pagina = filtro.Pagina, TamanhoPagina = filtro.TamanhoPagina };
    }

    public async Task<MovimentacaoEstoque?> ObterPorIdAsync(int id)
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<MovimentacaoEstoque>(
            @"SELECT m.id,
                     m.tipo_movimentacao AS TipoMovimentacao,
                     m.observacao,
                     m.numero_nf         AS NumeroNf,
                     m.fornecedor_id     AS FornecedorId,
                     f.razaosocial       AS NomeFornecedor,
                     m.criado_em         AS CriadoEm,
                     m.criado_por        AS CriadoPor,
                     u.nome              AS NomeCriadoPor
              FROM movimentacoes_estoque m
              LEFT JOIN fornecedores f ON f.id  = m.fornecedor_id
              LEFT JOIN usuarios     u ON u.id  = m.criado_por
              WHERE m.id = @id", new { id });
    }

    public async Task<List<MovimentacaoEstoqueItemListDto>> ObterItensPorMovimentacaoAsync(int movimentacaoId)
    {
        using var conn = _factory.CreateConnection();
        var result = await conn.QueryAsync<MovimentacaoEstoqueItemListDto>(
            @"SELECT i.id, i.movimentacao_id AS MovimentacaoId,
                     i.produto_variacao_id AS ProdutoVariacaoId,
                     p.produto AS NomeProduto, pv.cor AS Cor, pv.tamanho AS Tamanho,
                     i.quantidade, i.valor_unitario AS ValorUnitario,
                     (i.quantidade * i.valor_unitario) AS ValorTotal
              FROM movimentacoes_estoque_itens i
              INNER JOIN produto_variacoes pv ON pv.id = i.produto_variacao_id
              INNER JOIN produtos          p  ON p.id  = pv.produto_id
              WHERE i.movimentacao_id = @movimentacaoId
              ORDER BY p.produto, pv.cor, pv.tamanho", new { movimentacaoId });
        return result.ToList();
    }

    public async Task<int> InserirAsync(MovimentacaoEstoqueDto dto)
    {
        using var conn = _factory.CreateConnection();
        var proximoId = await ProximoIdAsync();
        await conn.ExecuteAsync(
            @"INSERT INTO movimentacoes_estoque
                (id, tipo_movimentacao, observacao, numero_nf, fornecedor_id)
              VALUES
                (@ProximoId, @TipoMovimentacao, @Observacao, @NumeroNf, @FornecedorId)",
            new
            {
                ProximoId = proximoId,
                dto.TipoMovimentacao,
                dto.Observacao,
                dto.NumeroNf,
                dto.FornecedorId
            });
        return proximoId;
    }

    public async Task InserirItemAsync(MovimentacaoEstoqueItemDto item, int movimentacaoId)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            @"INSERT INTO movimentacoes_estoque_itens
                (movimentacao_id, produto_variacao_id, quantidade, valor_unitario)
              VALUES (@MovimentacaoId, @ProdutoVariacaoId, @Quantidade, @ValorUnitario)",
            new { MovimentacaoId = movimentacaoId, item.ProdutoVariacaoId, item.Quantidade, item.ValorUnitario });
    }

    public async Task AtualizarEstoqueAsync(int variacaoId, int delta)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE estoque SET quantidade = quantidade + @delta, atualizado_em = NOW() WHERE produto_variacao_id = @variacaoId",
            new { delta, variacaoId });
    }

    public async Task<int> ObterEstoqueAtualAsync(int variacaoId)
    {
        using var conn = _factory.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            "SELECT COALESCE(quantidade, 0) FROM estoque WHERE produto_variacao_id = @variacaoId",
            new { variacaoId });
    }

    public async Task AtualizarDataUltimaCompraAsync(int variacaoId, DateTime data)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE produto_variacoes SET data_ultima_compra = @data WHERE id = @variacaoId",
            new { variacaoId, data });
    }

    public async Task AtualizarPrecoCustoAsync(int variacaoId, decimal precoCusto)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE produto_variacoes SET preco_custo = @precoCusto WHERE id = @variacaoId",
            new { variacaoId, precoCusto });
    }
}