using SI_Andrey_Teodoro_2026.DTOs;

namespace SI_Andrey_Teodoro_2026.Services.Interfaces;

public interface IUsuarioService
{
    Task<PaginacaoDto<UsuarioListDto>> ObterTodosAsync(FiltroConsultaDto filtro);
    Task<IEnumerable<UsuarioListDto>> ObterTodosAtivosAsync();
    Task<UsuarioDto?> ObterPorIdAsync(int id);
    Task<(bool sucesso, string mensagem, int id)> SalvarAsync(UsuarioDto dto);
    Task<(bool sucesso, string mensagem)> AlterarStatusAsync(int id, bool ativar);
}