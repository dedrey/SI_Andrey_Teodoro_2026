using Dapper;
using SI_Andrey_Teodoro_2026.Data;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Models;
using SI_Andrey_Teodoro_2026.Repositories.Interfaces;

namespace SI_Andrey_Teodoro_2026.Repositories;

public class EstadoRepository : BaseRepository, IEstadoRepository
{
    public EstadoRepository(DbConnectionFactory factory) : base(factory) { }

    protected override string Tabela => "estados";

    public async Task<PaginacaoDto<EstadoListDto>> ObterTodosAsync(FiltroConsultaDto filtro)
    {
        using var conn = _factory.CreateConnection();
        var where = new List<string>();
        if (!string.IsNullOrWhiteSpace(filtro.Busca))
            where.Add("(e.estado LIKE @Busca OR e.uf LIKE @Busca OR p.pais LIKE @Busca OR CAST(e.id AS CHAR) = @BuscaExata)");
        where.Add(filtro.StatusFiltro switch { "ativos" => "e.ativo = TRUE", "inativos" => "e.ativo = FALSE", _ => "1=1" });
        var whereClause = "WHERE " + string.Join(" AND ", where);
        var orderBy = filtro.OrdenarPor switch { "id" => "e.id", "data" => "e.criado_em", _ => "e.estado" };

        var sqlCount = $"SELECT COUNT(*) FROM estados e INNER JOIN paises p ON p.id = e.pais_id {whereClause}";
        var sqlData = $@"SELECT e.id, e.pais_id AS PaisId, e.estado AS NomeEstado, e.uf,
                                 p.pais AS NomePais, e.ativo, e.criado_em AS CriadoEm
                          FROM estados e INNER JOIN paises p ON p.id = e.pais_id
                          {whereClause} ORDER BY {orderBy} LIMIT @Limit OFFSET @Offset";

        var param = new
        {
            Busca = $"%{filtro.Busca}%",
            BuscaExata = filtro.Busca,
            Limit = filtro.TamanhoPagina,
            Offset = (filtro.Pagina - 1) * filtro.TamanhoPagina
        };

        var total = await conn.ExecuteScalarAsync<int>(sqlCount, param);
        var itens = await conn.QueryAsync<EstadoListDto>(sqlData, param);
        return new PaginacaoDto<EstadoListDto>
        {
            Itens = itens.ToList(),
            TotalItens = total,
            Pagina = filtro.Pagina,
            TamanhoPagina = filtro.TamanhoPagina
        };
    }

    public async Task<IEnumerable<EstadoListDto>> ObterPorPaisAsync(int paisId)
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryAsync<EstadoListDto>(
            "SELECT e.id, e.estado AS NomeEstado, e.uf, e.pais_id AS PaisId FROM estados e WHERE e.pais_id = @paisId AND e.ativo = TRUE ORDER BY e.estado",
            new { paisId });
    }

    public async Task<IEnumerable<EstadoListDto>> ObterTodosAtivosAsync()
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryAsync<EstadoListDto>(
            @"SELECT e.id, e.estado AS NomeEstado, e.uf
              FROM estados e
              INNER JOIN paises p ON p.id = e.pais_id
              WHERE e.ativo = TRUE AND p.pais = 'Brasil'
              ORDER BY e.uf");
    }

    public async Task<Estado?> ObterPorIdAsync(int id)
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Estado>(
            @"SELECT e.id, e.pais_id AS PaisId, e.estado AS NomeEstado, e.uf,
                     p.pais AS NomePais, e.ativo,
                     e.criado_em AS CriadoEm, e.atualizado_em AS AtualizadoEm,
                     ua.nome AS NomeAtualizadoPor
              FROM estados e
              INNER JOIN paises   p  ON p.id  = e.pais_id
              LEFT  JOIN usuarios ua ON ua.id = e.atualizado_por
              WHERE e.id = @id", new { id });
    }

    public async Task<int> InserirAsync(EstadoDto dto)
    {
        using var conn = _factory.CreateConnection();
        var proximoId = await ProximoIdAsync();
        await conn.ExecuteAsync(
            @"INSERT INTO estados (id, pais_id, estado, uf, ativo)
              VALUES (@ProximoId, @PaisId, @NomeEstado, @Uf, @Ativo)",
            new { ProximoId = proximoId, dto.PaisId, dto.NomeEstado, dto.Uf, dto.Ativo });
        return proximoId;
    }

    public async Task AtualizarAsync(EstadoDto dto)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            @"UPDATE estados
              SET id            = @Id,
                  pais_id       = @PaisId,
                  estado        = @NomeEstado,
                  uf            = @Uf,
                  atualizado_em = NOW()
              WHERE id = @IdOriginal", dto);
    }

    public Task AlterarStatusAsync(int id, bool ativo)
        => AlterarStatusBaseAsync(id, ativo);

    public async Task<bool> ExisteUfNoPaisAsync(string uf, int paisId, int? idOriginalIgnorar = null)
    {
        using var conn = _factory.CreateConnection();
        var sql = idOriginalIgnorar.HasValue
            ? "SELECT COUNT(*) FROM estados WHERE uf = @uf AND pais_id = @paisId AND id <> @idOriginalIgnorar"
            : "SELECT COUNT(*) FROM estados WHERE uf = @uf AND pais_id = @paisId";
        return await conn.ExecuteScalarAsync<int>(sql, new { uf, paisId, idOriginalIgnorar }) > 0;
    }
}