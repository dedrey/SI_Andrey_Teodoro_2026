using Dapper;
using SI_Andrey_Teodoro_2026.Data;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Models;
using SI_Andrey_Teodoro_2026.Repositories.Interfaces;

namespace SI_Andrey_Teodoro_2026.Repositories;

public class TransportadoraRepository : BaseRepository, ITransportadoraRepository
{
    public TransportadoraRepository(DbConnectionFactory factory) : base(factory) { }

    protected override string Tabela => "transportadoras";

    public async Task<PaginacaoDto<TransportadoraListDto>> ObterTodosAsync(FiltroConsultaDto filtro)
    {
        using var conn = _factory.CreateConnection();

        var where = new List<string>();
        if (!string.IsNullOrWhiteSpace(filtro.Busca))
            where.Add(@"(t.razaosocial   LIKE @Busca
                      OR t.nomefantasia LIKE @Busca
                      OR t.cnpj         LIKE @Busca
                      OR CAST(t.id AS CHAR) = @BuscaExata)");
        where.Add(filtro.StatusFiltro switch
        {
            "ativos" => "t.ativo = TRUE",
            "inativos" => "t.ativo = FALSE",
            _ => "1=1"
        });
        var whereClause = "WHERE " + string.Join(" AND ", where);
        var orderBy = filtro.OrdenarPor switch
        {
            "id" => "t.id",
            "data" => "t.criado_em",
            _ => "t.razaosocial"
        };

        var param = new
        {
            Busca = $"%{filtro.Busca}%",
            BuscaExata = filtro.Busca,
            Limit = filtro.TamanhoPagina,
            Offset = (filtro.Pagina - 1) * filtro.TamanhoPagina
        };

        var total = await conn.ExecuteScalarAsync<int>(
            $"SELECT COUNT(*) FROM transportadoras t {whereClause}", param);

        var itens = await conn.QueryAsync<TransportadoraListDto>(
            $@"SELECT t.id,
                      t.razaosocial       AS RazaoSocial,
                      t.nomefantasia      AS NomeFantasia,
                      t.cnpj,
                      t.inscricao_estadual AS InscricaoEstadual,
                      c.cidade            AS NomeCidade,
                      t.telefone, t.email,
                      t.ativo,
                      t.criado_em         AS CriadoEm
               FROM transportadoras t
               LEFT JOIN cidades c ON c.id = t.cidade_id
               {whereClause}
               ORDER BY {orderBy}
               LIMIT @Limit OFFSET @Offset", param);

        return new PaginacaoDto<TransportadoraListDto>
        {
            Itens = itens.ToList(),
            TotalItens = total,
            Pagina = filtro.Pagina,
            TamanhoPagina = filtro.TamanhoPagina
        };
    }

    public async Task<IEnumerable<TransportadoraListDto>> ObterTodosAtivosAsync()
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryAsync<TransportadoraListDto>(
            @"SELECT t.id, t.razaosocial AS RazaoSocial,
                     t.nomefantasia AS NomeFantasia, t.cnpj
              FROM transportadoras t WHERE t.ativo = TRUE ORDER BY t.razaosocial");
    }

    public async Task<Transportadora?> ObterPorIdAsync(int id)
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Transportadora>(
            @"SELECT t.id,
                     t.razaosocial       AS RazaoSocial,
                     t.nomefantasia      AS NomeFantasia,
                     t.cnpj,
                     t.inscricao_estadual AS InscricaoEstadual,
                     t.cidade_id         AS CidadeId,
                     c.cidade            AS NomeCidade,
                     t.endereco,
                     t.complemento,
                     t.bairro,
                     t.telefone, t.email,
                     t.ativo,
                     t.criado_em         AS CriadoEm,
                     t.atualizado_em     AS AtualizadoEm,
                     ua.nome             AS NomeAtualizadoPor
              FROM transportadoras t
              LEFT JOIN cidades  c  ON c.id  = t.cidade_id
              LEFT JOIN usuarios ua ON ua.id = t.atualizado_por
              WHERE t.id = @id", new { id });
    }

    public async Task<int> InserirAsync(TransportadoraDto dto)
    {
        using var conn = _factory.CreateConnection();
        var proximoId = await ProximoIdAsync();
        await conn.ExecuteAsync(
            @"INSERT INTO transportadoras
                (id, razaosocial, nomefantasia, cnpj, inscricao_estadual, cidade_id,
                endereco, complemento, bairro, telefone, email, ativo)
            VALUES
                (@ProximoId, @RazaoSocial, @NomeFantasia, @Cnpj, @InscricaoEstadual, @CidadeId,
                @Endereco, @Complemento, @Bairro, @Telefone, @Email, @Ativo)",
            new
            {
                ProximoId = proximoId,
                dto.RazaoSocial,
                dto.NomeFantasia,
                dto.Cnpj,
                dto.InscricaoEstadual,
                dto.CidadeId,
                dto.Endereco,
                dto.Complemento,
                dto.Bairro,
                dto.Telefone,
                dto.Email,
                dto.Ativo
            });
        return proximoId;
    }

    public async Task AtualizarAsync(TransportadoraDto dto)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            @"UPDATE transportadoras
              SET id                 = @Id,
                  razaosocial        = @RazaoSocial,
                  nomefantasia       = @NomeFantasia,
                  cnpj               = @Cnpj,
                  inscricao_estadual = @InscricaoEstadual,
                  cidade_id          = @CidadeId,
                  endereco           = @Endereco,
                  complemento        = @Complemento,
                  bairro             = @Bairro,
                  telefone           = @Telefone,
                  email              = @Email,
                  atualizado_em      = NOW()
              WHERE id = @IdOriginal", dto);
    }

    public Task AlterarStatusAsync(int id, bool ativo)
        => AlterarStatusBaseAsync(id, ativo);

    public async Task<bool> ExisteCnpjAsync(string cnpj, int? idOriginalIgnorar = null)
    {
        using var conn = _factory.CreateConnection();
        var sql = idOriginalIgnorar.HasValue
            ? "SELECT COUNT(*) FROM transportadoras WHERE cnpj = @cnpj AND id <> @idOriginalIgnorar"
            : "SELECT COUNT(*) FROM transportadoras WHERE cnpj = @cnpj";
        return await conn.ExecuteScalarAsync<int>(sql, new { cnpj, idOriginalIgnorar }) > 0;
    }
}