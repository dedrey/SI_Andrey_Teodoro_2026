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
    public Task<IEnumerable<FornecedorListDto>> ObterTodosAtivosAsync()
    => _repo.ObterTodosAtivosAsync();

    public async Task<FornecedorDto?> ObterPorIdAsync(int id)
    {
        var f = await _repo.ObterPorIdAsync(id);
        if (f == null) return null;
        return new FornecedorDto
        {
            Id = f.Id,
            IdOriginal = f.Id,
            RazaoSocial = f.RazaoSocial,
            TipoPessoa = f.TipoPessoa,
            NomeFantasia = f.NomeFantasia,
            CpfCnpj = FormatarDocumento(f.CpfCnpj, f.TipoPessoa),
            NomeCidade = f.NomeCidade,
            CidadeId = f.CidadeId,
            Cep = f.Cep ?? string.Empty,
            Endereco = f.Endereco,
            Numero = f.Numero ?? string.Empty,
            Complemento = f.Complemento ?? string.Empty,
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
            dto.NomeFantasia = dto.NomeFantasia?.Trim();
            dto.Telefone = dto.Telefone?.Trim() ?? string.Empty;
            dto.Email = dto.Email?.Trim() ?? string.Empty;
            if (!System.Text.RegularExpressions.Regex.IsMatch(dto.Email, @"^[^@\s]+@[^@\s]+\.[A-Za-z]{2,}$"))
                return (false, "E-mail inválido.", 0);
            dto.Endereco = dto.Endereco.Trim();
            dto.Numero = dto.Numero?.Trim().ToUpper() ?? string.Empty;
            dto.Bairro = dto.Bairro.Trim();
            dto.Complemento = dto.Complemento.Trim();
            dto.Cep = dto.Cep.Trim();

            if (!dto.CidadeId.HasValue)
                return (false, "Cidade é obrigatória.", 0);

            if (dto.TipoPessoa == "PJ" && string.IsNullOrWhiteSpace(dto.NomeFantasia))
                return (false, "Nome Fantasia é obrigatório para Pessoa Jurídica.", 0);
            if (dto.TipoPessoa == "PF")
                dto.NomeFantasia = null;

            var docLimpo = LimparDigitos(dto.CpfCnpj);
            var erroDoc = dto.TipoPessoa == "PF" ? ValidarCpf(docLimpo) : ValidarCnpj(docLimpo);
            if (erroDoc != null) return (false, erroDoc, 0);
            dto.CpfCnpj = docLimpo;

            int? ignorar = dto.IdOriginal > 0 ? dto.IdOriginal : null;

            if (await _repo.ExisteCpfCnpjAsync(dto.CpfCnpj, ignorar))
                return (false, $"Já existe um fornecedor com o documento '{FormatarDocumento(dto.CpfCnpj, dto.TipoPessoa)}'.", 0);

            if (await _repo.ExisteRazaoSocialAsync(dto.RazaoSocial, ignorar))
                return (false, $"Já existe um fornecedor com o nome/razão social '{dto.RazaoSocial}'.", 0);

            if (!string.IsNullOrWhiteSpace(dto.NomeFantasia) && await _repo.ExisteNomeFantasiaAsync(dto.NomeFantasia, ignorar))
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

    public static string FormatarDocumento(string? doc, string tipoPessoa)
    {
        if (string.IsNullOrWhiteSpace(doc)) return string.Empty;
        var d = LimparDigitos(doc);
        if (tipoPessoa == "PF" && d.Length == 11)
            return $"{d[..3]}.{d[3..6]}.{d[6..9]}-{d[9..]}";
        if (tipoPessoa == "PJ" && d.Length == 14)
            return $"{d[..2]}.{d[2..5]}.{d[5..8]}/{d[8..12]}-{d[12..]}";
        return doc;
    }

    private static string? ValidarCpf(string cpf)
    {
        if (cpf.Length != 11 || cpf.Distinct().Count() == 1) return "CPF inválido.";
        int[] m1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] m2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        return Digito(cpf, m1, 9) && Digito(cpf, m2, 10) ? null : "CPF inválido.";
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