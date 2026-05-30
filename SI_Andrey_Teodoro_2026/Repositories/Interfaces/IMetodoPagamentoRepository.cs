using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Models;

namespace SI_Andrey_Teodoro_2026.Repositories.Interfaces;

public interface IMetodoPagamentoRepository
{
    Task<PaginacaoDto<MetodoPagamentoListDto>> ObterTodosAsync(FiltroConsultaDto filtro);
    Task<IEnumerable<MetodoPagamentoListDto>> ObterTodosAtivosAsync();
    Task<MetodoPagamento?> ObterPorIdAsync(int id);
    Task<int> InserirAsync(MetodoPagamentoDto dto);
    Task AtualizarAsync(MetodoPagamentoDto dto);
    Task AlterarStatusAsync(int id, bool ativo);
    Task<bool> ExisteCodigoAsync(string codigo, int? idOriginalIgnorar = null);
    Task<bool> ExisteNomeAsync(string nome, int? idOriginalIgnorar = null);
}