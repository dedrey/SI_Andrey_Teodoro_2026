using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Repositories.Interfaces;
using SI_Andrey_Teodoro_2026.Services.Interfaces;

namespace SI_Andrey_Teodoro_2026.Services;

public class FornecedorService : BaseService<FornecedorDto, FornecedorListDto>, IFornecedorService
{
    private readonly IFornecedorRepository _repo;

    public FornecedorService(IFornecedorRepository repo) => _repo = repo;

    protected override string NomeEntidade => "Fornecedor";

    public Task<PaginacaoDto<FornecedorListDto>> ObterTodosAsync(FiltroConsultaDto filtro)
        => _repo.ObterTodosAsync(filtro);

    public async Task<FornecedorDto?> ObterPorIdAsync(int id)
    {
        var f = await _repo.ObterPorIdAsync(id);
        if (f == null) return null;
        return new FornecedorDto
        {
            Id = f.Id,
            IdOriginal = f.Id,
            RazaoSocial = f.RazaoSocial,
            NomeFantasia = f.NomeFantasia,
            Cnpj = FormatarCnpj(f.Cnpj),
            CidadeId = f.CidadeId,
            Endereco = f.Endereco,
            Complemento = f.Complemento,
            Bairro = f.Bairro,
            Telefone = f.Telefone ?? string.Empty,
            Email = f.Email ?? string.Empty,
            Ativo = f.Ativo,
            AtualizadoEm = f.AtualizadoEm,
            NomeAtualizadoPor = f.NomeAtualizadoPor
        };
    }

    public async Task<(bool sucesso, string mensagem, int id)> SalvarAsync(FornecedorDto dto)
    {
        try
        {
            dto.RazaoSocial = dto.RazaoSocial.Trim();
            dto.NomeFantasia = dto.NomeFantasia.Trim();
            dto.Telefone = dto.Telefone?.Trim() ?? string.Empty;
            dto.Email = dto.Email?.Trim().ToLower() ?? string.Empty;
            dto.Endereco = dto.Endereco.Trim();
            dto.Bairro = dto.Bairro.Trim();

            var cnpjLimpo = LimparDigitos(dto.Cnpj);
            var erroCnpj = ValidarCnpj(cnpjLimpo);
            if (erroCnpj != null) return (false, erroCnpj, 0);
            dto.Cnpj = cnpjLimpo;

            int? ignorar = dto.IdOriginal > 0 ? dto.IdOriginal : null;

            if (await _repo.ExisteCnpjAsync(dto.Cnpj, ignorar))
                return (false, $"Já existe um fornecedor com o CNPJ '{FormatarCnpj(dto.Cnpj)}'.", 0);

            if (await _repo.ExisteRazaoSocialAsync(dto.RazaoSocial, ignorar))
                return (false, $"Já existe um fornecedor com a razão social '{dto.RazaoSocial}'.", 0);

            if (await _repo.ExisteNomeFantasiaAsync(dto.NomeFantasia, ignorar))
                return (false, $"Já existe um fornecedor com o nome fantasia '{dto.NomeFantasia}'.", 0);

            if (dto.IdOriginal == 0)
            {
                var novoId = await _repo.InserirAsync(dto);
                return (true, "Fornecedor cadastrado com sucesso!", novoId);
            }

            await _repo.AtualizarAsync(dto);
            return (true, "Fornecedor atualizado com sucesso!", dto.Id);
        }
        catch (Exception ex) { return (false, Erro(ex).mensagem, 0); }
    }

    public async Task<(bool sucesso, string mensagem)> AlterarStatusAsync(int id, bool ativar)
    {
        try
        {
            await _repo.AlterarStatusAsync(id, ativar);
            return SucessoStatus(ativar);
        }
        catch (Exception ex) { return ErroStatus(ex); }
    }

    private static string LimparDigitos(string v)
        => new string(v.Where(char.IsDigit).ToArray());

    public static string FormatarCnpj(string cnpj)
    {
        var d = LimparDigitos(cnpj);
        return d.Length == 14 ? $"{d[..2]}.{d[2..5]}.{d[5..8]}/{d[8..12]}-{d[12..]}" : cnpj;
    }

    private static string? ValidarCnpj(string cnpj)
    {
        if (cnpj.Length != 14 || cnpj.Distinct().Count() == 1) return "CNPJ inválido.";
        int[] m1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] m2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        return Digito(cnpj, m1, 12) && Digito(cnpj, m2, 13) ? null : "CNPJ inválido.";
    }

    private static bool Digito(string d, int[] m, int pos)
    {
        var s = m.Select((v, i) => (d[i] - '0') * v).Sum();
        var r = s % 11;
        return (d[pos] - '0') == (r < 2 ? 0 : 11 - r);
    }
}