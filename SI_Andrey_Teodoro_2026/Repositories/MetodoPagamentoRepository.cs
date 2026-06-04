using Dapper;
using SI_Andrey_Teodoro_2026.Data;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Models;
using SI_Andrey_Teodoro_2026.Repositories.Interfaces;
namespace SI_Andrey_Teodoro_2026.Repositories;
public class MetodoPagamentoRepository : BaseRepository, IMetodoPagamentoRepository
{
    public MetodoPagamentoRepository(DbConnectionFactory factory) : base(factory) { }
    protected override string Tabela => "metodos_pagamento";
    public async Task<PaginacaoDto<MetodoPagamentoListDto>> ObterTodosAsync(FiltroConsultaDto filtro)
    {
        using var conn = _factory.CreateConnection();
        var where = new List<string>();
        if (!string.IsNullOrWhiteSpace(filtro.Busca))
            where.Add("(m.metodo_pagamento LIKE @Busca OR m.codigo LIKE @Busca OR CAST(m.id AS CHAR) = @BuscaExata)");
        where.Add(filtro.StatusFiltro switch
        {
            "ativos" => "m.ativo = TRUE",
            "inativos" => "m.ativo = FALSE",
            _ => "1=1"
        });
        var whereClause = "WHERE " + string.Join(" AND ", where);
        var orderBy = filtro.OrdenarPor switch
        {
            "id" => "m.id",
            "data" => "m.criado_em",
            "codigo" => "m.codigo",
            _ => "m.metodo_pagamento"
        };
        var sqlCount = $"SELECT COUNT(*) FROM metodos_pagamento m {whereClause}";
        var sqlData = $@"SELECT m.id,
                                  m.codigo,
                                  m.metodo_pagamento AS MetodoPagamento,
                                  m.ativo,
                                  m.criado_em AS CriadoEm
                          FROM metodos_pagamento m
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
        var itens = await conn.QueryAsync<MetodoPagamentoListDto>(sqlData, param);
        return new PaginacaoDto<MetodoPagamentoListDto>
        {
            Itens = itens.ToList(),
            TotalItens = total,
            Pagina = filtro.Pagina,
            TamanhoPagina = filtro.TamanhoPagina
        };
    }
    public async Task<IEnumerable<MetodoPagamentoListDto>> ObterTodosAtivosAsync()
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryAsync<MetodoPagamentoListDto>(
            "SELECT id, codigo, metodo_pagamento AS MetodoPagamento FROM metodos_pagamento WHERE ativo = TRUE ORDER BY metodo_pagamento");
    }
    public async Task<MetodoPagamento?> ObterPorIdAsync(int id)
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<MetodoPagamento>(
            @"SELECT m.id,
                     m.codigo,
                     m.metodo_pagamento AS NomeMetodoPagamento,
                     m.ativo,
                     m.criado_em     AS CriadoEm,
                     m.atualizado_em AS AtualizadoEm,
                     ua.nome         AS NomeAtualizadoPor
              FROM metodos_pagamento m
              LEFT JOIN usuarios ua ON ua.id = m.atualizado_por
              WHERE m.id = @id", new { id });
    }
    public async Task<int> InserirAsync(MetodoPagamentoDto dto)
    {
        using var conn = _factory.CreateConnection();
        var proximoId = await ProximoIdAsync();
        await conn.ExecuteAsync(
            @"INSERT INTO metodos_pagamento (id, codigo, metodo_pagamento, ativo)
              VALUES (@ProximoId, @Codigo, @MetodoPagamento, @Ativo)",
            new { ProximoId = proximoId, dto.Codigo, dto.MetodoPagamento, dto.Ativo });
        return proximoId;
    }
    public async Task AtualizarAsync(MetodoPagamentoDto dto)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            @"UPDATE metodos_pagamento
              SET id               = @Id,
                  codigo           = @Codigo,
                  metodo_pagamento = @MetodoPagamento,
                  atualizado_em    = NOW()
              WHERE id = @IdOriginal", dto);
    }
    public Task AlterarStatusAsync(int id, bool ativo)
        => AlterarStatusBaseAsync(id, ativo);
    public async Task<bool> ExisteCodigoAsync(string codigo, int? idOriginalIgnorar = null)
    {
        using var conn = _factory.CreateConnection();
        var sql = idOriginalIgnorar.HasValue
            ? "SELECT COUNT(*) FROM metodos_pagamento WHERE codigo = @codigo AND id <> @idOriginalIgnorar"
            : "SELECT COUNT(*) FROM metodos_pagamento WHERE codigo = @codigo";
        return await conn.ExecuteScalarAsync<int>(sql, new { codigo, idOriginalIgnorar }) > 0;
    }
    public async Task<bool> ExisteNomeAsync(string nome, int? idOriginalIgnorar = null)
    {
        using var conn = _factory.CreateConnection();
        var sql = idOriginalIgnorar.HasValue
            ? "SELECT COUNT(*) FROM metodos_pagamento WHERE metodo_pagamento = @nome AND id <> @idOriginalIgnorar"
            : "SELECT COUNT(*) FROM metodos_pagamento WHERE metodo_pagamento = @nome";
        return await conn.ExecuteScalarAsync<int>(sql, new { nome, idOriginalIgnorar }) > 0;
    }
}