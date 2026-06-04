using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Repositories.Interfaces;
using SI_Andrey_Teodoro_2026.Services.Interfaces;
namespace SI_Andrey_Teodoro_2026.Services;
public class MarcaService : BaseService<MarcaDto, MarcaListDto>, IMarcaService
{
    private readonly IMarcaRepository _repo;
    public MarcaService(IMarcaRepository repo) => _repo = repo;
    protected override string NomeEntidade => "Marca";
    public Task<PaginacaoDto<MarcaListDto>> ObterTodosAsync(FiltroConsultaDto filtro)
        => _repo.ObterTodosAsync(filtro);
    public Task<IEnumerable<MarcaListDto>> ObterTodosAtivosAsync()
        => _repo.ObterTodosAtivosAsync();
    public async Task<MarcaDto?> ObterPorIdAsync(int id)
    {
        var m = await _repo.ObterPorIdAsync(id);
        if (m == null) return null;
        return new MarcaDto
        {
            Id = m.Id,
            IdOriginal = m.Id,
            NomeMarca = m.NomeMarca,
            Ativo = m.Ativo,
            AtualizadoEm = m.AtualizadoEm,
            NomeAtualizadoPor = m.NomeAtualizadoPor
        };
    }
    public async Task<(bool sucesso, string mensagem, int id)> SalvarAsync(MarcaDto dto)
    {
        try
        {
            dto.NomeMarca = CapitalizarPrimeira(dto.NomeMarca.Trim());

            int? ignorar = dto.IdOriginal > 0 ? dto.IdOriginal : null;

            if (await _repo.ExisteNomeAsync(dto.NomeMarca, ignorar))
                return (false, $"Já existe uma marca com o nome '{dto.NomeMarca}'.", 0);

            if (dto.IdOriginal == 0)
            {
                var novoId = await _repo.InserirAsync(dto);
                return (true, "Marca cadastrada com sucesso!", novoId);
            }

            await _repo.AtualizarAsync(dto);
            return (true, "Marca atualizada com sucesso!", dto.Id);
        }
        catch (Exception ex) { return (false, Erro(ex).mensagem, 0); }
    }
    public async Task<(bool sucesso, string mensagem)> AlterarStatusAsync(int id, bool ativar)
    {
        try
        {
            await _repo.AlterarStatusAsync(id, ativar);
            return SucessoStatus(ativar);
        }
        catch (Exception ex) { return ErroStatus(ex); }
    }
    private static string CapitalizarPrimeira(string v)
        => string.IsNullOrEmpty(v) ? v : char.ToUpper(v[0]) + v[1..];
}