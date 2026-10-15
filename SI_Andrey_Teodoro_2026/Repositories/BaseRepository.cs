using Dapper;
using MySqlConnector;
using SI_Andrey_Teodoro_2026.Data;

namespace SI_Andrey_Teodoro_2026.Repositories;

public abstract class BaseRepository
{
    protected readonly DbConnectionFactory _factory;

    protected BaseRepository(DbConnectionFactory factory)
        => _factory = factory;

    protected abstract string Tabela { get; }

    protected async Task<int> ProximoIdAsync()
    {
        using var conn = _factory.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            $@"SELECT MIN(seq)
               FROM (SELECT 1 AS seq UNION ALL SELECT id + 1 FROM {Tabela}) t
               WHERE seq NOT IN (SELECT id FROM {Tabela})");
    }

    public Task<int> ObterProximoIdAsync()
        => ProximoIdAsync();

    public async Task<bool> ExisteIdAsync(int id)
    {
        using var conn = _factory.CreateConnection();
        return await conn.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM {Tabela} WHERE id = @id", new { id }) > 0;
    }

    protected static async Task InserirComIdAsync(Func<Task> inserir)
    {
        try
        {
            await inserir();
        }
        catch (MySqlException ex) when (ex.ErrorCode == MySqlErrorCode.DuplicateKeyEntry && ex.Message.Contains("PRIMARY"))
        {
            throw new IdDuplicadoException();
        }
    }

    protected async Task AlterarStatusBaseAsync(int id, bool ativo)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            $"UPDATE {Tabela} SET ativo = @ativo, atualizado_em = NOW() WHERE id = @id",
            new { ativo, id });
    }
}