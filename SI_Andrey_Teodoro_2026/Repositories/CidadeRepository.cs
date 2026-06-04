using Dapper;
using SI_Andrey_Teodoro_2026.Data;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Models;
using SI_Andrey_Teodoro_2026.Repositories.Interfaces;

namespace SI_Andrey_Teodoro_2026.Repositories;

public class CidadeRepository : BaseRepository, ICidadeRepository
{
    public CidadeRepository(DbConnectionFactory factory) : base(factory) { }

    protected override string Tabela => "cidades";

    public async Task<PaginacaoDto<CidadeListDto>> ObterTodosAsync(FiltroConsultaDto filtro)
    {
        using var conn = _factory.CreateConnection();
        var where = new List<string>();
        if (!string.IsNullOrWhiteSpace(filtro.Busca))
            where.Add("(c.cidade LIKE @Busca OR e.uf LIKE @Busca OR p.pais LIKE @Busca OR e.estado LIKE @Busca OR CAST(c.id AS CHAR) = @BuscaExata)");
        where.Add(filtro.StatusFiltro switch { "ativos" => "c.ativo = TRUE", "inativos" => "c.ativo = FALSE", _ => "1=1" });
        var whereClause = "WHERE " + string.Join(" AND ", where);
        var orderBy = filtro.OrdenarPor switch { "id" => "c.id", "data" => "c.criado_em", _ => "c.cidade" };

        var sqlCount = $@"SELECT COUNT(*) FROM cidades c
                          INNER JOIN estados e ON e.id = c.estado_id
                          INNER JOIN paises  p ON p.id = e.pais_id {whereClause}";
        var sqlData = $@"SELECT c.id, c.cidade AS NomeCidade, c.ddd, c.estado_id AS EstadoId,
                                 e.pais_id AS PaisId, e.estado AS NomeEstado, e.uf,
                                 p.pais AS NomePais, p.ddi AS Ddi, c.ativo,
                                 c.criado_em AS CriadoEm
                          FROM cidades c
                          INNER JOIN estados e ON e.id = c.estado_id
                          INNER JOIN paises  p ON p.id = e.pais_id
                          {whereClause} ORDER BY {orderBy} LIMIT @Limit OFFSET @Offset";

        var param = new
        {
            Busca = $"%{filtro.Busca}%",
            BuscaExata = filtro.Busca,
            Limit = filtro.TamanhoPagina,
            Offset = (filtro.Pagina - 1) * filtro.TamanhoPagina
        };

        var total = await conn.ExecuteScalarAsync<int>(sqlCount, param);
        var itens = await conn.QueryAsync<CidadeListDto>(sqlData, param);
        return new PaginacaoDto<CidadeListDto>
        {
            Itens = itens.ToList(),
            TotalItens = total,
            Pagina = filtro.Pagina,
            TamanhoPagina = filtro.TamanhoPagina
        };
    }

    public async Task<IEnumerable<CidadeListDto>> ObterPorEstadoAsync(int estadoId)
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryAsync<CidadeListDto>(
            @"SELECT c.id, c.cidade AS NomeCidade, c.ddd, c.estado_id AS EstadoId, e.pais_id AS PaisId,
                     e.estado AS NomeEstado, e.uf, p.pais AS NomePais
              FROM cidades c
              INNER JOIN estados e ON e.id = c.estado_id
              INNER JOIN paises  p ON p.id = e.pais_id
              WHERE c.estado_id = @estadoId AND c.ativo = TRUE ORDER BY c.cidade",
            new { estadoId });
    }

    public async Task<IEnumerable<CidadeListDto>> ObterTodosAtivosSemPaginacaoAsync()
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryAsync<CidadeListDto>(
            @"SELECT c.id, c.cidade AS NomeCidade, c.ddd,
                     c.estado_id AS EstadoId, e.pais_id AS PaisId,
                     e.estado AS NomeEstado, e.uf, p.pais AS NomePais
              FROM cidades c
              INNER JOIN estados e ON e.id = c.estado_id
              INNER JOIN paises  p ON p.id = e.pais_id
              WHERE c.ativo = TRUE ORDER BY c.cidade");
    }

    public async Task<Cidade?> ObterPorIdAsync(int id)
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Cidade>(
            @"SELECT c.id, c.cidade AS NomeCidade, c.ddd, c.estado_id AS EstadoId,
                     e.estado AS NomeEstado, e.uf, p.pais AS NomePais, c.ativo,
                     c.criado_em AS CriadoEm, c.atualizado_em AS AtualizadoEm,
                     ua.nome AS NomeAtualizadoPor
              FROM cidades c
              INNER JOIN estados   e  ON e.id  = c.estado_id
              INNER JOIN paises    p  ON p.id  = e.pais_id
              LEFT  JOIN usuarios  ua ON ua.id = c.atualizado_por
              WHERE c.id = @id", new { id });
    }

    public async Task<int> InserirAsync(CidadeDto dto)
    {
        using var conn = _factory.CreateConnection();
        var proximoId = await ProximoIdAsync();
        await conn.ExecuteAsync(
            @"INSERT INTO cidades (id, cidade, ddd, estado_id, ativo)
              VALUES (@ProximoId, @NomeCidade, @Ddd, @EstadoId, @Ativo)",
            new { ProximoId = proximoId, dto.NomeCidade, dto.Ddd, dto.EstadoId, dto.Ativo });
        return proximoId;
    }

    public async Task AtualizarAsync(CidadeDto dto)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            @"UPDATE cidades
              SET id = @Id, cidade = @NomeCidade, ddd = @Ddd, estado_id = @EstadoId,
                  atualizado_em = NOW()
              WHERE id = @IdOriginal", dto);
    }

    public Task AlterarStatusAsync(int id, bool ativo)
        => AlterarStatusBaseAsync(id, ativo);

    public async Task<bool> ExisteNomeNoEstadoAsync(string nome, int estadoId, int? idOriginalIgnorar = null)
    {
        using var conn = _factory.CreateConnection();
        var sql = idOriginalIgnorar.HasValue
            ? "SELECT COUNT(*) FROM cidades WHERE cidade = @nome AND estado_id = @estadoId AND id <> @idOriginalIgnorar"
            : "SELECT COUNT(*) FROM cidades WHERE cidade = @nome AND estado_id = @estadoId";
        return await conn.ExecuteScalarAsync<int>(sql, new { nome, estadoId, idOriginalIgnorar }) > 0;
    }
}