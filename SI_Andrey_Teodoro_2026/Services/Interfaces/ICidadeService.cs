using SI_Andrey_Teodoro_2026.DTOs;

namespace SI_Andrey_Teodoro_2026.Services.Interfaces;

public interface ICidadeService
{
    Task<PaginacaoDto<CidadeListDto>> ObterTodosAsync(FiltroConsultaDto filtro);
    Task<IEnumerable<CidadeListDto>> ObterPorEstadoAsync(int estadoId);
    Task<CidadeDto?> ObterPorIdAsync(int id);
    Task<(bool sucesso, string mensagem, int id)> SalvarAsync(CidadeDto dto);
    Task<(bool sucesso, string mensagem)> AlterarStatusAsync(int id, bool ativar);
}