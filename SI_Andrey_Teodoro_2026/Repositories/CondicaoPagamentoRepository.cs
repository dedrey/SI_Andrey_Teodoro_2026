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
                      OR CAST(c.id AS CHAR) = @BuscaExata)");
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
                                  c.ativo,
                                  c.criado_em AS CriadoEm
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
        {
            Itens = itens.ToList(),
            TotalItens = total,
            Pagina = filtro.Pagina,
            TamanhoPagina = filtro.TamanhoPagina
        };
    }
    public async Task<IEnumerable<CondicaoPagamentoListDto>> ObterTodosAtivosAsync()
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryAsync<CondicaoPagamentoListDto>(
            @"SELECT c.id,
                     c.condicao_pagamento  AS CondicaoPagamento,
                     c.metodo_pagamento_id AS MetodoPagamentoId,
                     m.metodo_pagamento    AS NomeMetodoPagamento,
                     m.codigo              AS CodigoMetodo,
                     c.numero_parcelas     AS NumeroParcelas
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
                     c.ativo,
                     c.criado_em     AS CriadoEm,
                     c.atualizado_em AS AtualizadoEm,
                     ua.nome         AS NomeAtualizadoPor
              FROM condicoes_pagamentos c
              INNER JOIN metodos_pagamento m ON m.id  = c.metodo_pagamento_id
              LEFT  JOIN usuarios         ua ON ua.id = c.atualizado_por
              WHERE c.id = @id", new { id });
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
        return proximoId;
    }
    public async Task AtualizarAsync(CondicaoPagamentoDto dto)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            @"UPDATE condicoes_pagamentos
              SET id                        = @Id,
                  condicao_pagamento        = @CondicaoPagamento,
                  metodo_pagamento_id       = @MetodoPagamentoId,
                  numero_parcelas           = @NumeroParcelas,
                  entrada_minima_percentual = @EntradaMinimaPercentual,
                  desconto_percentual       = @DescontoPercentual,
                  acrescimo_percentual      = @AcrescimoPercentual,
                  multa_percentual          = @MultaPercentual,
                  taxa_juros_percentual     = @TaxaJurosPercentual,
                  atualizado_em             = NOW()
              WHERE id = @IdOriginal", dto);
    }
    public Task AlterarStatusAsync(int id, bool ativo)
        => AlterarStatusBaseAsync(id, ativo);
    public async Task<bool> ExisteNomeAsync(string nome, int? idOriginalIgnorar = null)
    {
        using var conn = _factory.CreateConnection();
        var sql = idOriginalIgnorar.HasValue
            ? "SELECT COUNT(*) FROM condicoes_pagamentos WHERE condicao_pagamento = @nome AND id <> @idOriginalIgnorar"
            : "SELECT COUNT(*) FROM condicoes_pagamentos WHERE condicao_pagamento = @nome";
        return await conn.ExecuteScalarAsync<int>(sql, new { nome, idOriginalIgnorar }) > 0;
    }
}