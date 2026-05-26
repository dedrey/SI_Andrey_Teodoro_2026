using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Models;

namespace SI_Andrey_Teodoro_2026.Repositories.Interfaces;

public interface IFornecedorRepository
{
    Task<PaginacaoDto<FornecedorListDto>> ObterTodosAsync(FiltroConsultaDto filtro);
    Task<Fornecedor?> ObterPorIdAsync(int id);
    Task<int> InserirAsync(FornecedorDto dto);
    Task AtualizarAsync(FornecedorDto dto);
    Task AlterarStatusAsync(int id, bool ativo);
    Task<bool> ExisteCnpjAsync(string cnpj, int? idOriginalIgnorar = null);
    Task<bool> ExisteRazaoSocialAsync(string razaoSocial, int? idOriginalIgnorar = null);
    Task<bool> ExisteNomeFantasiaAsync(string nomeFantasia, int? idOriginalIgnorar = null);
}