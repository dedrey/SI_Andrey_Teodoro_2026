using SI_Andrey_Teodoro_2026.DTOs;

namespace SI_Andrey_Teodoro_2026.Services.Interfaces;

public interface IEmitenteService
{
    Task<PaginacaoDto<EmitenteListDto>> ObterTodosAsync(FiltroConsultaDto filtro);
    Task<IEnumerable<EmitenteListDto>> ObterTodosAtivosAsync();
    Task<EmitenteDto?> ObterPorIdAsync(int id);
    Task<(bool sucesso, string mensagem, int id)> SalvarAsync(EmitenteDto dto);
    Task<(bool sucesso, string mensagem)> AlterarStatusAsync(int id, bool ativar);
}