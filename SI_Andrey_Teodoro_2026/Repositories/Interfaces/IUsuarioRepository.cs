using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Models;

namespace SI_Andrey_Teodoro_2026.Repositories.Interfaces;

public interface IUsuarioRepository
{
    Task<PaginacaoDto<UsuarioListDto>> ObterTodosAsync(FiltroConsultaDto filtro);
    Task<IEnumerable<UsuarioListDto>> ObterTodosAtivosAsync();
    Task<Usuario?> ObterPorIdAsync(int id);
    Task<int> InserirAsync(UsuarioDto dto);
    Task AtualizarAsync(UsuarioDto dto);
    Task AlterarStatusAsync(int id, bool ativo);
    Task<bool> ExisteCpfAsync(string cpf, int? idOriginalIgnorar = null);
    Task<bool> ExisteEmailAsync(string email, int? idOriginalIgnorar = null);
}