using Dapper;
using SI_Andrey_Teodoro_2026.Data;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Models;
using SI_Andrey_Teodoro_2026.Repositories.Interfaces;

namespace SI_Andrey_Teodoro_2026.Repositories;

public class FornecedorRepository : BaseRepository, IFornecedorRepository
{
    public FornecedorRepository(DbConnectionFactory factory) : base(factory) { }

    protected override string Tabela => "fornecedores";

    public async Task<PaginacaoDto<FornecedorListDto>> ObterTodosAsync(FiltroConsultaDto filtro)
    {
        using var conn = _factory.CreateConnection();
        var where = new List<string>();
        if (!string.IsNullOrWhiteSpace(filtro.Busca))
            where.Add(@"(f.razaosocial   LIKE @Busca
                      OR f.nomefantasia  LIKE @Busca
                      OR f.cnpj          LIKE @Busca
                      OR CAST(f.id AS CHAR) = @BuscaExata)");
        where.Add(filtro.StatusFiltro switch
        {
            "ativos" => "f.ativo = TRUE",
            "inativos" => "f.ativo = FALSE",
            _ => "1=1"
        });
        var whereClause = "WHERE " + string.Join(" AND ", where);
        var orderBy = filtro.OrdenarPor switch
        {
            "id" => "f.id",
            "data" => "f.criado_em",
            _ => "f.razaosocial"
        };

        var sqlCount = $"SELECT COUNT(*) FROM fornecedores f {whereClause}";
        var sqlData = $@"SELECT f.id,
                                  f.razaosocial   AS RazaoSocial,
                                  f.nomefantasia  AS NomeFantasia,
                                  f.cnpj,
                                  c.cidade        AS NomeCidade,
                                  f.telefone, f.email,
                                  f.ativo,
                                  f.criado_em AS CriadoEm
                          FROM fornecedores f
                          LEFT JOIN cidades c ON c.id = f.cidade_id
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
        var itens = await conn.QueryAsync<FornecedorListDto>(sqlData, param);
        return new PaginacaoDto<FornecedorListDto>
        {
            Itens = itens.ToList(),
            TotalItens = total,
            Pagina = filtro.Pagina,
            TamanhoPagina = filtro.TamanhoPagina
        };
    }

    public async Task<Fornecedor?> ObterPorIdAsync(int id)
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Fornecedor>(
            @"SELECT f.id,
                     f.razaosocial   AS RazaoSocial,
                     f.nomefantasia  AS NomeFantasia,
                     f.cnpj,
                     f.cidade_id     AS CidadeId,
                     c.cidade        AS NomeCidade,
                     f.endereco, f.complemento, f.bairro,
                     f.telefone, f.email,
                     f.ativo,
                     f.criado_em     AS CriadoEm,
                     f.atualizado_em AS AtualizadoEm,
                     ua.nome         AS NomeAtualizadoPor
              FROM fornecedores f
              LEFT JOIN cidades  c  ON c.id  = f.cidade_id
              LEFT JOIN usuarios ua ON ua.id = f.atualizado_por
              WHERE f.id = @id", new { id });
    }

    public async Task<int> InserirAsync(FornecedorDto dto)
    {
        using var conn = _factory.CreateConnection();
        var proximoId = await ProximoIdAsync();
        await conn.ExecuteAsync(
            @"INSERT INTO fornecedores
                (id, razaosocial, nomefantasia, cnpj, cidade_id,
                 endereco, complemento, bairro, telefone, email, ativo)
              VALUES
                (@ProximoId, @RazaoSocial, @NomeFantasia, @Cnpj, @CidadeId,
                 @Endereco, @Complemento, @Bairro, @Telefone, @Email, @Ativo)",
            new
            {
                ProximoId = proximoId,
                RazaoSocial = dto.RazaoSocial,
                NomeFantasia = dto.NomeFantasia,
                dto.Cnpj,
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

    public async Task AtualizarAsync(FornecedorDto dto)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            @"UPDATE fornecedores
              SET id           = @Id,
                  razaosocial  = @RazaoSocial,
                  nomefantasia = @NomeFantasia,
                  cnpj         = @Cnpj,
                  cidade_id    = @CidadeId,
                  endereco     = @Endereco,
                  complemento  = @Complemento,
                  bairro       = @Bairro,
                  telefone     = @Telefone,
                  email        = @Email,
                  atualizado_em = NOW()
              WHERE id = @IdOriginal",
            new
            {
                dto.Id,
                dto.IdOriginal,
                RazaoSocial = dto.RazaoSocial,
                NomeFantasia = dto.NomeFantasia,
                dto.Cnpj,
                dto.CidadeId,
                dto.Endereco,
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
            ? "SELECT COUNT(*) FROM fornecedores WHERE cnpj = @cnpj AND id <> @idOriginalIgnorar"
            : "SELECT COUNT(*) FROM fornecedores WHERE cnpj = @cnpj";
        return await conn.ExecuteScalarAsync<int>(sql, new { cnpj, idOriginalIgnorar }) > 0;
    }

    public async Task<bool> ExisteRazaoSocialAsync(string razaoSocial, int? idOriginalIgnorar = null)
    {
        using var conn = _factory.CreateConnection();
        var sql = idOriginalIgnorar.HasValue
            ? "SELECT COUNT(*) FROM fornecedores WHERE razaosocial = @razaoSocial AND id <> @idOriginalIgnorar"
            : "SELECT COUNT(*) FROM fornecedores WHERE razaosocial = @razaoSocial";
        return await conn.ExecuteScalarAsync<int>(sql, new { razaoSocial, idOriginalIgnorar }) > 0;
    }

    public async Task<bool> ExisteNomeFantasiaAsync(string nomeFantasia, int? idOriginalIgnorar = null)
    {
        using var conn = _factory.CreateConnection();
        var sql = idOriginalIgnorar.HasValue
            ? "SELECT COUNT(*) FROM fornecedores WHERE nomefantasia = @nomeFantasia AND id <> @idOriginalIgnorar"
            : "SELECT COUNT(*) FROM fornecedores WHERE nomefantasia = @nomeFantasia";
        return await conn.ExecuteScalarAsync<int>(sql, new { nomeFantasia, idOriginalIgnorar }) > 0;
    }
    public async Task<IEnumerable<FornecedorListDto>> ObterTodosAtivosAsync()
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryAsync<FornecedorListDto>(
            @"SELECT id,
                 razaosocial AS RazaoSocial,
                 nomefantasia AS NomeFantasia,
                 cnpj,
                 ativo
          FROM fornecedores
          WHERE ativo = TRUE
          ORDER BY razaosocial");
    }
}