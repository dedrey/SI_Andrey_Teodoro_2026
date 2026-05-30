using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Models;

namespace SI_Andrey_Teodoro_2026.Repositories.Interfaces;

public interface ITransportadoraRepository
{
    Task<PaginacaoDto<TransportadoraListDto>> ObterTodosAsync(FiltroConsultaDto filtro);
    Task<IEnumerable<TransportadoraListDto>> ObterTodosAtivosAsync();
    Task<Transportadora?> ObterPorIdAsync(int id);
    Task<int> InserirAsync(TransportadoraDto dto);
    Task AtualizarAsync(TransportadoraDto dto);
    Task AlterarStatusAsync(int id, bool ativo);
    Task<bool> ExisteCnpjAsync(string cnpj, int? idOriginalIgnorar = null);
}