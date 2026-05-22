using SI_Andrey_Teodoro_2026.DTOs;

namespace SI_Andrey_Teodoro_2026.Services.Interfaces;

public interface IFornecedorService
{
    Task<PaginacaoDto<FornecedorListDto>> ObterTodosAsync(FiltroConsultaDto filtro);
    Task<FornecedorDto?> ObterPorIdAsync(int id);
    Task<(bool sucesso, string mensagem, int id)> SalvarAsync(FornecedorDto dto);
    Task<(bool sucesso, string mensagem)> AlterarStatusAsync(int id, bool ativar);
}