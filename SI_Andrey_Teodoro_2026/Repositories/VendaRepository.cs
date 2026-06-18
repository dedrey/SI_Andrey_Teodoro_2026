using Dapper;
using SI_Andrey_Teodoro_2026.Data;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Models;
using SI_Andrey_Teodoro_2026.Repositories.Interfaces;

namespace SI_Andrey_Teodoro_2026.Repositories;

public class VendaRepository : BaseRepository, IVendaRepository
{
    public VendaRepository(DbConnectionFactory factory) : base(factory) { }

    protected override string Tabela => "vendas";

    public async Task<PaginacaoDto<VendaListDto>> ObterTodosAsync(FiltroConsultaDto filtro)
    {
        using var conn = _factory.CreateConnection();
        var where = new List<string>();

        if (!string.IsNullOrWhiteSpace(filtro.Busca))
            where.Add(@"(c.nome_razaosocial LIKE @Busca OR CAST(v.id AS CHAR) = @BuscaExata)");

        where.Add(filtro.StatusFiltro switch
        {
            "ABERTA" => "v.status_venda = 'ABERTA'",
            "FINALIZADA" => "v.status_venda = 'FINALIZADA'",
            "CANCELADA" => "v.status_venda = 'CANCELADA'",
            _ => "1=1"
        });

        var whereClause = where.Count > 0 ? "WHERE " + string.Join(" AND ", where) : "";
        var orderBy = filtro.OrdenarPor switch
        {
            "id" => "v.id",
            "total" => "v.valor_total DESC",
            _ => "v.criado_em DESC"
        };

        var sqlCount = $@"SELECT COUNT(*) FROM vendas v
                          LEFT JOIN clientes c ON c.id = v.cliente_id
                          {whereClause}";

        var sqlData = $@"SELECT v.id,
                                COALESCE(c.nome_razaosocial, 'Sem cliente') AS NomeCliente,
                                cp.condicao_pagamento          AS NomeCondicao,
                                COUNT(vi.id)                   AS TotalItens,
                                v.valor_subtotal               AS ValorSubtotal,
                                v.valor_desconto               AS ValorDesconto,
                                v.valor_total                  AS ValorTotal,
                                COALESCE(cp.desconto_percentual,   0) AS DescontoPercentual,
                                COALESCE(cp.acrescimo_percentual,  0) AS AcrescimoPercentual,
                                COALESCE(cp.taxa_juros_percentual, 0) AS TaxaJurosPercentual,
                                COALESCE(cp.numero_parcelas,       1) AS NumeroParcelas,
                                v.status_venda                 AS StatusVenda,
                                v.motivo_cancelamento          AS MotivoCancelamento,
                                v.criado_em                    AS CriadoEm
                         FROM vendas v
                         LEFT JOIN clientes             c  ON c.id  = v.cliente_id
                         LEFT JOIN condicoes_pagamentos cp ON cp.id = v.condicao_pagamento_id
                         LEFT JOIN vendas_itens         vi ON vi.venda_id = v.id
                         {whereClause}
                         GROUP BY v.id, c.nome_razaosocial, cp.condicao_pagamento,
                                  v.valor_subtotal, v.valor_desconto, v.valor_total,
                                  cp.desconto_percentual, cp.acrescimo_percentual,
                                  cp.taxa_juros_percentual, cp.numero_parcelas,
                                  v.status_venda, v.motivo_cancelamento, v.criado_em
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
        var itens = await conn.QueryAsync<VendaListDto>(sqlData, param);
        return new PaginacaoDto<VendaListDto>
        {
            Itens = itens.ToList(),
            TotalItens = total,
            Pagina = filtro.Pagina,
            TamanhoPagina = filtro.TamanhoPagina
        };
    }

    public async Task<Venda?> ObterPorIdAsync(int id)
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Venda>(
            @"SELECT v.id,
                     v.cliente_id            AS ClienteId,
                     COALESCE(c.nome_razaosocial, '') AS NomeCliente,
                     v.condicao_pagamento_id AS CondicaoPagamentoId,
                     cp.condicao_pagamento   AS NomeCondicao,
                     v.movimentacao_id       AS MovimentacaoId,
                     v.valor_subtotal        AS ValorSubtotal,
                     v.valor_desconto        AS ValorDesconto,
                     v.valor_total           AS ValorTotal,
                     v.status_venda          AS StatusVenda,
                     v.criado_em             AS CriadoEm,
                     v.atualizado_em         AS AtualizadoEm
              FROM vendas v
              LEFT JOIN clientes            c  ON c.id  = v.cliente_id
              LEFT JOIN condicoes_pagamentos cp ON cp.id = v.condicao_pagamento_id
              WHERE v.id = @id", new { id });
    }

    public async Task<List<VendaItemListDto>> ObterItensPorVendaAsync(int vendaId)
    {
        using var conn = _factory.CreateConnection();
        var result = await conn.QueryAsync<VendaItemListDto>(
            @"SELECT vi.id,
                     vi.venda_id             AS VendaId,
                     vi.produto_variacao_id  AS ProdutoVariacaoId,
                     p.produto               AS NomeProduto,
                     pv.cor                  AS Cor,
                     pv.tamanho              AS Tamanho,
                     vi.quantidade,
                     vi.valor_unitario       AS ValorUnitario,
                     vi.valor_desconto       AS ValorDesconto,
                     vi.valor_total          AS ValorTotal
              FROM vendas_itens vi
              INNER JOIN produto_variacoes pv ON pv.id = vi.produto_variacao_id
              INNER JOIN produtos          p  ON p.id  = pv.produto_id
              WHERE vi.venda_id = @vendaId
              ORDER BY p.produto, pv.cor, pv.tamanho", new { vendaId });
        return result.ToList();
    }

    public async Task<int> InserirAsync(VendaDto dto)
    {
        using var conn = _factory.CreateConnection();
        var proximoId = await ProximoIdAsync();
        await conn.ExecuteAsync(
            @"INSERT INTO vendas (id, cliente_id, condicao_pagamento_id,
                                  valor_subtotal, valor_desconto, valor_total, status_venda)
              VALUES (@ProximoId, @ClienteId, @CondicaoPagamentoId,
                      @ValorSubtotal, @ValorDesconto, @ValorTotal, 'ABERTA')",
            new
            {
                ProximoId = proximoId,
                dto.ClienteId,
                dto.CondicaoPagamentoId,
                dto.ValorSubtotal,
                dto.ValorDesconto,
                dto.ValorTotal
            });
        return proximoId;
    }

    public async Task AtualizarAsync(VendaDto dto)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            @"UPDATE vendas
              SET cliente_id            = @ClienteId,
                  condicao_pagamento_id = @CondicaoPagamentoId,
                  valor_subtotal        = @ValorSubtotal,
                  valor_desconto        = @ValorDesconto,
                  valor_total           = @ValorTotal,
                  atualizado_em         = NOW()
              WHERE id = @IdOriginal",
            new
            {
                dto.ClienteId,
                dto.CondicaoPagamentoId,
                dto.ValorSubtotal,
                dto.ValorDesconto,
                dto.ValorTotal,
                dto.IdOriginal
            });
    }

    public async Task InserirItemAsync(VendaItemDto item, int vendaId)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            @"INSERT INTO vendas_itens (venda_id, produto_variacao_id, quantidade,
                                        valor_unitario, valor_desconto, valor_total)
              VALUES (@VendaId, @ProdutoVariacaoId, @Quantidade,
                      @ValorUnitario, @ValorDesconto, @ValorTotalItem)",
            new
            {
                VendaId = vendaId,
                item.ProdutoVariacaoId,
                item.Quantidade,
                item.ValorUnitario,
                item.ValorDesconto,
                ValorTotalItem = item.ValorTotal
            });
    }

    public async Task RemoverItensAsync(int vendaId)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync("DELETE FROM vendas_itens WHERE venda_id = @vendaId", new { vendaId });
    }

    public async Task AtualizarTotaisAsync(int vendaId, decimal subtotal, decimal desconto, decimal total)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            @"UPDATE vendas SET valor_subtotal = @subtotal, valor_desconto = @desconto,
                                valor_total = @total, atualizado_em = NOW()
              WHERE id = @vendaId",
            new { vendaId, subtotal, desconto, total });
    }

    public async Task AtualizarStatusAsync(int vendaId, string status,
        int? movimentacaoId = null, string? motivoCancelamento = null)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            @"UPDATE vendas
              SET status_venda          = @status,
                  movimentacao_id       = COALESCE(@movimentacaoId, movimentacao_id),
                  motivo_cancelamento   = COALESCE(@motivoCancelamento, motivo_cancelamento),
                  atualizado_em         = NOW()
              WHERE id = @vendaId",
            new { vendaId, status, movimentacaoId, motivoCancelamento });
    }

    public async Task AtualizarEstoqueAsync(int variacaoId, int delta)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            @"UPDATE estoque SET quantidade = quantidade + @delta, atualizado_em = NOW()
              WHERE produto_variacao_id = @variacaoId",
            new { delta, variacaoId });
    }

    public async Task<int> ObterEstoqueAtualAsync(int variacaoId)
    {
        using var conn = _factory.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            "SELECT COALESCE(quantidade, 0) FROM estoque WHERE produto_variacao_id = @variacaoId",
            new { variacaoId });
    }

    public async Task<int> InserirMovimentacaoSaidaAsync(int vendaId)
    {
        using var conn = _factory.CreateConnection();
        var proximoId = await conn.ExecuteScalarAsync<int>(
            @"SELECT MIN(seq)
              FROM (SELECT 1 AS seq
                    UNION ALL
                    SELECT id + 1 FROM movimentacoes_estoque) t
              WHERE seq NOT IN (SELECT id FROM movimentacoes_estoque)
                AND seq NOT IN (
                    SELECT movimentacao_id FROM vendas
                    WHERE movimentacao_id IS NOT NULL
                )");

        await conn.ExecuteAsync(
            @"INSERT INTO movimentacoes_estoque (id, tipo_movimentacao, observacao)
              VALUES (@proximoId, 'SAIDA', @obs)",
            new { proximoId, obs = $"Saída automática — Venda #{vendaId}" });

        return proximoId;
    }

    public async Task InserirMovimentacaoItemAsync(int movimentacaoId, int variacaoId,
        int quantidade, decimal valorUnitario)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            @"INSERT INTO movimentacoes_estoque_itens
                (movimentacao_id, produto_variacao_id, quantidade, valor_unitario)
              VALUES (@movimentacaoId, @variacaoId, @quantidade, @valorUnitario)",
            new { movimentacaoId, variacaoId, quantidade, valorUnitario });
    }

    public async Task<int> InserirContaReceberAsync(int clienteId, int vendaId,
        string descricao, DateTime vencimento, decimal valor, bool jaRecebida = false)
    {
        using var conn = _factory.CreateConnection();
        var proximoId = await conn.ExecuteScalarAsync<int>(
            @"SELECT MIN(seq) FROM (SELECT 1 AS seq UNION ALL SELECT id + 1 FROM contas_receber) t
              WHERE seq NOT IN (SELECT id FROM contas_receber)");

        var status = jaRecebida ? "RECEBIDA" : "ABERTA";
        var saldo = jaRecebida ? 0m : valor;

        await conn.ExecuteAsync(
            @"INSERT INTO contas_receber (id, cliente_id, venda_id, descricao,
                                          data_vencimento, valor_original, valor_saldo, status)
              VALUES (@proximoId, @clienteId, @vendaId, @descricao,
                      @vencimento, @valor, @saldo, @status)",
            new { proximoId, clienteId, vendaId, descricao, vencimento, valor, saldo, status });
        if (jaRecebida)
        {
            var proximoBaixaId = await conn.ExecuteScalarAsync<int>(
                @"SELECT MIN(seq) FROM (SELECT 1 AS seq UNION ALL SELECT id + 1 FROM contas_receber_baixas) t
                  WHERE seq NOT IN (SELECT id FROM contas_receber_baixas)");
            await conn.ExecuteAsync(
                @"INSERT INTO contas_receber_baixas
                    (id, conta_receber_id, data_recebimento, valor_recebido, observacao)
                  VALUES (@proximoBaixaId, @proximoId, @vencimento, @valor, 'Pagamento instantâneo no ato da venda')",
                new { proximoBaixaId, proximoId, vencimento, valor });
        }

        return proximoId;
    }
}