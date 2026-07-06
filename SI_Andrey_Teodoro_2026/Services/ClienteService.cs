using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Repositories.Interfaces;
using SI_Andrey_Teodoro_2026.Services.Interfaces;

namespace SI_Andrey_Teodoro_2026.Services;

public class ClienteService : BaseService<ClienteDto, ClienteListDto>, IClienteService
{
    private readonly IClienteRepository _repo;

    public ClienteService(IClienteRepository repo) => _repo = repo;

    protected override string NomeEntidade => "Cliente";

    public Task<PaginacaoDto<ClienteListDto>> ObterTodosAsync(FiltroConsultaDto filtro)
        => _repo.ObterTodosAsync(filtro);

    public Task<IEnumerable<ClienteListDto>> ObterTodosAtivosAsync()
        => _repo.ObterTodosAtivosAsync();

    public async Task<ClienteDto?> ObterPorIdAsync(int id)
    {
        var c = await _repo.ObterPorIdAsync(id);
        if (c == null) return null;
        return new ClienteDto
        {
            Id = c.Id,
            IdOriginal = c.Id,
            TipoPessoa = c.TipoPessoa,
            Estrangeiro = c.Estrangeiro,
            NomeRazaoSocial = c.NomeRazaoSocial,
            Sexo = c.Sexo,
            ApelidoNomeFantasia = c.ApelidoNomeFantasia,
            CpfCnpj = FormatarDocumento(c.CpfCnpj, c.TipoPessoa),
            DocumentoEstrangeiro = c.DocumentoEstrangeiro,
            NomeCidade = c.NomeCidade,
            CidadeId = c.CidadeId,
            Cep = c.Cep ?? string.Empty,
            Endereco = c.Endereco,
            Numero = c.Numero ?? string.Empty,
            Complemento = c.Complemento ?? string.Empty,
            Bairro = c.Bairro,
            Telefone = c.Telefone ?? string.Empty,
            Celular = c.Celular,
            Email = c.Email ?? string.Empty,
            InscricaoEstadual = c.InscricaoEstadual,
            InscricaoMunicipal = c.InscricaoMunicipal,
            LimiteCredito = c.LimiteCredito,
            Ativo = c.Ativo,
            AtualizadoEm = c.AtualizadoEm,
            NomeAtualizadoPor = c.NomeAtualizadoPor
        };
    }

    public async Task<(bool sucesso, string mensagem, int id)> SalvarAsync(ClienteDto dto)
    {
        try
        {
            dto.NomeRazaoSocial = dto.NomeRazaoSocial.Trim();
            dto.ApelidoNomeFantasia = dto.ApelidoNomeFantasia?.Trim();
            dto.Telefone = dto.Telefone?.Trim() ?? string.Empty;
            dto.Celular = dto.Celular?.Trim();
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

            if (dto.TipoPessoa == "PJ" && string.IsNullOrWhiteSpace(dto.Celular))
                return (false, "Celular é obrigatório para Pessoa Jurídica.", 0);
            if (dto.TipoPessoa == "PF")
                dto.Celular = null;

            if (!dto.Estrangeiro && !string.IsNullOrWhiteSpace(dto.CpfCnpj))
            {
                var docLimpo = LimparDigitos(dto.CpfCnpj);
                string? erroDoc = dto.TipoPessoa == "PF"
                    ? ValidarCpf(docLimpo)
                    : ValidarCnpj(docLimpo);
                if (erroDoc != null) return (false, erroDoc, 0);
                dto.CpfCnpj = docLimpo;
            }

            int? ignorar = dto.IdOriginal > 0 ? dto.IdOriginal : null;

            if (!dto.Estrangeiro && !string.IsNullOrWhiteSpace(dto.CpfCnpj))
            {
                if (await _repo.ExisteDocumentoAsync(dto.CpfCnpj, ignorar))
                    return (false, $"Já existe um cliente com este documento.", 0);
            }

            if (dto.IdOriginal == 0)
            {
                var novoId = await _repo.InserirAsync(dto);
                return (true, "Cliente cadastrado com sucesso!", novoId);
            }

            await _repo.AtualizarAsync(dto);
            return (true, "Cliente atualizado com sucesso!", dto.Id);
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