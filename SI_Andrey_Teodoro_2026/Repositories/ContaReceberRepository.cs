using System.Data;
using Dapper;
using SI_Andrey_Teodoro_2026.Data;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Models;
using SI_Andrey_Teodoro_2026.Repositories.Interfaces;

namespace SI_Andrey_Teodoro_2026.Repositories;

public class ContaReceberRepository : BaseRepository, IContaReceberRepository
{
    public ContaReceberRepository(DbConnectionFactory factory) : base(factory) { }

    protected override string Tabela => "contas_receber";

    public async Task<PaginacaoDto<ContaReceberListDto>> ObterTodosAsync(FiltroConsultaDto filtro)
    {
        using var conn = _factory.CreateConnection();
        var where = new List<string>();
        if (!string.IsNullOrWhiteSpace(filtro.Busca))
            where.Add(@"(cr.descricao    LIKE @Busca
                      OR c.nome_razaosocial LIKE @Busca
                      OR CAST(cr.id AS CHAR) = @BuscaExata
                      OR CAST(cr.venda_id AS CHAR) = @BuscaExata)");
        where.Add(filtro.StatusFiltro switch
        {
            "aberta" => "cr.status = 'ABERTA'",
            "recebida" => "cr.status = 'RECEBIDA'",
            "cancelada" => "cr.status = 'CANCELADA'",
            "vencidas" => "cr.status = 'ABERTA' AND cr.data_vencimento < CURDATE()",
            _ => "1=1"
        });
        var whereClause = "WHERE " + string.Join(" AND ", where);
        var orderBy = filtro.OrdenarPor switch
        {
            "id" => "cr.id",
            "valor" => "cr.valor_original DESC",
            _ => "cr.data_vencimento, cr.id"
        };

        var sqlCount = $@"SELECT COUNT(*) FROM contas_receber cr
                          LEFT JOIN clientes c ON c.id = cr.cliente_id
                          {whereClause}";

        var sqlData = $@"
            SELECT cr.id,
                   cr.venda_id              AS VendaId,
                   c.nome_razaosocial       AS NomeCliente,
                   cr.descricao,
                   cr.data_vencimento       AS DataVencimento,
                   cr.valor_original        AS ValorOriginal,
                   cr.valor_saldo           AS ValorSaldo,
                   (SELECT COALESCE(SUM(crb.valor_recebido), 0)
                    FROM contas_receber_baixas crb
                    WHERE crb.conta_receber_id = cr.id) AS ValorRecebido,
                   (SELECT MAX(crb.data_recebimento)
                    FROM contas_receber_baixas crb
                    WHERE crb.conta_receber_id = cr.id) AS DataUltimoRecebimento,
                   (SELECT crb.id
                    FROM contas_receber_baixas crb
                    WHERE crb.conta_receber_id = cr.id
                    ORDER BY crb.data_recebimento DESC, crb.id DESC LIMIT 1) AS UltimaBaixaId,
                   (SELECT crb.comprovante_arquivo
                    FROM contas_receber_baixas crb
                    WHERE crb.conta_receber_id = cr.id
                    ORDER BY crb.data_recebimento DESC, crb.id DESC LIMIT 1) AS ComprovanteArquivo,
                   (SELECT COUNT(*)
                    FROM contas_receber x
                    WHERE x.venda_id = cr.venda_id
                      AND (x.data_vencimento < cr.data_vencimento
                           OR (x.data_vencimento = cr.data_vencimento AND x.id <= cr.id))) AS NumeroParcela,
                   (SELECT COUNT(*)
                    FROM contas_receber x
                    WHERE x.venda_id = cr.venda_id) AS TotalParcelas,
                   cr.status,
                   ve.motivo_cancelamento   AS MotivoCancelamentoVenda
            FROM contas_receber cr
            LEFT JOIN clientes c ON c.id = cr.cliente_id
            LEFT JOIN vendas  ve ON ve.id = cr.venda_id
            {whereClause}
            ORDER BY {orderBy} LIMIT @Limit OFFSET @Offset";

        var param = new
        {
            Busca = $"%{filtro.Busca}%",
            BuscaExata = filtro.Busca,
            Limit = filtro.TamanhoPagina,
            Offset = (filtro.Pagina - 1) * filtro.TamanhoPagina
        };

        var total = await conn.ExecuteScalarAsync<int>(sqlCount, param);
        var itens = await conn.QueryAsync<ContaReceberListDto>(sqlData, param);
        return new PaginacaoDto<ContaReceberListDto>
        { Itens = itens.ToList(), TotalItens = total, Pagina = filtro.Pagina, TamanhoPagina = filtro.TamanhoPagina };
    }

    public async Task<ContaReceber?> ObterPorIdAsync(int id)
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<ContaReceber>(
            @"SELECT cr.id,
                     cr.cliente_id        AS ClienteId,
                     c.nome_razaosocial   AS NomeCliente,
                     cr.venda_id          AS VendaId,
                     ve.motivo_cancelamento AS MotivoCancelamentoVenda,
                     DATE(ve.criado_em)   AS DataVenda,
                     cr.descricao,
                     cr.data_vencimento   AS DataVencimento,
                     cr.valor_original    AS ValorOriginal,
                     cr.valor_saldo       AS ValorSaldo,
                     cr.status,
                     cr.criado_em         AS CriadoEm,
                     cr.criado_por        AS CriadoPor,
                     cr.atualizado_em     AS AtualizadoEm,
                     ua.nome              AS NomeAtualizadoPor
              FROM contas_receber cr
              LEFT JOIN clientes  c  ON c.id  = cr.cliente_id
              LEFT JOIN vendas    ve ON ve.id = cr.venda_id
              LEFT JOIN usuarios  ua ON ua.id = cr.atualizado_por
              WHERE cr.id = @id", new { id });
    }

    public async Task<List<ContaReceberBaixaDto>> ObterBaixasAsync(int contaReceberId)
    {
        using var conn = _factory.CreateConnection();
        var result = await conn.QueryAsync<ContaReceberBaixaDto>(
            @"SELECT id, conta_receber_id AS ContaReceberId,
                     data_recebimento AS DataRecebimento,
                     valor_recebido   AS ValorRecebido,
                     comprovante_arquivo AS ComprovanteArquivo,
                     observacao,
                     criado_em AS CriadoEm
              FROM contas_receber_baixas
              WHERE conta_receber_id = @contaReceberId
              ORDER BY data_recebimento, id", new { contaReceberId });
        return result.ToList();
    }

    public async Task<int> InserirAsync(ContaReceberDto dto)
    {
        using var conn = _factory.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            @"INSERT INTO contas_receber
                (cliente_id, venda_id, descricao, data_vencimento,
                 valor_original, valor_saldo, status)
              VALUES
                (@ClienteId, @VendaId, @Descricao, @DataVencimento,
                 @ValorOriginal, @ValorOriginal, 'ABERTA');
              SELECT LAST_INSERT_ID();",
            new
            {
                dto.ClienteId,
                dto.VendaId,
                dto.Descricao,
                dto.DataVencimento,
                dto.ValorOriginal
            });
    }

    public async Task AtualizarAsync(ContaReceberDto dto)
    {
        using var conn = _factory.CreateConnection();

        await conn.ExecuteAsync(
            @"UPDATE contas_receber
              SET cliente_id      = @ClienteId,
                  descricao       = @Descricao,
                  data_vencimento = @DataVencimento,
                  valor_original  = @ValorOriginal,
                  atualizado_em   = NOW()
              WHERE id = @IdOriginal",
            new
            {
                dto.IdOriginal,
                dto.ClienteId,
                dto.Descricao,
                dto.DataVencimento,
                dto.ValorOriginal
            });
        await RecalcularSaldoAsync(conn, dto.IdOriginal);
    }

    public async Task AtualizarStatusAsync(int id, string status)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            @"UPDATE contas_receber
              SET status = @status,
                  valor_saldo = CASE WHEN @status = 'CANCELADA' THEN 0 ELSE valor_saldo END,
                  atualizado_em = NOW()
              WHERE id = @id",
            new { id, status });
    }

    public async Task<int> RegistrarBaixaAsync(int contaReceberId, DateTime dataRecebimento, decimal valorRecebido,
        string? comprovanteArquivo, string? observacao)
    {
        using var conn = _factory.CreateConnection();

        var baixaId = await conn.ExecuteScalarAsync<int>(
            @"INSERT INTO contas_receber_baixas
                (conta_receber_id, data_recebimento, valor_recebido, comprovante_arquivo, observacao)
              VALUES
                (@contaReceberId, @dataRecebimento, @valorRecebido, @comprovanteArquivo, @observacao);
              SELECT LAST_INSERT_ID();",
            new { contaReceberId, dataRecebimento, valorRecebido, comprovanteArquivo, observacao });

        await RecalcularSaldoAsync(conn, contaReceberId);

        return baixaId;
    }
    public async Task AtualizarComprovanteBaixaAsync(int baixaId, string comprovanteArquivo)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE contas_receber_baixas SET comprovante_arquivo = @comprovanteArquivo WHERE id = @baixaId",
            new { baixaId, comprovanteArquivo });
    }
    private static async Task RecalcularSaldoAsync(System.Data.IDbConnection conn, int contaReceberId)
    {
        var valorOriginal = await conn.ExecuteScalarAsync<decimal>(
            "SELECT valor_original FROM contas_receber WHERE id = @id", new { id = contaReceberId });

        var totalRecebido = await conn.ExecuteScalarAsync<decimal>(
            "SELECT COALESCE(SUM(valor_recebido), 0) FROM contas_receber_baixas WHERE conta_receber_id = @id",
            new { id = contaReceberId });

        var novoSaldo = valorOriginal - totalRecebido;
        if (novoSaldo < 0) novoSaldo = 0;

        var novoStatus = novoSaldo <= 0 ? "RECEBIDA" : "ABERTA";

        await conn.ExecuteAsync(
            @"UPDATE contas_receber
              SET valor_saldo = @novoSaldo, status = @novoStatus, atualizado_em = NOW()
              WHERE id = @id AND status <> 'CANCELADA'",
            new { id = contaReceberId, novoSaldo, novoStatus });
    }
    public async Task<List<ContaReceberResumoVendaDto>> ObterContasDaVendaAsync(int vendaId)
    {
        using var conn = _factory.CreateConnection();
        var contas = (await conn.QueryAsync<ContaReceberResumoVendaDto>(
            @"SELECT cr.id,
                     cr.descricao,
                     cr.data_vencimento AS DataVencimento,
                     cr.valor_original  AS ValorOriginal,
                     (SELECT COALESCE(SUM(crb.valor_recebido), 0)
                      FROM contas_receber_baixas crb
                      WHERE crb.conta_receber_id = cr.id) AS ValorRecebido,
                     cr.valor_saldo     AS ValorSaldo,
                     cr.status,
                     (SELECT MAX(crb.data_recebimento)
                      FROM contas_receber_baixas crb
                      WHERE crb.conta_receber_id = cr.id) AS DataUltimoRecebimento,
                     ve.motivo_cancelamento AS MotivoCancelamentoVenda
              FROM contas_receber cr
              LEFT JOIN vendas ve ON ve.id = cr.venda_id
              WHERE cr.venda_id = @vendaId
              ORDER BY cr.data_vencimento, cr.id", new { vendaId })).ToList();

        if (contas.Count == 0) return contas;

        var idsContas = contas.Select(c => c.Id).ToList();
        var todasBaixas = await conn.QueryAsync<ContaReceberBaixaDto>(
            @"SELECT id, conta_receber_id AS ContaReceberId,
                     data_recebimento AS DataRecebimento,
                     valor_recebido   AS ValorRecebido,
                     comprovante_arquivo AS ComprovanteArquivo,
                     observacao,
                     criado_em AS CriadoEm
              FROM contas_receber_baixas
              WHERE conta_receber_id IN @idsContas
              ORDER BY data_recebimento, id", new { idsContas });

        var baixasPorConta = todasBaixas.GroupBy(b => b.ContaReceberId)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var conta in contas)
            conta.Baixas = baixasPorConta.TryGetValue(conta.Id, out var lista) ? lista : new();

        return contas;
    }

    public async Task<decimal> ObterTotalRecebidoDaVendaAsync(int vendaId)
    {
        using var conn = _factory.CreateConnection();
        return await conn.ExecuteScalarAsync<decimal>(
            @"SELECT COALESCE(SUM(crb.valor_recebido), 0)
              FROM contas_receber_baixas crb
              INNER JOIN contas_receber cr ON cr.id = crb.conta_receber_id
              WHERE cr.venda_id = @vendaId", new { vendaId });
    }

    public async Task<List<ContaReceberRecebidoVendaDto>> ObterRecebidoPorContaDaVendaAsync(int vendaId, IDbTransaction tx)
    {
        var result = await tx.Connection!.QueryAsync<ContaReceberRecebidoVendaDto>(
            @"SELECT cr.id,
                     cr.cliente_id AS ClienteId,
                     (SELECT COALESCE(SUM(crb.valor_recebido), 0)
                      FROM contas_receber_baixas crb
                      WHERE crb.conta_receber_id = cr.id) AS ValorRecebido
              FROM contas_receber cr
              WHERE cr.venda_id = @vendaId
              ORDER BY cr.data_vencimento, cr.id", new { vendaId }, tx);
        return result.ToList();
    }

    public async Task CancelarPorVendaAsync(int vendaId, IDbTransaction tx)
    {
        await tx.Connection!.ExecuteAsync(
            @"UPDATE contas_receber
              SET status        = 'CANCELADA',
                  valor_saldo   = 0,
                  atualizado_em = NOW()
              WHERE venda_id = @vendaId",
            new { vendaId }, tx);
    }
}