using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Models;

namespace SI_Andrey_Teodoro_2026.Repositories.Interfaces;

public interface IVeiculoRepository
{
    Task<PaginacaoDto<VeiculoListDto>> ObterTodosAsync(FiltroConsultaDto filtro);
    Task<IEnumerable<VeiculoListDto>> ObterTodosAtivosAsync();
    Task<Veiculo?> ObterPorIdAsync(int id);
    Task<int> InserirAsync(VeiculoDto dto);
    Task AtualizarAsync(VeiculoDto dto);
    Task AlterarStatusAsync(int id, bool ativo);
    Task<bool> ExistePlacaAsync(string placa, int? idOriginalIgnorar = null);
}