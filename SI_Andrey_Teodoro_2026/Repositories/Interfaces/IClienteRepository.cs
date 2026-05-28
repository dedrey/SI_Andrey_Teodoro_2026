using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Models;

namespace SI_Andrey_Teodoro_2026.Repositories.Interfaces;

public interface IClienteRepository
{
    Task<PaginacaoDto<ClienteListDto>> ObterTodosAsync(FiltroConsultaDto filtro);
    Task<IEnumerable<ClienteListDto>> ObterTodosAtivosAsync();
    Task<Cliente?> ObterPorIdAsync(int id);
    Task<int> InserirAsync(ClienteDto dto);
    Task AtualizarAsync(ClienteDto dto);
    Task AlterarStatusAsync(int id, bool ativo);
    Task<bool> ExisteDocumentoAsync(string documento, int? idOriginalIgnorar = null);
}