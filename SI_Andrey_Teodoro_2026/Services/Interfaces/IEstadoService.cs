using SI_Andrey_Teodoro_2026.DTOs;

namespace SI_Andrey_Teodoro_2026.Services.Interfaces;

public interface IEstadoService
{
    Task<PaginacaoDto<EstadoListDto>> ObterTodosAsync(FiltroConsultaDto filtro);
    Task<IEnumerable<EstadoListDto>> ObterPorPaisAsync(int paisId);
    Task<EstadoDto?> ObterPorIdAsync(int id);
    Task<(bool sucesso, string mensagem, int id)> SalvarAsync(EstadoDto dto);
    Task<(bool sucesso, string mensagem)> AlterarStatusAsync(int id, bool ativar);
}