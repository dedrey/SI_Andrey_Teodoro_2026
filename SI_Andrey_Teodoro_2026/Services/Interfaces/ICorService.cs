using SI_Andrey_Teodoro_2026.DTOs;
namespace SI_Andrey_Teodoro_2026.Services.Interfaces;

public interface ICorService
{
    Task<PaginacaoDto<CorListDto>> ObterTodosAsync(FiltroConsultaDto filtro);
    Task<IEnumerable<CorListDto>> ObterTodosAtivosAsync();
    Task<(bool sucesso, string mensagem, int id)> SalvarAsync(CorDto dto);
    Task<(bool sucesso, string mensagem)> AlterarStatusAsync(int id, bool ativar);
}