using Dapper;
using SI_Andrey_Teodoro_2026.Data;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Models;
using SI_Andrey_Teodoro_2026.Repositories.Interfaces;

namespace SI_Andrey_Teodoro_2026.Repositories;

public class PaisRepository : IPaisRepository
{
    private readonly DbConnectionFactory _factory;
    public PaisRepository(DbConnectionFactory factory) => _factory = factory;

    public async Task<PaginacaoDto<PaisListDto>> ObterTodosAsync(FiltroConsultaDto filtro)
    {
        using var conn = _factory.CreateConnection();
        var where = new List<string>();

        if (!string.IsNullOrWhiteSpace(filtro.Busca))
            where.Add("(p.pais LIKE @Busca OR p.sigla LIKE @Busca OR p.ddi LIKE @Busca OR CAST(p.id AS CHAR) = @BuscaExata)");

        where.Add(filtro.StatusFiltro switch
        {
            "ativos" => "p.ativo = TRUE",
            "inativos" => "p.ativo = FALSE",
            _ => "1=1"
        });

        var whereClause = where.Count > 0 ? "WHERE " + string.Join(" AND ", where) : "";
        var orderBy = filtro.OrdenarPor switch
        {
            "id" => "p.id",
            "data" => "p.criado_em",
            _ => "p.pais"
        };

        var sqlCount = $"SELECT COUNT(*) FROM paises p {whereClause}";
        var sqlData = $@"SELECT p.id, p.ddi, p.sigla, p.moeda,
                                 p.simbolo_moeda AS SimboleMoeda,
                                 p.pais AS NomePais, p.ativo,
                                 p.criado_em AS CriadoEm,
                                 p.atualizado_em AS AtualizadoEm
                          FROM paises p {whereClause}
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
        var itens = await conn.QueryAsync<PaisListDto>(sqlData, param);
        return new PaginacaoDto<PaisListDto>
        {
            Itens = itens.ToList(),
            TotalItens = total,
            Pagina = filtro.Pagina,
            TamanhoPagina = filtro.TamanhoPagina
        };
    }

    public async Task<IEnumerable<PaisListDto>> ObterTodosAtivosSemPaginacaoAsync()
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryAsync<PaisListDto>(
            "SELECT id, pais AS NomePais, sigla, ddi FROM paises WHERE ativo = TRUE ORDER BY pais");
    }

    public async Task<Pais?> ObterPorIdAsync(int id)
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Pais>(
            @"SELECT id, ddi, sigla, moeda, simbolo_moeda AS SimboleMoeda,
                     pais AS NomePais, ativo,
                     criado_em AS CriadoEm, atualizado_em AS AtualizadoEm
              FROM paises WHERE id = @id", new { id });
    }

    public async Task<int> InserirAsync(PaisDto dto)
    {
        using var conn = _factory.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            @"INSERT INTO paises (ddi, sigla, moeda, simbolo_moeda, pais, ativo)
              VALUES (@Ddi, @Sigla, @Moeda, @SimboleMoeda, @NomePais, @Ativo);
              SELECT LAST_INSERT_ID();", dto);
    }

    public async Task AtualizarAsync(PaisDto dto)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            @"UPDATE paises SET id = @Id, ddi = @Ddi, sigla = @Sigla, moeda = @Moeda,
                                simbolo_moeda = @SimboleMoeda, pais = @NomePais,
                                atualizado_em = NOW()
              WHERE id = @IdOriginal", dto);
        await conn.ExecuteAsync("ALTER TABLE paises AUTO_INCREMENT = 1");
    }

    public async Task AlterarStatusAsync(int id, bool ativo)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE paises SET ativo = @ativo, atualizado_em = NOW() WHERE id = @id",
            new { ativo, id });
    }

    public async Task<bool> ExisteSiglaAsync(string sigla, int? idOriginalIgnorar = null)
    {
        using var conn = _factory.CreateConnection();
        var sql = idOriginalIgnorar.HasValue
            ? "SELECT COUNT(*) FROM paises WHERE sigla = @sigla AND id <> @idOriginalIgnorar"
            : "SELECT COUNT(*) FROM paises WHERE sigla = @sigla";
        return await conn.ExecuteScalarAsync<int>(sql, new { sigla, idOriginalIgnorar }) > 0;
    }

    public async Task<bool> ExisteNomeAsync(string nome, int? idOriginalIgnorar = null)
    {
        using var conn = _factory.CreateConnection();
        var sql = idOriginalIgnorar.HasValue
            ? "SELECT COUNT(*) FROM paises WHERE pais = @nome AND id <> @idOriginalIgnorar"
            : "SELECT COUNT(*) FROM paises WHERE pais = @nome";
        return await conn.ExecuteScalarAsync<int>(sql, new { nome, idOriginalIgnorar }) > 0;
    }

    public async Task<bool> ExisteDdiAsync(string ddi, int? idOriginalIgnorar = null)
    {
        using var conn = _factory.CreateConnection();
        var sql = idOriginalIgnorar.HasValue
            ? "SELECT COUNT(*) FROM paises WHERE ddi = @ddi AND id <> @idOriginalIgnorar"
            : "SELECT COUNT(*) FROM paises WHERE ddi = @ddi";
        return await conn.ExecuteScalarAsync<int>(sql, new { ddi, idOriginalIgnorar }) > 0;
    }
}