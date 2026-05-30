using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Models;

namespace SI_Andrey_Teodoro_2026.Repositories.Interfaces;

public interface IEmitenteRepository
{
    Task<PaginacaoDto<EmitenteListDto>> ObterTodosAsync(FiltroConsultaDto filtro);
    Task<IEnumerable<EmitenteListDto>> ObterTodosAtivosAsync();
    Task<Emitente?> ObterPorIdAsync(int id);
    Task<int> InserirAsync(EmitenteDto dto);
    Task AtualizarAsync(EmitenteDto dto);
    Task AlterarStatusAsync(int id, bool ativo);
    Task<bool> ExisteCnpjAsync(string cnpj, int? idOriginalIgnorar = null);
}