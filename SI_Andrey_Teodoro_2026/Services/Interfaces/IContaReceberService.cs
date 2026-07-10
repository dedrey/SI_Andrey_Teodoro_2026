using SI_Andrey_Teodoro_2026.DTOs;
namespace SI_Andrey_Teodoro_2026.Services.Interfaces;

public interface IContaReceberService
{
    Task<PaginacaoDto<ContaReceberListDto>> ObterTodosAsync(FiltroConsultaDto filtro);
    Task<PaginacaoDto<ContaReceberVendaGrupoListDto>> ObterTodosAgrupadosAsync(FiltroConsultaDto filtro);
    Task<ContaReceberDto?> ObterPorIdAsync(int id);
    Task<(bool sucesso, string mensagem, int id)> SalvarAsync(ContaReceberDto dto);
    Task<(bool sucesso, string mensagem)> RegistrarRecebimentoAsync(int contaReceberId, DateTime dataRecebimento,
        decimal valorRecebido, string? comprovanteArquivo, string? observacao);
    Task<(bool sucesso, string mensagem)> CancelarAsync(int id);
    Task<(bool sucesso, string mensagem)> AnexarComprovanteAsync(int baixaId, string comprovanteArquivo);
    Task<List<ContaReceberResumoVendaDto>> ObterContasDaVendaAsync(int vendaId);
}