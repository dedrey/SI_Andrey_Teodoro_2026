using SI_Andrey_Teodoro_2026.DTOs;
namespace SI_Andrey_Teodoro_2026.Services.Interfaces;

public interface ITamanhoService
{
    Task<PaginacaoDto<TamanhoListDto>> ObterTodosAsync(FiltroConsultaDto filtro);
    Task<IEnumerable<TamanhoListDto>> ObterTodosAtivosAsync();
    Task<(bool sucesso, string mensagem, int id)> SalvarAsync(TamanhoDto dto);
    Task<(bool sucesso, string mensagem)> AlterarStatusAsync(int id, bool ativar);
}