using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Repositories.Interfaces;
using SI_Andrey_Teodoro_2026.Services.Interfaces;

namespace SI_Andrey_Teodoro_2026.Services;

public class EstadoService : BaseService<EstadoDto, EstadoListDto>, IEstadoService
{
    private readonly IEstadoRepository _repo;

    public EstadoService(IEstadoRepository repo) => _repo = repo;

    protected override string NomeEntidade => "Estado";

    public Task<PaginacaoDto<EstadoListDto>> ObterTodosAsync(FiltroConsultaDto filtro)
        => _repo.ObterTodosAsync(filtro);

    public Task<IEnumerable<EstadoListDto>> ObterPorPaisAsync(int paisId)
        => _repo.ObterPorPaisAsync(paisId);

    public Task<IEnumerable<EstadoListDto>> ObterTodosAtivosAsync()
        => _repo.ObterTodosAtivosAsync();

    public async Task<EstadoDto?> ObterPorIdAsync(int id)
    {
        var e = await _repo.ObterPorIdAsync(id);
        if (e == null) return null;
        return new EstadoDto
        {
            Id = e.Id,
            IdOriginal = e.Id,
            PaisId = e.PaisId,
            NomeEstado = e.NomeEstado,
            Uf = e.Uf,
            Ativo = e.Ativo,
            AtualizadoEm = e.AtualizadoEm,
            NomeAtualizadoPor = e.NomeAtualizadoPor
        };
    }

    public async Task<(bool sucesso, string mensagem, int id)> SalvarAsync(EstadoDto dto)
    {
        try
        {
            dto.Uf = dto.Uf.ToUpper().Trim();
            dto.NomeEstado = dto.NomeEstado.Trim();

            int? ignorar = dto.IdOriginal > 0 ? dto.IdOriginal : null;

            if (await _repo.ExisteUfNoPaisAsync(dto.Uf, dto.PaisId, ignorar))
                return (false, $"Já existe um estado com a UF '{dto.Uf}' neste país.", 0);

            if (dto.IdOriginal == 0)
            {
                var novoId = await _repo.InserirAsync(dto);
                return (true, "Estado cadastrado com sucesso!", novoId);
            }

            await _repo.AtualizarAsync(dto);
            return (true, "Estado atualizado com sucesso!", dto.Id);
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
}