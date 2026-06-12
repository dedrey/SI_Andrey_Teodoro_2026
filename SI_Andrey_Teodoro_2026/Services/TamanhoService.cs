using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Repositories.Interfaces;
using SI_Andrey_Teodoro_2026.Services.Interfaces;

namespace SI_Andrey_Teodoro_2026.Services;

public class TamanhoService : BaseService<TamanhoDto, TamanhoListDto>, ITamanhoService
{
    private readonly ITamanhoRepository _repo;
    public TamanhoService(ITamanhoRepository repo) => _repo = repo;
    protected override string NomeEntidade => "Tamanho";

    public Task<PaginacaoDto<TamanhoListDto>> ObterTodosAsync(FiltroConsultaDto filtro)
        => _repo.ObterTodosAsync(filtro);

    public Task<IEnumerable<TamanhoListDto>> ObterTodosAtivosAsync()
        => _repo.ObterTodosAtivosAsync();

    public async Task<(bool sucesso, string mensagem, int id)> SalvarAsync(TamanhoDto dto)
    {
        try
        {
            dto.Nome = dto.Nome.Trim().ToUpper();
            if (string.IsNullOrWhiteSpace(dto.Nome))
                return (false, "Nome do tamanho é obrigatório.", 0);

            int? ignorar = dto.IdOriginal > 0 ? dto.IdOriginal : null;
            if (await _repo.ExisteNomeAsync(dto.Nome, ignorar))
                return (false, $"Já existe um tamanho '{dto.Nome}'.", 0);

            if (dto.IdOriginal == 0)
            {
                var id = await _repo.InserirAsync(dto);
                return (true, "Tamanho cadastrado com sucesso!", id);
            }
            await _repo.AtualizarAsync(dto);
            return (true, "Tamanho atualizado com sucesso!", dto.Id);
        }
        catch (Exception ex) { return (false, Erro(ex).mensagem, 0); }
    }

    public async Task<(bool sucesso, string mensagem)> AlterarStatusAsync(int id, bool ativar)
    {
        try { await _repo.AlterarStatusAsync(id, ativar); return SucessoStatus(ativar); }
        catch (Exception ex) { return ErroStatus(ex); }
    }
}