using Dapper;
using SI_Andrey_Teodoro_2026.Data;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Models;
using SI_Andrey_Teodoro_2026.Repositories.Interfaces;

namespace SI_Andrey_Teodoro_2026.Repositories;

public class ClienteRepository : IClienteRepository
{
    private readonly DbConnectionFactory _factory;
    public ClienteRepository(DbConnectionFactory factory) => _factory = factory;

    public async Task<PaginacaoDto<ClienteListDto>> ObterTodosAsync(FiltroConsultaDto filtro)
    {
        using var conn = _factory.CreateConnection();

        var where = new List<string>();
        if (!string.IsNullOrWhiteSpace(filtro.Busca))
            where.Add(@"(c.nome_razaosocial       LIKE @Busca
                      OR c.cpf_cnpj               LIKE @Busca
                      OR c.documento_estrangeiro  LIKE @Busca
                      OR c.apelido_nomefantasia   LIKE @Busca
                      OR CAST(c.id AS CHAR) = @BuscaExata)");
        where.Add(filtro.StatusFiltro switch
        {
            "ativos" => "c.ativo = TRUE",
            "inativos" => "c.ativo = FALSE",
            _ => "1=1"
        });
        var whereClause = "WHERE " + string.Join(" AND ", where);
        var orderBy = filtro.OrdenarPor switch
        {
            "id" => "c.id",
            "data" => "c.criado_em",
            _ => "c.nome_razaosocial"
        };

        var sqlCount = $"SELECT COUNT(*) FROM clientes c {whereClause}";
        var sqlData = $@"SELECT c.id,
                                 c.nome_razaosocial      AS NomeRazaoSocial,
                                 c.cpf_cnpj              AS CpfCnpj,
                                 c.tipo_pessoa           AS TipoPessoa,
                                 c.estrangeiro,
                                 c.documento_estrangeiro AS DocumentoEstrangeiro,
                                 c.pais_origem           AS PaisOrigem,
                                 c.apelido_nomefantasia  AS ApelidoNomeFantasia,
                                 c.telefone, c.email,
                                 c.limite_credito        AS LimiteCredito,
                                 c.ativo,
                                 c.criado_em             AS CriadoEm
                          FROM clientes c
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
        var itens = await conn.QueryAsync<ClienteListDto>(sqlData, param);

        return new PaginacaoDto<ClienteListDto>
        {
            Itens = itens.ToList(),
            TotalItens = total,
            Pagina = filtro.Pagina,
            TamanhoPagina = filtro.TamanhoPagina
        };
    }

    public async Task<IEnumerable<ClienteListDto>> ObterTodosAtivosAsync()
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryAsync<ClienteListDto>(
            @"SELECT id, nome_razaosocial AS NomeRazaoSocial, cpf_cnpj AS CpfCnpj,
                     tipo_pessoa AS TipoPessoa, estrangeiro
              FROM clientes WHERE ativo = TRUE ORDER BY nome_razaosocial");
    }

    public async Task<Cliente?> ObterPorIdAsync(int id)
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Cliente>(
            @"SELECT c.id,
                     c.nome_razaosocial      AS NomeRazaoSocial,
                     c.cpf_cnpj              AS CpfCnpj,
                     c.tipo_pessoa           AS TipoPessoa,
                     c.estrangeiro,
                     c.documento_estrangeiro AS DocumentoEstrangeiro,
                     c.pais_origem           AS PaisOrigem,
                     c.apelido_nomefantasia  AS ApelidoNomeFantasia,
                     c.cidade_id             AS CidadeId,
                     cid.cidade              AS NomeCidade,
                     c.endereco, c.complemento, c.bairro,
                     c.telefone, c.email,
                     c.inscricao_estadual    AS InscricaoEstadual,
                     c.inscricao_municipal   AS InscricaoMunicipal,
                     c.limite_credito        AS LimiteCredito,
                     c.ativo,
                     c.criado_em             AS CriadoEm,
                     c.atualizado_em         AS AtualizadoEm,
                     ua.nome                 AS NomeAtualizadoPor
              FROM clientes c
              LEFT JOIN cidades  cid ON cid.id = c.cidade_id
              LEFT JOIN usuarios ua  ON ua.id  = c.atualizado_por
              WHERE c.id = @id",
            new { id });
    }

    public async Task<int> InserirAsync(ClienteDto dto)
    {
        using var conn = _factory.CreateConnection();
        var proximoId = await conn.ExecuteScalarAsync<int>(
            @"SELECT MIN(seq) FROM (SELECT 1 AS seq UNION ALL SELECT id+1 FROM clientes) t
              WHERE seq NOT IN (SELECT id FROM clientes)");

        await conn.ExecuteAsync(
            @"INSERT INTO clientes
                (id, nome_razaosocial, cpf_cnpj, tipo_pessoa, estrangeiro,
                 documento_estrangeiro, pais_origem, apelido_nomefantasia,
                 cidade_id, endereco, complemento, bairro, telefone, email,
                 inscricao_estadual, inscricao_municipal, limite_credito, ativo)
              VALUES
                (@ProximoId, @NomeRazaoSocial, @CpfCnpj, @TipoPessoa, @Estrangeiro,
                 @DocumentoEstrangeiro, @PaisOrigem, @ApelidoNomeFantasia,
                 @CidadeId, @Endereco, @Complemento, @Bairro, @Telefone, @Email,
                 @InscricaoEstadual, @InscricaoMunicipal, @LimiteCredito, @Ativo)",
            new
            {
                ProximoId = proximoId,
                dto.NomeRazaoSocial,
                dto.CpfCnpj,
                dto.TipoPessoa,
                dto.Estrangeiro,
                dto.DocumentoEstrangeiro,
                dto.PaisOrigem,
                dto.ApelidoNomeFantasia,
                dto.CidadeId,
                dto.Endereco,
                dto.Complemento,
                dto.Bairro,
                dto.Telefone,
                dto.Email,
                dto.InscricaoEstadual,
                dto.InscricaoMunicipal,
                dto.LimiteCredito,
                dto.Ativo
            });

        return proximoId;
    }

    public async Task AtualizarAsync(ClienteDto dto)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            @"UPDATE clientes
              SET id                    = @Id,
                  nome_razaosocial      = @NomeRazaoSocial,
                  cpf_cnpj              = @CpfCnpj,
                  tipo_pessoa           = @TipoPessoa,
                  estrangeiro           = @Estrangeiro,
                  documento_estrangeiro = @DocumentoEstrangeiro,
                  pais_origem           = @PaisOrigem,
                  apelido_nomefantasia  = @ApelidoNomeFantasia,
                  cidade_id             = @CidadeId,
                  endereco              = @Endereco,
                  complemento           = @Complemento,
                  bairro                = @Bairro,
                  telefone              = @Telefone,
                  email                 = @Email,
                  inscricao_estadual    = @InscricaoEstadual,
                  inscricao_municipal   = @InscricaoMunicipal,
                  limite_credito        = @LimiteCredito,
                  atualizado_em         = NOW()
              WHERE id = @IdOriginal", dto);
    }

    public async Task AlterarStatusAsync(int id, bool ativo)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE clientes SET ativo = @ativo, atualizado_em = NOW() WHERE id = @id",
            new { ativo, id });
    }

    public async Task<bool> ExisteDocumentoAsync(string documento, int? idOriginalIgnorar = null)
    {
        using var conn = _factory.CreateConnection();
        var sql = idOriginalIgnorar.HasValue
            ? @"SELECT COUNT(*) FROM clientes
                WHERE (cpf_cnpj = @documento OR documento_estrangeiro = @documento)
                  AND id <> @idOriginalIgnorar"
            : @"SELECT COUNT(*) FROM clientes
                WHERE (cpf_cnpj = @documento OR documento_estrangeiro = @documento)";
        return await conn.ExecuteScalarAsync<int>(sql, new { documento, idOriginalIgnorar }) > 0;
    }
}