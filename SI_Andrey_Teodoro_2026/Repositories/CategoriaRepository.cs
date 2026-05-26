using Dapper;
using SI_Andrey_Teodoro_2026.Data;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Models;
using SI_Andrey_Teodoro_2026.Repositories.Interfaces;

namespace SI_Andrey_Teodoro_2026.Repositories;

public class CategoriaRepository : ICategoriaRepository
{
    private readonly DbConnectionFactory _factory;
    public CategoriaRepository(DbConnectionFactory factory) => _factory = factory;

    public async Task<PaginacaoDto<CategoriaListDto>> ObterTodosAsync(FiltroConsultaDto filtro)
    {
        using var conn = _factory.CreateConnection();
        var where = new List<string>();
        if (!string.IsNullOrWhiteSpace(filtro.Busca))
            where.Add("(c.categoria LIKE @Busca OR CAST(c.id AS CHAR) = @BuscaExata)");
        where.Add(filtro.StatusFiltro switch { "ativos" => "c.ativo = TRUE", "inativos" => "c.ativo = FALSE", _ => "1=1" });
        var whereClause = "WHERE " + string.Join(" AND ", where);
        var orderBy = filtro.OrdenarPor switch { "id" => "c.id", "data" => "c.criado_em", _ => "c.categoria" };

        var sqlCount = $"SELECT COUNT(*) FROM categorias c {whereClause}";
        var sqlData = $@"SELECT c.id, c.categoria AS NomeCategoria, c.ativo, c.criado_em AS CriadoEm
                          FROM categorias c {whereClause}
                          ORDER BY {orderBy} LIMIT @Limit OFFSET @Offset";

        var param = new
        {
            Busca = $"%{filtro.Busca}%",
            BuscaExata = filtro.Busca,
            Limit = filtro.TamanhoPagina,
            Offset = (filtro.Pagina - 1) * filtro.TamanhoPagina
        };
        var total = await conn.ExecuteScalarAsync<int>(sqlCount, param);
        var itens = await conn.QueryAsync<CategoriaListDto>(sqlData, param);
        return new PaginacaoDto<CategoriaListDto>
        {
            Itens = itens.ToList(),
            TotalItens = total,
            Pagina = filtro.Pagina,
            TamanhoPagina = filtro.TamanhoPagina
        };
    }

    public async Task<IEnumerable<CategoriaListDto>> ObterTodosAtivosAsync()
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryAsync<CategoriaListDto>(
            "SELECT id, categoria AS NomeCategoria FROM categorias WHERE ativo = TRUE ORDER BY categoria");
    }

    public async Task<Categoria?> ObterPorIdAsync(int id)
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Categoria>(
            @"SELECT c.id, c.categoria AS NomeCategoria, c.ativo,
                     c.criado_em AS CriadoEm, c.atualizado_em AS AtualizadoEm,
                     ua.nome AS NomeAtualizadoPor
              FROM categorias c
              LEFT JOIN usuarios ua ON ua.id = c.atualizado_por
              WHERE c.id = @id", new { id });
    }

    public async Task<int> InserirAsync(CategoriaDto dto)
    {
        using var conn = _factory.CreateConnection();
        var proximoId = await conn.ExecuteScalarAsync<int>(
            @"SELECT MIN(seq)
              FROM (SELECT 1 AS seq UNION ALL SELECT id + 1 FROM categorias) t
              WHERE seq NOT IN (SELECT id FROM categorias)");

        await conn.ExecuteAsync(
            @"INSERT INTO categorias (id, categoria, ativo)
              VALUES (@ProximoId, @NomeCategoria, @Ativo)",
            new { ProximoId = proximoId, dto.NomeCategoria, dto.Ativo });

        return proximoId;
    }

    public async Task AtualizarAsync(CategoriaDto dto)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            @"UPDATE categorias
              SET id            = @Id,
                  categoria     = @NomeCategoria,
                  atualizado_em = NOW()
              WHERE id = @IdOriginal", dto);
    }

    public async Task AlterarStatusAsync(int id, bool ativo)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE categorias SET ativo = @ativo, atualizado_em = NOW() WHERE id = @id",
            new { ativo, id });
    }

    public async Task<bool> ExisteNomeAsync(string nome, int? idOriginalIgnorar = null)
    {
        using var conn = _factory.CreateConnection();
        var sql = idOriginalIgnorar.HasValue
            ? "SELECT COUNT(*) FROM categorias WHERE categoria = @nome AND id <> @idOriginalIgnorar"
            : "SELECT COUNT(*) FROM categorias WHERE categoria = @nome";
        return await conn.ExecuteScalarAsync<int>(sql, new { nome, idOriginalIgnorar }) > 0;
    }
}