using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Repositories.Interfaces;
using SI_Andrey_Teodoro_2026.Services.Interfaces;

namespace SI_Andrey_Teodoro_2026.Services;

public class ClienteService : IClienteService
{
    private readonly IClienteRepository _repo;
    public ClienteService(IClienteRepository repo) => _repo = repo;

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
            NomeRazaoSocial = c.NomeRazaoSocial,
            CpfCnpj = c.Estrangeiro ? null : FormatarDocumento(c.CpfCnpj, c.TipoPessoa),
            TipoPessoa = c.TipoPessoa,
            Estrangeiro = c.Estrangeiro,
            DocumentoEstrangeiro = c.DocumentoEstrangeiro,
            Nacionalidade = c.Nacionalidade,
            ApelidoNomeFantasia = c.ApelidoNomeFantasia,
            CidadeId = c.CidadeId,
            Endereco = c.Endereco ?? string.Empty,
            Bairro = c.Bairro ?? string.Empty,
            Telefone = c.Telefone ?? string.Empty,
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
            dto.Endereco = dto.Endereco.Trim();
            dto.Bairro = dto.Bairro.Trim();
            dto.Telefone = dto.Telefone.Trim();
            dto.Email = dto.Email.Trim().ToLower();

            int? ignorar = dto.IdOriginal > 0 ? dto.IdOriginal : null;

            if (!dto.Estrangeiro)
            {
                var docLimpo = LimparDigitos(dto.CpfCnpj ?? "");
                if (dto.TipoPessoa == "PF")
                {
                    var erroCpf = ValidarCpf(docLimpo);
                    if (erroCpf != null) return (false, erroCpf, 0);
                }
                else
                {
                    var erroCnpj = ValidarCnpj(docLimpo);
                    if (erroCnpj != null) return (false, erroCnpj, 0);
                }
                dto.CpfCnpj = docLimpo;
                dto.DocumentoEstrangeiro = null;
                dto.Nacionalidade = null;

                if (await _repo.ExisteDocumentoAsync(dto.CpfCnpj, ignorar))
                    return (false, $"Já existe um cliente com este {(dto.TipoPessoa == "PF" ? "CPF" : "CNPJ")}.", 0);
            }
            else
            {
                dto.CpfCnpj = null;
                dto.DocumentoEstrangeiro = dto.DocumentoEstrangeiro?.Trim().ToUpper();
                dto.Nacionalidade = dto.Nacionalidade?.Trim();

                if (string.IsNullOrWhiteSpace(dto.DocumentoEstrangeiro))
                    return (false, "Documento estrangeiro é obrigatório.", 0);
                if (string.IsNullOrWhiteSpace(dto.Nacionalidade))
                    return (false, "Nacionalidade é obrigatória.", 0);

                if (await _repo.ExisteDocumentoAsync(dto.DocumentoEstrangeiro, ignorar))
                    return (false, $"Já existe um cliente com o documento '{dto.DocumentoEstrangeiro}'.", 0);
            }

            if (dto.TipoPessoa == "PJ" && string.IsNullOrWhiteSpace(dto.ApelidoNomeFantasia))
                return (false, "Nome Fantasia é obrigatório para Pessoa Jurídica.", 0);

            if (dto.IdOriginal == 0)
            {
                var novoId = await _repo.InserirAsync(dto);
                return (true, "Cliente cadastrado com sucesso!", novoId);
            }
            await _repo.AtualizarAsync(dto);
            return (true, "Cliente atualizado com sucesso!", dto.Id);
        }
        catch (Exception ex) { return (false, $"Erro ao salvar cliente: {ex.Message}", 0); }
    }

    public async Task<(bool sucesso, string mensagem)> AlterarStatusAsync(int id, bool ativar)
    {
        try
        {
            await _repo.AlterarStatusAsync(id, ativar);
            return (true, $"Cliente {(ativar ? "ativado" : "desativado")} com sucesso!");
        }
        catch (Exception ex) { return (false, $"Erro ao alterar status: {ex.Message}"); }
    }
    private static string LimparDigitos(string v)
        => new string(v.Where(char.IsDigit).ToArray());

    private static string FormatarDocumento(string? doc, string tipo)
    {
        if (string.IsNullOrWhiteSpace(doc)) return "";
        var d = new string(doc.Where(char.IsDigit).ToArray());
        if (tipo == "PF" && d.Length == 11)
            return $"{d[..3]}.{d[3..6]}.{d[6..9]}-{d[9..]}";
        if (tipo == "PJ" && d.Length == 14)
            return $"{d[..2]}.{d[2..5]}.{d[5..8]}/{d[8..12]}-{d[12..]}";
        return doc;
    }

    private static string? ValidarCpf(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf)) return "CPF é obrigatório.";
        if (cpf.Length != 11) return "CPF deve ter 11 dígitos.";
        if (cpf.Distinct().Count() == 1) return "CPF inválido.";
        int[] m1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] m2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        var s1 = m1.Select((m, i) => (cpf[i] - '0') * m).Sum();
        var r1 = s1 % 11; var d1 = r1 < 2 ? 0 : 11 - r1;
        if ((cpf[9] - '0') != d1) return "CPF inválido.";
        var s2 = m2.Select((m, i) => (cpf[i] - '0') * m).Sum();
        var r2 = s2 % 11; var d2 = r2 < 2 ? 0 : 11 - r2;
        return (cpf[10] - '0') != d2 ? "CPF inválido." : null;
    }

    private static string? ValidarCnpj(string cnpj)
    {
        if (string.IsNullOrWhiteSpace(cnpj)) return "CNPJ é obrigatório.";
        if (cnpj.Length != 14) return "CNPJ deve ter 14 dígitos.";
        if (cnpj.Distinct().Count() == 1) return "CNPJ inválido.";
        int[] m1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] m2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        var s1 = m1.Select((m, i) => (cnpj[i] - '0') * m).Sum();
        var r1 = s1 % 11; if ((cnpj[12] - '0') != (r1 < 2 ? 0 : 11 - r1)) return "CNPJ inválido.";
        var s2 = m2.Select((m, i) => (cnpj[i] - '0') * m).Sum();
        var r2 = s2 % 11;
        return (cnpj[13] - '0') != (r2 < 2 ? 0 : 11 - r2) ? "CNPJ inválido." : null;
    }
}