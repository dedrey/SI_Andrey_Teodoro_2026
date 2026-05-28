using SI_Andrey_Teodoro_2026.DTOs;

namespace SI_Andrey_Teodoro_2026.Services.Interfaces;

public interface IClienteService
{
    Task<PaginacaoDto<ClienteListDto>> ObterTodosAsync(FiltroConsultaDto filtro);
    Task<IEnumerable<ClienteListDto>> ObterTodosAtivosAsync();
    Task<ClienteDto?> ObterPorIdAsync(int id);
    Task<(bool sucesso, string mensagem, int id)> SalvarAsync(ClienteDto dto);
    Task<(bool sucesso, string mensagem)> AlterarStatusAsync(int id, bool ativar);
}