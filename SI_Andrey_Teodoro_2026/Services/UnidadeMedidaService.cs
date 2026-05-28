using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Repositories.Interfaces;
using SI_Andrey_Teodoro_2026.Services.Interfaces;

namespace SI_Andrey_Teodoro_2026.Services;

public class UnidadeMedidaService : IUnidadeMedidaService
{
    private readonly IUnidadeMedidaRepository _repo;
    public UnidadeMedidaService(IUnidadeMedidaRepository repo) => _repo = repo;

    public Task<PaginacaoDto<UnidadeMedidaListDto>> ObterTodosAsync(FiltroConsultaDto filtro)
        => _repo.ObterTodosAsync(filtro);

    public Task<IEnumerable<UnidadeMedidaListDto>> ObterTodosAtivosAsync()
        => _repo.ObterTodosAtivosAsync();

    public async Task<UnidadeMedidaDto?> ObterPorIdAsync(int id)
    {
        var u = await _repo.ObterPorIdAsync(id);
        if (u == null) return null;
        return new UnidadeMedidaDto
        {
            Id = u.Id,
            IdOriginal = u.Id,
            Sigla = u.Sigla,
            Descricao = u.Descricao,
            Ativo = u.Ativo,
            AtualizadoEm = u.AtualizadoEm,
            NomeAtualizadoPor = u.NomeAtualizadoPor
        };
    }

    public async Task<(bool sucesso, string mensagem, int id)> SalvarAsync(UnidadeMedidaDto dto)
    {
        try
        {
            dto.Sigla = dto.Sigla.Trim().ToUpper();
            dto.Descricao = CapitalizarPrimeira(dto.Descricao.Trim());

            int? ignorar = dto.IdOriginal > 0 ? dto.IdOriginal : null;

            if (await _repo.ExisteSiglaAsync(dto.Sigla, ignorar))
                return (false, $"Já existe uma unidade de medida com a sigla '{dto.Sigla}'.", 0);

            if (dto.IdOriginal == 0)
            {
                var novoId = await _repo.InserirAsync(dto);
                return (true, "Unidade de medida cadastrada com sucesso!", novoId);
            }
            await _repo.AtualizarAsync(dto);
            return (true, "Unidade de medida atualizada com sucesso!", dto.Id);
        }
        catch (Exception ex) { return (false, $"Erro ao salvar unidade de medida: {ex.Message}", 0); }
    }

    public async Task<(bool sucesso, string mensagem)> AlterarStatusAsync(int id, bool ativar)
    {
        try
        {
            await _repo.AlterarStatusAsync(id, ativar);
            return (true, $"Unidade de medida {(ativar ? "ativada" : "desativada")} com sucesso!");
        }
        catch (Exception ex) { return (false, $"Erro ao alterar status: {ex.Message}"); }
    }

    private static string CapitalizarPrimeira(string v)
        => string.IsNullOrEmpty(v) ? v : char.ToUpper(v[0]) + v[1..];
}