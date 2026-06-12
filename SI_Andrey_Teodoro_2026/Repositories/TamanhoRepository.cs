using Dapper;
using SI_Andrey_Teodoro_2026.Data;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Repositories.Interfaces;

namespace SI_Andrey_Teodoro_2026.Repositories;

public class TamanhoRepository : BaseRepository, ITamanhoRepository
{
    public TamanhoRepository(DbConnectionFactory factory) : base(factory) { }
    protected override string Tabela => "tamanhos";

    public async Task<PaginacaoDto<TamanhoListDto>> ObterTodosAsync(FiltroConsultaDto filtro)
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
        var total = await conn.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM tamanhos {w}",
            new { Busca = $"%{filtro.Busca}%" });
        var itens = await conn.QueryAsync<TamanhoListDto>(
            $"SELECT id, nome, ordem, ativo FROM tamanhos {w} ORDER BY ordem, nome LIMIT @Limit OFFSET @Offset",
            new { Busca = $"%{filtro.Busca}%", Limit = filtro.TamanhoPagina, Offset = (filtro.Pagina - 1) * filtro.TamanhoPagina });
        return new PaginacaoDto<TamanhoListDto> { Itens = itens.ToList(), TotalItens = total, Pagina = filtro.Pagina, TamanhoPagina = filtro.TamanhoPagina };
    }

    public async Task<IEnumerable<TamanhoListDto>> ObterTodosAtivosAsync()
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryAsync<TamanhoListDto>(
            "SELECT id, nome, ordem, ativo FROM tamanhos WHERE ativo = TRUE ORDER BY ordem, nome");
    }

    public async Task<int> InserirAsync(TamanhoDto dto)
    {
        using var conn = _factory.CreateConnection();
        var id = await ProximoIdAsync();
        await conn.ExecuteAsync(
            "INSERT INTO tamanhos (id, nome, ordem, ativo) VALUES (@id, @Nome, @Ordem, @Ativo)",
            new { id, dto.Nome, dto.Ordem, dto.Ativo });
        return id;
    }

    public async Task AtualizarAsync(TamanhoDto dto)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE tamanhos SET nome = @Nome, ordem = @Ordem WHERE id = @IdOriginal",
            new { dto.Nome, dto.Ordem, dto.IdOriginal });
    }

    public Task AlterarStatusAsync(int id, bool ativo)
        => AlterarStatusBaseAsync(id, ativo);

    public async Task<bool> ExisteNomeAsync(string nome, int? idIgnorar = null)
    {
        using var conn = _factory.CreateConnection();
        var sql = idIgnorar.HasValue
            ? "SELECT COUNT(*) FROM tamanhos WHERE nome = @nome AND id <> @idIgnorar"
            : "SELECT COUNT(*) FROM tamanhos WHERE nome = @nome";
        return await conn.ExecuteScalarAsync<int>(sql, new { nome, idIgnorar }) > 0;
    }
}