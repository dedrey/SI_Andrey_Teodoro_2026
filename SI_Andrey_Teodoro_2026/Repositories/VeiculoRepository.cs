using Dapper;
using SI_Andrey_Teodoro_2026.Data;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Models;
using SI_Andrey_Teodoro_2026.Repositories.Interfaces;

namespace SI_Andrey_Teodoro_2026.Repositories;

public class VeiculoRepository : BaseRepository, IVeiculoRepository
{
    public VeiculoRepository(DbConnectionFactory factory) : base(factory) { }

    protected override string Tabela => "veiculos";

    public async Task<PaginacaoDto<VeiculoListDto>> ObterTodosAsync(FiltroConsultaDto filtro)
    {
        using var conn = _factory.CreateConnection();

        var where = new List<string>();
        if (!string.IsNullOrWhiteSpace(filtro.Busca))
            where.Add(@"(v.placa LIKE @Busca
                      OR v.uf   LIKE @Busca
                      OR t.razaosocial LIKE @Busca
                      OR CAST(v.id AS CHAR) = @BuscaExata)");
        where.Add(filtro.StatusFiltro switch
        {
            "ativos" => "v.ativo = TRUE",
            "inativos" => "v.ativo = FALSE",
            _ => "1=1"
        });
        var whereClause = "WHERE " + string.Join(" AND ", where);
        var orderBy = filtro.OrdenarPor switch
        {
            "id" => "v.id",
            "data" => "v.criado_em",
            "transportadora" => "t.razaosocial",
            _ => "v.placa"
        };

        var sqlCount = $@"SELECT COUNT(*) FROM veiculos v
                          INNER JOIN transportadoras t ON t.id = v.transportadora_id
                          {whereClause}";
        var sqlData = $@"SELECT v.id,
                                 v.transportadora_id   AS TransportadoraId,
                                 t.razaosocial         AS NomeTransportadora,
                                 v.placa, v.uf,
                                 v.ativo,
                                 v.criado_em           AS CriadoEm
                          FROM veiculos v
                          INNER JOIN transportadoras t ON t.id = v.transportadora_id
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
        var itens = await conn.QueryAsync<VeiculoListDto>(sqlData, param);

        return new PaginacaoDto<VeiculoListDto>
        {
            Itens = itens.ToList(),
            TotalItens = total,
            Pagina = filtro.Pagina,
            TamanhoPagina = filtro.TamanhoPagina
        };
    }

    public async Task<IEnumerable<VeiculoListDto>> ObterTodosAtivosAsync()
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryAsync<VeiculoListDto>(
            @"SELECT v.id, v.placa, v.uf,
                     v.transportadora_id AS TransportadoraId,
                     t.razaosocial       AS NomeTransportadora
              FROM veiculos v
              INNER JOIN transportadoras t ON t.id = v.transportadora_id
              WHERE v.ativo = TRUE ORDER BY v.placa");
    }

    public async Task<Veiculo?> ObterPorIdAsync(int id)
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Veiculo>(
            @"SELECT v.id,
                     v.transportadora_id  AS TransportadoraId,
                     t.razaosocial        AS NomeTransportadora,
                     v.placa, v.uf,
                     v.ativo,
                     v.criado_em          AS CriadoEm,
                     v.atualizado_em      AS AtualizadoEm,
                     ua.nome              AS NomeAtualizadoPor
              FROM veiculos v
              INNER JOIN transportadoras t ON t.id  = v.transportadora_id
              LEFT  JOIN usuarios        ua ON ua.id = v.atualizado_por
              WHERE v.id = @id", new { id });
    }

    public async Task<int> InserirAsync(VeiculoDto dto)
    {
        using var conn = _factory.CreateConnection();
        var proximoId = await ProximoIdAsync();

        await conn.ExecuteAsync(
            @"INSERT INTO veiculos (id, transportadora_id, placa, uf, ativo)
              VALUES (@ProximoId, @TransportadoraId, @Placa, @Uf, @Ativo)",
            new { ProximoId = proximoId, dto.TransportadoraId, dto.Placa, dto.Uf, dto.Ativo });

        return proximoId;
    }

    public async Task AtualizarAsync(VeiculoDto dto)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            @"UPDATE veiculos
              SET id                = @Id,
                  transportadora_id = @TransportadoraId,
                  placa             = @Placa,
                  uf                = @Uf,
                  atualizado_em     = NOW()
              WHERE id = @IdOriginal", dto);
    }

    public Task AlterarStatusAsync(int id, bool ativo)
        => AlterarStatusBaseAsync(id, ativo);

    public async Task<bool> ExistePlacaAsync(string placa, int? idOriginalIgnorar = null)
    {
        using var conn = _factory.CreateConnection();
        var sql = idOriginalIgnorar.HasValue
            ? "SELECT COUNT(*) FROM veiculos WHERE placa = @placa AND id <> @idOriginalIgnorar"
            : "SELECT COUNT(*) FROM veiculos WHERE placa = @placa";
        return await conn.ExecuteScalarAsync<int>(sql, new { placa, idOriginalIgnorar }) > 0;
    }
}