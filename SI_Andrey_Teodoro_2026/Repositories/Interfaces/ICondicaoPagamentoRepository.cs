using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Models;

namespace SI_Andrey_Teodoro_2026.Repositories.Interfaces;

public interface ICondicaoPagamentoRepository
{
    Task<PaginacaoDto<CondicaoPagamentoListDto>> ObterTodosAsync(FiltroConsultaDto filtro);
    Task<IEnumerable<CondicaoPagamentoListDto>> ObterTodosAtivosAsync();
    Task<CondicaoPagamento?> ObterPorIdAsync(int id);
    Task<int> InserirAsync(CondicaoPagamentoDto dto);
    Task AtualizarAsync(CondicaoPagamentoDto dto);
    Task AlterarStatusAsync(int id, bool ativo);
    Task<bool> ExisteNomeAsync(string nome, int? idOriginalIgnorar = null);
    Task<List<CondicaoPagamentoParcelaDto>> ObterParcelasAsync(int condicaoId);

}