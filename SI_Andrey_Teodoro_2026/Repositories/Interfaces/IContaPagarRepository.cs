using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Models;
namespace SI_Andrey_Teodoro_2026.Repositories.Interfaces;

public interface IContaPagarRepository
{
    Task<PaginacaoDto<ContaPagarListDto>> ObterTodosAsync(FiltroConsultaDto filtro);
    Task<ContaPagar?> ObterPorIdAsync(int id);
    Task<int> InserirAsync(ContaPagarDto dto);
    Task AtualizarAsync(ContaPagarDto dto);
    Task AtualizarStatusAsync(int id, string status, DateTime? dataPagamento = null, string? comprovanteArquivo = null);

    Task<int> InserirAutomaticaAsync(int? fornecedorId, int movimentacaoId, string descricao,
        DateTime dataVencimento, decimal valorOriginal);
}