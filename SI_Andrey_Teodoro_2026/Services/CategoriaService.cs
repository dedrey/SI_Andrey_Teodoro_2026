using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Repositories.Interfaces;
using SI_Andrey_Teodoro_2026.Services.Interfaces;

namespace SI_Andrey_Teodoro_2026.Services;

public class CategoriaService : ICategoriaService
{
    private readonly ICategoriaRepository _repo;
    public CategoriaService(ICategoriaRepository repo) => _repo = repo;

    public Task<PaginacaoDto<CategoriaListDto>> ObterTodosAsync(FiltroConsultaDto filtro)
        => _repo.ObterTodosAsync(filtro);

    public Task<IEnumerable<CategoriaListDto>> ObterTodosAtivosAsync()
        => _repo.ObterTodosAtivosAsync();

    public async Task<CategoriaDto?> ObterPorIdAsync(int id)
    {
        var c = await _repo.ObterPorIdAsync(id);
        if (c == null) return null;
        return new CategoriaDto
        {
            Id = c.Id,
            IdOriginal = c.Id,
            NomeCategoria = c.NomeCategoria,
            Ativo = c.Ativo,
            AtualizadoEm = c.AtualizadoEm,
            NomeAtualizadoPor = c.NomeAtualizadoPor
        };
    }

    public async Task<(bool sucesso, string mensagem, int id)> SalvarAsync(CategoriaDto dto)
    {
        try
        {
            // Normaliza: capitaliza primeira letra, remove espaços extras
            dto.NomeCategoria = CapitalizarPrimeira(dto.NomeCategoria.Trim());

            int? ignorar = dto.IdOriginal > 0 ? dto.IdOriginal : null;

            if (await _repo.ExisteNomeAsync(dto.NomeCategoria, ignorar))
                return (false, $"Já existe uma categoria com o nome '{dto.NomeCategoria}'.", 0);

            if (dto.IdOriginal == 0)
            {
                var novoId = await _repo.InserirAsync(dto);
                return (true, "Categoria cadastrada com sucesso!", novoId);
            }
            await _repo.AtualizarAsync(dto);
            return (true, "Categoria atualizada com sucesso!", dto.Id);
        }
        catch (Exception ex) { return (false, $"Erro ao salvar categoria: {ex.Message}", 0); }
    }

    public async Task<(bool sucesso, string mensagem)> AlterarStatusAsync(int id, bool ativar)
    {
        try
        {
            await _repo.AlterarStatusAsync(id, ativar);
            return (true, $"Categoria {(ativar ? "ativada" : "desativada")} com sucesso!");
        }
        catch (Exception ex) { return (false, $"Erro ao alterar status: {ex.Message}"); }
    }

    private static string CapitalizarPrimeira(string v)
        => string.IsNullOrEmpty(v) ? v : char.ToUpper(v[0]) + v[1..];
}