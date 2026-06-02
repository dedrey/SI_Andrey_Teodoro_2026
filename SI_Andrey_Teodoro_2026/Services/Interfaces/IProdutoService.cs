using SI_Andrey_Teodoro_2026.DTOs;

namespace SI_Andrey_Teodoro_2026.Services.Interfaces;

public interface IProdutoService
{
    Task<PaginacaoDto<ProdutoListDto>> ObterTodosAsync(FiltroConsultaDto filtro);
    Task<IEnumerable<ProdutoListDto>> ObterTodosAtivosAsync();
    Task<ProdutoDto?> ObterPorIdAsync(int id);
    Task<(bool sucesso, string mensagem, int id)> SalvarAsync(ProdutoDto dto);
    Task<(bool sucesso, string mensagem)> AlterarStatusAsync(int id, bool ativar);
    Task<(bool sucesso, string mensagem)> AlterarStatusVariacaoAsync(int id, bool ativar);
    Task<(bool sucesso, string mensagem)> AtualizarEstoqueAsync(int variacaoId, int quantidade);
}