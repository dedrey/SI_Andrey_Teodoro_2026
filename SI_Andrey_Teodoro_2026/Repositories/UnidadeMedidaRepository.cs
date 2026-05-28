using Dapper;
using SI_Andrey_Teodoro_2026.Data;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Models;
using SI_Andrey_Teodoro_2026.Repositories.Interfaces;

namespace SI_Andrey_Teodoro_2026.Repositories;

public class UnidadeMedidaRepository : IUnidadeMedidaRepository
{
    private readonly DbConnectionFactory _factory;
    public UnidadeMedidaRepository(DbConnectionFactory factory) => _factory = factory;

    public async Task<PaginacaoDto<UnidadeMedidaListDto>> ObterTodosAsync(FiltroConsultaDto filtro)
    {
        using var conn = _factory.CreateConnection();

        var where = new List<string>();
        if (!string.IsNullOrWhiteSpace(filtro.Busca))
            where.Add("(u.unidade_medida LIKE @Busca OR u.descricao LIKE @Busca OR CAST(u.id AS CHAR) = @BuscaExata)");
        where.Add(filtro.StatusFiltro switch
        {
            "ativos" => "u.ativo = TRUE",
            "inativos" => "u.ativo = FALSE",
            _ => "1=1"
        });
        var whereClause = "WHERE " + string.Join(" AND ", where);
        var orderBy = filtro.OrdenarPor switch
        {
            "id" => "u.id",
            "data" => "u.criado_em",
            _ => "u.unidade_medida"
        };

        var sqlCount = $"SELECT COUNT(*) FROM unidades_medida u {whereClause}";
        var sqlData = $@"SELECT u.id,
                                 u.unidade_medida AS Sigla,
                                 u.descricao      AS Descricao,
                                 u.ativo,
                                 u.criado_em AS CriadoEm
                          FROM unidades_medida u
                          {whereClause}
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
        var itens = await conn.QueryAsync<UnidadeMedidaListDto>(sqlData, param);

        return new PaginacaoDto<UnidadeMedidaListDto>
        {
            Itens = itens.ToList(),
            TotalItens = total,
            Pagina = filtro.Pagina,
            TamanhoPagina = filtro.TamanhoPagina
        };
    }

    public async Task<IEnumerable<UnidadeMedidaListDto>> ObterTodosAtivosAsync()
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryAsync<UnidadeMedidaListDto>(
            @"SELECT id,
                     unidade_medida AS Sigla,
                     descricao      AS Descricao
              FROM unidades_medida
              WHERE ativo = TRUE
              ORDER BY unidade_medida");
    }

    public async Task<UnidadeMedida?> ObterPorIdAsync(int id)
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<UnidadeMedida>(
            @"SELECT u.id,
                     u.unidade_medida AS Sigla,
                     u.descricao      AS Descricao,
                     u.ativo,
                     u.criado_em     AS CriadoEm,
                     u.atualizado_em AS AtualizadoEm,
                     ua.nome         AS NomeAtualizadoPor
              FROM unidades_medida u
              LEFT JOIN usuarios ua ON ua.id = u.atualizado_por
              WHERE u.id = @id",
            new { id });
    }

    public async Task<int> InserirAsync(UnidadeMedidaDto dto)
    {
        using var conn = _factory.CreateConnection();

        var proximoId = await conn.ExecuteScalarAsync<int>(
            @"SELECT MIN(seq)
              FROM (SELECT 1 AS seq UNION ALL SELECT id + 1 FROM unidades_medida) t
              WHERE seq NOT IN (SELECT id FROM unidades_medida)");

        await conn.ExecuteAsync(
            @"INSERT INTO unidades_medida (id, unidade_medida, descricao, ativo)
              VALUES (@ProximoId, @Sigla, @Descricao, @Ativo)",
            new { ProximoId = proximoId, dto.Sigla, dto.Descricao, dto.Ativo });

        return proximoId;
    }

    public async Task AtualizarAsync(UnidadeMedidaDto dto)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            @"UPDATE unidades_medida
              SET id             = @Id,
                  unidade_medida = @Sigla,
                  descricao      = @Descricao,
                  atualizado_em  = NOW()
              WHERE id = @IdOriginal", dto);
    }

    public async Task AlterarStatusAsync(int id, bool ativo)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE unidades_medida SET ativo = @ativo, atualizado_em = NOW() WHERE id = @id",
            new { ativo, id });
    }

    public async Task<bool> ExisteSiglaAsync(string sigla, int? idOriginalIgnorar = null)
    {
        using var conn = _factory.CreateConnection();
        var sql = idOriginalIgnorar.HasValue
            ? "SELECT COUNT(*) FROM unidades_medida WHERE unidade_medida = @sigla AND id <> @idOriginalIgnorar"
            : "SELECT COUNT(*) FROM unidades_medida WHERE unidade_medida = @sigla";
        return await conn.ExecuteScalarAsync<int>(sql, new { sigla, idOriginalIgnorar }) > 0;
    }
}