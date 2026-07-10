using Dapper;
using SI_Andrey_Teodoro_2026.Data;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Models;
using SI_Andrey_Teodoro_2026.Repositories.Interfaces;

namespace SI_Andrey_Teodoro_2026.Repositories;

public class CondicaoPagamentoRepository : BaseRepository, ICondicaoPagamentoRepository
{
    public CondicaoPagamentoRepository(DbConnectionFactory factory) : base(factory) { }

    protected override string Tabela => "condicoes_pagamentos";

    public async Task<PaginacaoDto<CondicaoPagamentoListDto>> ObterTodosAsync(FiltroConsultaDto filtro)
    {
        using var conn = _factory.CreateConnection();
        var where = new List<string>();
        if (!string.IsNullOrWhiteSpace(filtro.Busca))
            where.Add(@"(c.condicao_pagamento LIKE @Busca
                      OR m.metodo_pagamento   LIKE @Busca
                      OR m.codigo             LIKE @Busca
                      OR CAST(c.id AS CHAR)  = @BuscaExata)");
        where.Add(filtro.StatusFiltro switch
        {
            "ativos" => "c.ativo = TRUE",
            "inativos" => "c.ativo = FALSE",
            _ => "1=1"
        });
        var whereClause = "WHERE " + string.Join(" AND ", where);
        var orderBy = filtro.OrdenarPor switch
        {
            "id" => "c.id",
            "data" => "c.criado_em",
            "metodo" => "m.metodo_pagamento",
            _ => "c.condicao_pagamento"
        };

        var sqlCount = $@"SELECT COUNT(*) FROM condicoes_pagamentos c
                          INNER JOIN metodos_pagamento m ON m.id = c.metodo_pagamento_id
                          {whereClause}";
        var sqlData = $@"SELECT c.id,
                                 c.condicao_pagamento        AS CondicaoPagamento,
                                 c.metodo_pagamento_id       AS MetodoPagamentoId,
                                 m.metodo_pagamento          AS NomeMetodoPagamento,
                                 m.codigo                    AS CodigoMetodo,
                                 c.numero_parcelas           AS NumeroParcelas,
                                 c.entrada_minima_percentual AS EntradaMinimaPercentual,
                                 c.desconto_percentual       AS DescontoPercentual,
                                 c.acrescimo_percentual      AS AcrescimoPercentual,
                                 c.multa_percentual          AS MultaPercentual,
                                 c.taxa_juros_percentual     AS TaxaJurosPercentual,
                                 c.ativo, c.criado_em AS CriadoEm
                          FROM condicoes_pagamentos c
                          INNER JOIN metodos_pagamento m ON m.id = c.metodo_pagamento_id
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
        var itens = await conn.QueryAsync<CondicaoPagamentoListDto>(sqlData, param);
        return new PaginacaoDto<CondicaoPagamentoListDto>
        { Itens = itens.ToList(), TotalItens = total, Pagina = filtro.Pagina, TamanhoPagina = filtro.TamanhoPagina };
    }

    public async Task<IEnumerable<CondicaoPagamentoListDto>> ObterTodosAtivosAsync()
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryAsync<CondicaoPagamentoListDto>(
            @"SELECT c.id, c.condicao_pagamento AS CondicaoPagamento,
                     c.metodo_pagamento_id AS MetodoPagamentoId,
                     m.metodo_pagamento AS NomeMetodoPagamento, m.codigo AS CodigoMetodo,
                     c.numero_parcelas AS NumeroParcelas,
                     c.entrada_minima_percentual AS EntradaMinimaPercentual,
                     c.desconto_percentual AS DescontoPercentual,
                     c.acrescimo_percentual AS AcrescimoPercentual,
                     c.multa_percentual AS MultaPercentual,
                     c.taxa_juros_percentual AS TaxaJurosPercentual, c.ativo
              FROM condicoes_pagamentos c
              INNER JOIN metodos_pagamento m ON m.id = c.metodo_pagamento_id
              WHERE c.ativo = TRUE ORDER BY c.condicao_pagamento");
    }

    public async Task<CondicaoPagamento?> ObterPorIdAsync(int id)
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<CondicaoPagamento>(
            @"SELECT c.id,
                     c.condicao_pagamento        AS NomeCondicaoPagamento,
                     c.metodo_pagamento_id       AS MetodoPagamentoId,
                     m.metodo_pagamento          AS NomeMetodoPagamento,
                     m.codigo                    AS CodigoMetodo,
                     c.numero_parcelas           AS NumeroParcelas,
                     c.entrada_minima_percentual AS EntradaMinimaPercentual,
                     c.desconto_percentual       AS DescontoPercentual,
                     c.acrescimo_percentual      AS AcrescimoPercentual,
                     c.multa_percentual          AS MultaPercentual,
                     c.taxa_juros_percentual     AS TaxaJurosPercentual,
                     c.ativo, c.criado_em AS CriadoEm,
                     c.atualizado_em AS AtualizadoEm, ua.nome AS NomeAtualizadoPor
              FROM condicoes_pagamentos c
              INNER JOIN metodos_pagamento m  ON m.id  = c.metodo_pagamento_id
              LEFT  JOIN usuarios         ua ON ua.id = c.atualizado_por
              WHERE c.id = @id", new { id });
    }

    // retorna parcelas individuais de uma condição
    public async Task<List<CondicaoPagamentoParcelaDto>> ObterParcelasAsync(int condicaoId)
    {
        using var conn = _factory.CreateConnection();
        var result = await conn.QueryAsync<CondicaoPagamentoParcelaDto>(
            @"SELECT id, numero_parcela AS NumeroParcela, dias_vencimento AS DiasVencimento
              FROM condicoes_pagamentos_parcelas
              WHERE condicao_pagamento_id = @condicaoId
              ORDER BY numero_parcela", new { condicaoId });
        return result.ToList();
    }

    // remove e reinserção das parcelas (simples e confiável)
    private async Task SalvarParcelasAsync(int condicaoId, List<CondicaoPagamentoParcelaDto> parcelas,
        Dapper.SqlMapper.GridReader? _ = null)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            "DELETE FROM condicoes_pagamentos_parcelas WHERE condicao_pagamento_id = @condicaoId",
            new { condicaoId });
        foreach (var p in parcelas.OrderBy(x => x.NumeroParcela))
            await conn.ExecuteAsync(
                @"INSERT INTO condicoes_pagamentos_parcelas
                    (condicao_pagamento_id, numero_parcela, dias_vencimento)
                  VALUES (@condicaoId, @NumeroParcela, @DiasVencimento)",
                new { condicaoId, p.NumeroParcela, p.DiasVencimento });
    }

    public async Task<int> InserirAsync(CondicaoPagamentoDto dto)
    {
        using var conn = _factory.CreateConnection();
        var proximoId = await ProximoIdAsync();
        await conn.ExecuteAsync(
            @"INSERT INTO condicoes_pagamentos
                (id, condicao_pagamento, metodo_pagamento_id, numero_parcelas,
                 entrada_minima_percentual, desconto_percentual, acrescimo_percentual,
                 multa_percentual, taxa_juros_percentual, ativo)
              VALUES
                (@ProximoId, @CondicaoPagamento, @MetodoPagamentoId, @NumeroParcelas,
                 @EntradaMinimaPercentual, @DescontoPercentual, @AcrescimoPercentual,
                 @MultaPercentual, @TaxaJurosPercentual, @Ativo)",
            new
            {
                ProximoId = proximoId,
                dto.CondicaoPagamento,
                dto.MetodoPagamentoId,
                dto.NumeroParcelas,
                dto.EntradaMinimaPercentual,
                dto.DescontoPercentual,
                dto.AcrescimoPercentual,
                dto.MultaPercentual,
                dto.TaxaJurosPercentual,
                dto.Ativo
            });
        if (dto.Parcelas.Count > 0)
            await SalvarParcelasAsync(proximoId, dto.Parcelas);
        return proximoId;
    }

    public async Task AtualizarAsync(CondicaoPagamentoDto dto)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            @"UPDATE condicoes_pagamentos
              SET condicao_pagamento        = @CondicaoPagamento,
                  metodo_pagamento_id       = @MetodoPagamentoId,
                  numero_parcelas           = @NumeroParcelas,
                  entrada_minima_percentual = @EntradaMinimaPercentual,
                  desconto_percentual       = @DescontoPercentual,
                  acrescimo_percentual      = @AcrescimoPercentual,
                  multa_percentual          = @MultaPercentual,
                  taxa_juros_percentual     = @TaxaJurosPercentual,
                  atualizado_em             = NOW()
              WHERE id = @IdOriginal", dto);
        await SalvarParcelasAsync(dto.IdOriginal, dto.Parcelas);
    }

    public Task AlterarStatusAsync(int id, bool ativo) => AlterarStatusBaseAsync(id, ativo);

    public async Task<bool> ExisteNomeAsync(string nome, int? idOriginalIgnorar = null)
    {
        using var conn = _factory.CreateConnection();
        var sql = idOriginalIgnorar.HasValue
            ? "SELECT COUNT(*) FROM condicoes_pagamentos WHERE condicao_pagamento = @nome AND id <> @idOriginalIgnorar"
            : "SELECT COUNT(*) FROM condicoes_pagamentos WHERE condicao_pagamento = @nome";
        return await conn.ExecuteScalarAsync<int>(sql, new { nome, idOriginalIgnorar }) > 0;
    }
}