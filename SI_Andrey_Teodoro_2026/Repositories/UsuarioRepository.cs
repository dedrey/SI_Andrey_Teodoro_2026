using Dapper;
using SI_Andrey_Teodoro_2026.Data;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Models;
using SI_Andrey_Teodoro_2026.Repositories.Interfaces;

namespace SI_Andrey_Teodoro_2026.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly DbConnectionFactory _factory;
    public UsuarioRepository(DbConnectionFactory factory) => _factory = factory;

    public async Task<PaginacaoDto<UsuarioListDto>> ObterTodosAsync(FiltroConsultaDto filtro)
    {
        using var conn = _factory.CreateConnection();
        var where = new List<string>();
        if (!string.IsNullOrWhiteSpace(filtro.Busca))
            where.Add("(u.nome LIKE @Busca OR u.email LIKE @Busca OR u.cpf LIKE @Busca OR CAST(u.id AS CHAR) = @BuscaExata)");
        where.Add(filtro.StatusFiltro switch { "ativos" => "u.ativo = TRUE", "inativos" => "u.ativo = FALSE", _ => "1=1" });
        var whereClause = "WHERE " + string.Join(" AND ", where);
        var orderBy = filtro.OrdenarPor switch { "id" => "u.id", "data" => "u.criado_em", _ => "u.nome" };

        var sqlCount = $"SELECT COUNT(*) FROM usuarios u {whereClause}";
        var sqlData = $@"SELECT u.id, u.nome, u.email, u.cpf, u.telefone, u.ativo,
                                 u.criado_em AS CriadoEm
                          FROM usuarios u {whereClause}
                          ORDER BY {orderBy} LIMIT @Limit OFFSET @Offset";

        var param = new
        {
            Busca = $"%{filtro.Busca}%",
            BuscaExata = filtro.Busca,
            Limit = filtro.TamanhoPagina,
            Offset = (filtro.Pagina - 1) * filtro.TamanhoPagina
        };
        var total = await conn.ExecuteScalarAsync<int>(sqlCount, param);
        var itens = await conn.QueryAsync<UsuarioListDto>(sqlData, param);
        return new PaginacaoDto<UsuarioListDto> { Itens = itens.ToList(), TotalItens = total, Pagina = filtro.Pagina, TamanhoPagina = filtro.TamanhoPagina };
    }

    public async Task<IEnumerable<UsuarioListDto>> ObterTodosAtivosAsync()
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryAsync<UsuarioListDto>(
            "SELECT id, nome, email FROM usuarios WHERE ativo = TRUE ORDER BY nome");
    }

    public async Task<Usuario?> ObterPorIdAsync(int id)
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Usuario>(
            @"SELECT u.id, u.nome, u.email, u.cpf, u.telefone, u.ativo,
                     u.criado_em AS CriadoEm, u.atualizado_em AS AtualizadoEm,
                     ua.nome AS NomeAtualizadoPor
              FROM usuarios u
              LEFT JOIN usuarios ua ON ua.id = u.atualizado_por
              WHERE u.id = @id", new { id });
    }

    public async Task<int> InserirAsync(UsuarioDto dto)
    {
        using var conn = _factory.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            @"INSERT INTO usuarios (nome, email, cpf, telefone, ativo)
              VALUES (@Nome, @Email, @Cpf, @Telefone, @Ativo);
              SELECT LAST_INSERT_ID();", dto);
    }

    public async Task AtualizarAsync(UsuarioDto dto)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            @"UPDATE usuarios SET id = @Id, nome = @Nome, email = @Email,
                                  cpf = @Cpf, telefone = @Telefone
              WHERE id = @IdOriginal", dto);
        await conn.ExecuteAsync("ALTER TABLE usuarios AUTO_INCREMENT = 1");
    }

    public async Task AlterarStatusAsync(int id, bool ativo)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync("UPDATE usuarios SET ativo = @ativo WHERE id = @id", new { ativo, id });
    }

    public async Task<bool> ExisteCpfAsync(string cpf, int? idOriginalIgnorar = null)
    {
        using var conn = _factory.CreateConnection();
        var sql = idOriginalIgnorar.HasValue
            ? "SELECT COUNT(*) FROM usuarios WHERE cpf = @cpf AND id <> @idOriginalIgnorar"
            : "SELECT COUNT(*) FROM usuarios WHERE cpf = @cpf";
        return await conn.ExecuteScalarAsync<int>(sql, new { cpf, idOriginalIgnorar }) > 0;
    }

    public async Task<bool> ExisteEmailAsync(string email, int? idOriginalIgnorar = null)
    {
        using var conn = _factory.CreateConnection();
        var sql = idOriginalIgnorar.HasValue
            ? "SELECT COUNT(*) FROM usuarios WHERE email = @email AND id <> @idOriginalIgnorar"
            : "SELECT COUNT(*) FROM usuarios WHERE email = @email";
        return await conn.ExecuteScalarAsync<int>(sql, new { email, idOriginalIgnorar }) > 0;
    }
}