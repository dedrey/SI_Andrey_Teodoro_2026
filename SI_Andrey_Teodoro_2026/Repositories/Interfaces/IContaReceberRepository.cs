using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Models;
namespace SI_Andrey_Teodoro_2026.Repositories.Interfaces;

public interface IContaReceberRepository
{
    Task<PaginacaoDto<ContaReceberListDto>> ObterTodosAsync(FiltroConsultaDto filtro);
    Task<PaginacaoDto<ContaReceberVendaGrupoListDto>> ObterTodosAgrupadosAsync(FiltroConsultaDto filtro);
    Task<ContaReceber?> ObterPorIdAsync(int id);
    Task<List<ContaReceberBaixaDto>> ObterBaixasAsync(int contaReceberId);
    Task<int> InserirAsync(ContaReceberDto dto);
    Task AtualizarAsync(ContaReceberDto dto);
    Task AtualizarStatusAsync(int id, string status);

    Task<int> RegistrarBaixaAsync(int contaReceberId, DateTime dataRecebimento, decimal valorRecebido,
        string? comprovanteArquivo, string? observacao);

    Task RemoverBaixaAsync(int baixaId, int contaReceberId);
    Task AtualizarComprovanteBaixaAsync(int baixaId, string comprovanteArquivo);
    Task<List<ContaReceberResumoVendaDto>> ObterContasDaVendaAsync(int vendaId);
}