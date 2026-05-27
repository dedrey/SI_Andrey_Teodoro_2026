using Dapper;
using SI_Andrey_Teodoro_2026.Data;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Models;
using SI_Andrey_Teodoro_2026.Repositories.Interfaces;

namespace SI_Andrey_Teodoro_2026.Repositories;

public class MarcaRepository : IMarcaRepository
{
    private readonly DbConnectionFactory _factory;
    public MarcaRepository(DbConnectionFactory factory) => _factory = factory;

    public async Task<PaginacaoDto<MarcaListDto>> ObterTodosAsync(FiltroConsultaDto filtro)
    {
        using var conn = _factory.CreateConnection();

        var where = new List<string>();
        if (!string.IsNullOrWhiteSpace(filtro.Busca))
            where.Add("(m.marca LIKE @Busca OR CAST(m.id AS CHAR) = @BuscaExata)");
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
            _ => "m.marca"
        };

        var sqlCount = $"SELECT COUNT(*) FROM marcas m {whereClause}";
        var sqlData = $@"SELECT m.id,
                                 m.marca AS NomeMarca,
                                 m.ativo,
                                 m.criado_em AS CriadoEm
                          FROM marcas m
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
        var itens = await conn.QueryAsync<MarcaListDto>(sqlData, param);

        return new PaginacaoDto<MarcaListDto>
        {
            Itens = itens.ToList(),
            TotalItens = total,
            Pagina = filtro.Pagina,
            TamanhoPagina = filtro.TamanhoPagina
        };
    }

    public async Task<IEnumerable<MarcaListDto>> ObterTodosAtivosAsync()
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryAsync<MarcaListDto>(
            "SELECT id, marca AS NomeMarca FROM marcas WHERE ativo = TRUE ORDER BY marca");
    }

    public async Task<Marca?> ObterPorIdAsync(int id)
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Marca>(
            @"SELECT m.id,
                     m.marca AS NomeMarca,
                     m.ativo,
                     m.criado_em     AS CriadoEm,
                     m.atualizado_em AS AtualizadoEm,
                     ua.nome         AS NomeAtualizadoPor
              FROM marcas m
              LEFT JOIN usuarios ua ON ua.id = m.atualizado_por
              WHERE m.id = @id",
            new { id });
    }

    public async Task<int> InserirAsync(MarcaDto dto)
    {
        using var conn = _factory.CreateConnection();

        // Calcula o menor ID disponível na sequência (preenche buracos).
        var proximoId = await conn.ExecuteScalarAsync<int>(
            @"SELECT MIN(seq)
              FROM (SELECT 1 AS seq UNION ALL SELECT id + 1 FROM marcas) t
              WHERE seq NOT IN (SELECT id FROM marcas)");

        await conn.ExecuteAsync(
            @"INSERT INTO marcas (id, marca, ativo)
              VALUES (@ProximoId, @NomeMarca, @Ativo)",
            new { ProximoId = proximoId, dto.NomeMarca, dto.Ativo });

        return proximoId;
    }

    public async Task AtualizarAsync(MarcaDto dto)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            @"UPDATE marcas
              SET id            = @Id,
                  marca         = @NomeMarca,
                  atualizado_em = NOW()
              WHERE id = @IdOriginal", dto);
    }

    public async Task AlterarStatusAsync(int id, bool ativo)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE marcas SET ativo = @ativo, atualizado_em = NOW() WHERE id = @id",
            new { ativo, id });
    }

    public async Task<bool> ExisteNomeAsync(string nome, int? idOriginalIgnorar = null)
    {
        using var conn = _factory.CreateConnection();
        var sql = idOriginalIgnorar.HasValue
            ? "SELECT COUNT(*) FROM marcas WHERE marca = @nome AND id <> @idOriginalIgnorar"
            : "SELECT COUNT(*) FROM marcas WHERE marca = @nome";
        return await conn.ExecuteScalarAsync<int>(sql, new { nome, idOriginalIgnorar }) > 0;
    }
}