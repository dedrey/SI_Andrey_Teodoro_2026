using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Repositories.Interfaces;
using SI_Andrey_Teodoro_2026.Services.Interfaces;

namespace SI_Andrey_Teodoro_2026.Services;

public class CorService : BaseService<CorDto, CorListDto>, ICorService
{
    private readonly ICorRepository _repo;
    public CorService(ICorRepository repo) => _repo = repo;
    protected override string NomeEntidade => "Cor";

    public Task<PaginacaoDto<CorListDto>> ObterTodosAsync(FiltroConsultaDto filtro)
        => _repo.ObterTodosAsync(filtro);

    public Task<IEnumerable<CorListDto>> ObterTodosAtivosAsync()
        => _repo.ObterTodosAtivosAsync();

    public async Task<(bool sucesso, string mensagem, int id)> SalvarAsync(CorDto dto)
    {
        try
        {
            dto.Nome = dto.Nome.Trim();
            if (string.IsNullOrWhiteSpace(dto.Nome))
                return (false, "Nome da cor é obrigatório.", 0);

            int? ignorar = dto.IdOriginal > 0 ? dto.IdOriginal : null;
            if (await _repo.ExisteNomeAsync(dto.Nome, ignorar))
                return (false, $"Já existe uma cor com o nome '{dto.Nome}'.", 0);

            if (dto.IdOriginal == 0)
            {
                var id = await _repo.InserirAsync(dto);
                return (true, "Cor cadastrada com sucesso!", id);
            }
            await _repo.AtualizarAsync(dto);
            return (true, "Cor atualizada com sucesso!", dto.Id);
        }
        catch (Exception ex) { return (false, Erro(ex).mensagem, 0); }
    }

    public async Task<(bool sucesso, string mensagem)> AlterarStatusAsync(int id, bool ativar)
    {
        try { await _repo.AlterarStatusAsync(id, ativar); return SucessoStatus(ativar); }
        catch (Exception ex) { return ErroStatus(ex); }
    }
}