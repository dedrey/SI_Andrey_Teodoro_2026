using Dapper;
using SI_Andrey_Teodoro_2026.Data;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Models;
using SI_Andrey_Teodoro_2026.Repositories.Interfaces;

namespace SI_Andrey_Teodoro_2026.Repositories;

public class EmitenteRepository : BaseRepository, IEmitenteRepository
{
    public EmitenteRepository(DbConnectionFactory factory) : base(factory) { }

    protected override string Tabela => "emitentes";

    public async Task<PaginacaoDto<EmitenteListDto>> ObterTodosAsync(FiltroConsultaDto filtro)
    {
        using var conn = _factory.CreateConnection();
        var where = new List<string>();
        if (!string.IsNullOrWhiteSpace(filtro.Busca))
            where.Add(@"(e.nome_razaosocial     LIKE @Busca
                      OR e.apelido_nomefantasia LIKE @Busca
                      OR e.cnpj                 LIKE @Busca
                      OR CAST(e.id AS CHAR) = @BuscaExata)");
        where.Add(filtro.StatusFiltro switch
        {
            "ativos" => "e.ativo = TRUE",
            "inativos" => "e.ativo = FALSE",
            _ => "1=1"
        });
        var whereClause = "WHERE " + string.Join(" AND ", where);
        var orderBy = filtro.OrdenarPor switch
        {
            "id" => "e.id",
            "data" => "e.criado_em",
            _ => "e.nome_razaosocial"
        };

        var sqlCount = $"SELECT COUNT(*) FROM emitentes e {whereClause}";
        var sqlData = $@"SELECT e.id,
                                  e.nome_razaosocial      AS NomeRazaoSocial,
                                  e.apelido_nomefantasia  AS ApelidoNomeFantasia,
                                  e.cnpj,
                                  e.regime_tributario     AS RegimeTributario,
                                  c.cidade                AS NomeCidade,
                                  e.telefone, e.email,
                                  e.ativo,
                                  e.criado_em AS CriadoEm
                          FROM emitentes e
                          LEFT JOIN cidades c ON c.id = e.cidade_id
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
        var itens = await conn.QueryAsync<EmitenteListDto>(sqlData, param);
        return new PaginacaoDto<EmitenteListDto>
        {
            Itens = itens.ToList(),
            TotalItens = total,
            Pagina = filtro.Pagina,
            TamanhoPagina = filtro.TamanhoPagina
        };
    }

    public async Task<IEnumerable<EmitenteListDto>> ObterTodosAtivosAsync()
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryAsync<EmitenteListDto>(
            @"SELECT id,
                     nome_razaosocial     AS NomeRazaoSocial,
                     apelido_nomefantasia AS ApelidoNomeFantasia,
                     cnpj
              FROM emitentes WHERE ativo = TRUE ORDER BY nome_razaosocial");
    }

    public async Task<Emitente?> ObterPorIdAsync(int id)
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Emitente>(
            @"SELECT e.id,
                     e.nome_razaosocial      AS NomeRazaoSocial,
                     e.apelido_nomefantasia  AS ApelidoNomeFantasia,
                     e.cnpj,
                     e.inscricao_estadual    AS InscricaoEstadual,
                     e.regime_tributario     AS RegimeTributario,
                     e.cidade_id             AS CidadeId,
                     c.cidade                AS NomeCidade,
                     e.endereco, e.numero, e.complemento, e.bairro,
                     e.telefone, e.email,
                     e.ativo,
                     e.criado_em     AS CriadoEm,
                     e.atualizado_em AS AtualizadoEm,
                     ua.nome         AS NomeAtualizadoPor
              FROM emitentes e
              LEFT JOIN cidades  c  ON c.id  = e.cidade_id
              LEFT JOIN usuarios ua ON ua.id = e.atualizado_por
              WHERE e.id = @id", new { id });
    }

    public async Task<int> InserirAsync(EmitenteDto dto)
    {
        using var conn = _factory.CreateConnection();
        var proximoId = await ProximoIdAsync();
        await conn.ExecuteAsync(
            @"INSERT INTO emitentes
                (id, nome_razaosocial, apelido_nomefantasia, cnpj, inscricao_estadual,
                 regime_tributario, cidade_id, endereco, numero, complemento, bairro,
                 telefone, email, ativo)
              VALUES
                (@ProximoId, @NomeRazaoSocial, @ApelidoNomeFantasia, @Cnpj, @InscricaoEstadual,
                 @RegimeTributario, @CidadeId, @Endereco, @Numero, @Complemento, @Bairro,
                 @Telefone, @Email, @Ativo)",
            new
            {
                ProximoId = proximoId,
                NomeRazaoSocial = dto.NomeRazaoSocial,
                ApelidoNomeFantasia = dto.ApelidoNomeFantasia,
                dto.Cnpj,
                dto.InscricaoEstadual,
                dto.RegimeTributario,
                dto.CidadeId,
                dto.Endereco,
                dto.Numero,
                dto.Complemento,
                dto.Bairro,
                dto.Telefone,
                dto.Email,
                dto.Ativo
            });
        return proximoId;
    }

    public async Task AtualizarAsync(EmitenteDto dto)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            @"UPDATE emitentes
              SET id                   = @Id,
                  nome_razaosocial     = @NomeRazaoSocial,
                  apelido_nomefantasia = @ApelidoNomeFantasia,
                  cnpj                 = @Cnpj,
                  inscricao_estadual   = @InscricaoEstadual,
                  regime_tributario    = @RegimeTributario,
                  cidade_id            = @CidadeId,
                  endereco             = @Endereco,
                  numero               = @Numero,
                  complemento          = @Complemento,
                  bairro               = @Bairro,
                  telefone             = @Telefone,
                  email                = @Email,
                  atualizado_em        = NOW()
              WHERE id = @IdOriginal",
            new
            {
                dto.Id,
                dto.IdOriginal,
                NomeRazaoSocial = dto.NomeRazaoSocial,
                ApelidoNomeFantasia = dto.ApelidoNomeFantasia,
                dto.Cnpj,
                dto.InscricaoEstadual,
                dto.RegimeTributario,
                dto.CidadeId,
                dto.Endereco,
                dto.Numero,
                dto.Complemento,
                dto.Bairro,
                dto.Telefone,
                dto.Email
            });
    }

    public Task AlterarStatusAsync(int id, bool ativo)
        => AlterarStatusBaseAsync(id, ativo);

    public async Task<bool> ExisteCnpjAsync(string cnpj, int? idOriginalIgnorar = null)
    {
        using var conn = _factory.CreateConnection();
        var sql = idOriginalIgnorar.HasValue
            ? "SELECT COUNT(*) FROM emitentes WHERE cnpj = @cnpj AND id <> @idOriginalIgnorar"
            : "SELECT COUNT(*) FROM emitentes WHERE cnpj = @cnpj";
        return await conn.ExecuteScalarAsync<int>(sql, new { cnpj, idOriginalIgnorar }) > 0;
    }
}