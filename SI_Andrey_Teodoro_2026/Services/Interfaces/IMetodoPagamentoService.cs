using SI_Andrey_Teodoro_2026.DTOs;

namespace SI_Andrey_Teodoro_2026.Services.Interfaces;

public interface IMetodoPagamentoService
{
    Task<PaginacaoDto<MetodoPagamentoListDto>> ObterTodosAsync(FiltroConsultaDto filtro);
    Task<IEnumerable<MetodoPagamentoListDto>> ObterTodosAtivosAsync();
    Task<MetodoPagamentoDto?> ObterPorIdAsync(int id);
    Task<(bool sucesso, string mensagem, int id)> SalvarAsync(MetodoPagamentoDto dto);
    Task<(bool sucesso, string mensagem)> AlterarStatusAsync(int id, bool ativar);
}