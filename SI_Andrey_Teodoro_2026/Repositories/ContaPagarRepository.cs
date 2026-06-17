using Dapper;
using SI_Andrey_Teodoro_2026.Data;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Models;
using SI_Andrey_Teodoro_2026.Repositories.Interfaces;

namespace SI_Andrey_Teodoro_2026.Repositories;

public class ContaPagarRepository : BaseRepository, IContaPagarRepository
{
    public ContaPagarRepository(DbConnectionFactory factory) : base(factory) { }

    protected override string Tabela => "contas_pagar";

    public async Task<PaginacaoDto<ContaPagarListDto>> ObterTodosAsync(FiltroConsultaDto filtro)
    {
        using var conn = _factory.CreateConnection();
        var where = new List<string>();
        if (!string.IsNullOrWhiteSpace(filtro.Busca))
            where.Add(@"(cp.descricao   LIKE @Busca
                      OR f.razaosocial  LIKE @Busca
                      OR me.numero_nf   LIKE @Busca
                      OR CAST(cp.id AS CHAR) = @BuscaExata)");
        where.Add(filtro.StatusFiltro switch
        {
            "aberta" => "cp.status = 'ABERTA'",
            "paga" => "cp.status = 'PAGA'",
            "cancelada" => "cp.status = 'CANCELADA'",
            "vencidas" => "cp.status = 'ABERTA' AND cp.data_vencimento < CURDATE()",
            _ => "1=1"
        });
        var whereClause = "WHERE " + string.Join(" AND ", where);
        var orderBy = filtro.OrdenarPor switch
        {
            "id" => "cp.id",
            "valor" => "cp.valor_original DESC",
            "vencimento" => "cp.data_vencimento",
            _ => "cp.data_vencimento"
        };

        var sqlCount = $@"SELECT COUNT(*) FROM contas_pagar cp
                          LEFT JOIN fornecedores         f  ON f.id  = cp.fornecedor_id
                          LEFT JOIN movimentacoes_estoque me ON me.id = cp.movimentacao_id
                          {whereClause}";

        var sqlData = $@"SELECT cp.id,
                                 f.razaosocial      AS NomeFornecedor,
                                 cp.movimentacao_id  AS MovimentacaoId,
                                 me.numero_nf        AS NumeroNfMovimentacao,
                                 cp.descricao,
                                 cp.data_vencimento   AS DataVencimento,
                                 cp.data_pagamento    AS DataPagamento,
                                 cp.comprovante_arquivo AS ComprovanteArquivo,
                                 cp.valor_original    AS ValorOriginal,
                                 cp.status,
                                 cp.criado_em         AS CriadoEm
                          FROM contas_pagar cp
                          LEFT JOIN fornecedores          f  ON f.id  = cp.fornecedor_id
                          LEFT JOIN movimentacoes_estoque me ON me.id = cp.movimentacao_id
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
        var itens = await conn.QueryAsync<ContaPagarListDto>(sqlData, param);
        return new PaginacaoDto<ContaPagarListDto>
        { Itens = itens.ToList(), TotalItens = total, Pagina = filtro.Pagina, TamanhoPagina = filtro.TamanhoPagina };
    }

    public async Task<ContaPagar?> ObterPorIdAsync(int id)
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<ContaPagar>(
            @"SELECT cp.id,
                     cp.fornecedor_id    AS FornecedorId,
                     f.razaosocial       AS NomeFornecedor,
                     cp.movimentacao_id  AS MovimentacaoId,
                     me.numero_nf        AS NumeroNfMovimentacao,
                     cp.descricao,
                     cp.data_vencimento  AS DataVencimento,
                     cp.data_pagamento   AS DataPagamento,
                     cp.comprovante_arquivo AS ComprovanteArquivo,
                     cp.valor_original   AS ValorOriginal,
                     cp.valor_saldo      AS ValorSaldo,
                     cp.status,
                     cp.criado_em        AS CriadoEm,
                     cp.criado_por       AS CriadoPor,
                     cp.atualizado_em    AS AtualizadoEm,
                     ua.nome             AS NomeAtualizadoPor
              FROM contas_pagar cp
              LEFT JOIN fornecedores          f  ON f.id  = cp.fornecedor_id
              LEFT JOIN movimentacoes_estoque me ON me.id = cp.movimentacao_id
              LEFT JOIN usuarios              ua ON ua.id = cp.atualizado_por
              WHERE cp.id = @id", new { id });
    }

    public async Task<int> InserirAsync(ContaPagarDto dto)
    {
        using var conn = _factory.CreateConnection();
        var proximoId = await ProximoIdAsync();
        await conn.ExecuteAsync(
            @"INSERT INTO contas_pagar
                (id, fornecedor_id, movimentacao_id, descricao, data_vencimento, data_pagamento,
                 valor_original, valor_saldo, status)
              VALUES
                (@ProximoId, @FornecedorId, @MovimentacaoId, @Descricao, @DataVencimento, NULL,
                 @ValorOriginal, @ValorOriginal, 'ABERTA')",
            new
            {
                ProximoId = proximoId,
                dto.FornecedorId,
                dto.MovimentacaoId,
                dto.Descricao,
                dto.DataVencimento,
                dto.ValorOriginal
            });
        return proximoId;
    }

    public async Task AtualizarAsync(ContaPagarDto dto)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            @"UPDATE contas_pagar
              SET fornecedor_id   = @FornecedorId,
                  descricao       = @Descricao,
                  data_vencimento = @DataVencimento,
                  valor_original  = @ValorOriginal,
                  valor_saldo     = CASE WHEN status = 'ABERTA' THEN @ValorOriginal ELSE valor_saldo END,
                  atualizado_em   = NOW()
              WHERE id = @IdOriginal",
            new
            {
                dto.IdOriginal,
                dto.FornecedorId,
                dto.Descricao,
                dto.DataVencimento,
                dto.ValorOriginal
            });
    }

    public async Task AtualizarStatusAsync(int id, string status, DateTime? dataPagamento = null, string? comprovanteArquivo = null)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            @"UPDATE contas_pagar
              SET status = @status,
                  data_pagamento = @dataPagamento,
                  comprovante_arquivo = COALESCE(@comprovanteArquivo, comprovante_arquivo),
                  valor_saldo = CASE WHEN @status IN ('PAGA','CANCELADA') THEN 0 ELSE valor_original END,
                  atualizado_em = NOW()
              WHERE id = @id",
            new { id, status, dataPagamento, comprovanteArquivo });
    }

    public async Task<int> InserirAutomaticaAsync(int? fornecedorId, int movimentacaoId, string descricao,
        DateTime dataVencimento, decimal valorOriginal)
    {
        using var conn = _factory.CreateConnection();
        var proximoId = await ProximoIdAsync();
        await conn.ExecuteAsync(
            @"INSERT INTO contas_pagar
                (id, fornecedor_id, movimentacao_id, descricao, data_vencimento, data_pagamento,
                 valor_original, valor_saldo, status)
              VALUES
                (@ProximoId, @fornecedorId, @movimentacaoId, @descricao, @dataVencimento, NULL,
                 @valorOriginal, @valorOriginal, 'ABERTA')",
            new { ProximoId = proximoId, fornecedorId, movimentacaoId, descricao, dataVencimento, valorOriginal });
        return proximoId;
    }
}