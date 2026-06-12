using Dapper;
using SI_Andrey_Teodoro_2026.Data;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Repositories.Interfaces;

namespace SI_Andrey_Teodoro_2026.Repositories;

public class CorRepository : BaseRepository, ICorRepository
{
    public CorRepository(DbConnectionFactory factory) : base(factory) { }
    protected override string Tabela => "cores";

    public async Task<PaginacaoDto<CorListDto>> ObterTodosAsync(FiltroConsultaDto filtro)
    {
        using var conn = _factory.CreateConnection();
        var where = new List<string>();
        if (!string.IsNullOrWhiteSpace(filtro.Busca))
            where.Add("nome LIKE @Busca");
        where.Add(filtro.StatusFiltro switch
        {
            "ativos" => "ativo = TRUE",
            "inativos" => "ativo = FALSE",
            _ => "1=1"
        });
        var w = "WHERE " + string.Join(" AND ", where);
        var total = await conn.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM cores {w}",
            new { Busca = $"%{filtro.Busca}%" });
        var itens = await conn.QueryAsync<CorListDto>(
            $"SELECT id, nome, ativo FROM cores {w} ORDER BY nome LIMIT @Limit OFFSET @Offset",
            new { Busca = $"%{filtro.Busca}%", Limit = filtro.TamanhoPagina, Offset = (filtro.Pagina - 1) * filtro.TamanhoPagina });
        return new PaginacaoDto<CorListDto> { Itens = itens.ToList(), TotalItens = total, Pagina = filtro.Pagina, TamanhoPagina = filtro.TamanhoPagina };
    }

    public async Task<IEnumerable<CorListDto>> ObterTodosAtivosAsync()
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryAsync<CorListDto>(
            "SELECT id, nome, ativo FROM cores WHERE ativo = TRUE ORDER BY nome");
    }

    public async Task<int> InserirAsync(CorDto dto)
    {
        using var conn = _factory.CreateConnection();
        var id = await ProximoIdAsync();
        await conn.ExecuteAsync(
            "INSERT INTO cores (id, nome, ativo) VALUES (@id, @Nome, @Ativo)",
            new { id, dto.Nome, dto.Ativo });
        return id;
    }

    public async Task AtualizarAsync(CorDto dto)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE cores SET nome = @Nome WHERE id = @IdOriginal",
            new { dto.Nome, dto.IdOriginal });
    }

    public Task AlterarStatusAsync(int id, bool ativo)
        => AlterarStatusBaseAsync(id, ativo);

    public async Task<bool> ExisteNomeAsync(string nome, int? idIgnorar = null)
    {
        using var conn = _factory.CreateConnection();
        var sql = idIgnorar.HasValue
            ? "SELECT COUNT(*) FROM cores WHERE nome = @nome AND id <> @idIgnorar"
            : "SELECT COUNT(*) FROM cores WHERE nome = @nome";
        return await conn.ExecuteScalarAsync<int>(sql, new { nome, idIgnorar }) > 0;
    }
}