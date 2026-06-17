using SI_Andrey_Teodoro_2026.DTOs;
namespace SI_Andrey_Teodoro_2026.Services.Interfaces;

public interface IContaPagarService
{
    Task<PaginacaoDto<ContaPagarListDto>> ObterTodosAsync(FiltroConsultaDto filtro);
    Task<ContaPagarDto?> ObterPorIdAsync(int id);
    Task<(bool sucesso, string mensagem, int id)> SalvarAsync(ContaPagarDto dto);
    Task<(bool sucesso, string mensagem)> MarcarComoPagaAsync(int id, DateTime dataPagamento, string? comprovanteArquivo = null);
    Task<(bool sucesso, string mensagem)> CancelarAsync(int id);
    Task GerarContaAutomaticaAsync(int? fornecedorId, int movimentacaoId, string numeroNf,
        DateTime dataEntrada, int diasPrazo, decimal valorTotal);
}