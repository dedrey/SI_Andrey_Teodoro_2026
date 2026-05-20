using MySqlConnector;
using System.Data;

namespace SI_Andrey_Teodoro_2026.Data;

public class DbConnectionFactory
{
    private readonly string _connectionString;

    public DbConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("String de conexão 'DefaultConnection' não encontrada.");
    }

    public IDbConnection CreateConnection()
        => new MySqlConnection(_connectionString);
}