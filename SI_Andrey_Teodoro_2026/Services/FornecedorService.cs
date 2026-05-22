using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Repositories.Interfaces;
using SI_Andrey_Teodoro_2026.Services.Interfaces;

namespace SI_Andrey_Teodoro_2026.Services;

public class FornecedorService : IFornecedorService
{
    private readonly IFornecedorRepository _repo;

    public FornecedorService(IFornecedorRepository repo) => _repo = repo;

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
            Cnpj = FormatarCnpj(f.Cnpj),
            NomeFantasia = f.NomeFantasia,
            PaisId = f.PaisId,
            EstadoId = f.EstadoId,
            CidadeId = f.CidadeId,
            Endereco = f.Endereco,
            Bairro = f.Bairro,
            Telefone = f.Telefone,
            Email = f.Email,
            Ativo = f.Ativo,
            AtualizadoEm = f.AtualizadoEm
        };
    }

    public async Task<(bool sucesso, string mensagem, int id)> SalvarAsync(FornecedorDto dto)
    {
        try
        {
            dto.RazaoSocial = dto.RazaoSocial.Trim();
            dto.NomeFantasia = dto.NomeFantasia?.Trim();
            dto.Endereco = dto.Endereco?.Trim();
            dto.Bairro = dto.Bairro?.Trim();
            dto.Telefone = dto.Telefone?.Trim();
            dto.Email = dto.Email?.Trim().ToLower();
            var cnpjLimpo = LimparDocumento(dto.Cnpj);

            var erroCnpj = ValidarCnpj(cnpjLimpo);
            if (erroCnpj != null)
                return (false, erroCnpj, 0);

            dto.Cnpj = cnpjLimpo;

            int? ignorar = dto.IdOriginal > 0 ? dto.IdOriginal : null;
            if (await _repo.ExisteCnpjAsync(dto.Cnpj, ignorar))
                return (false, $"Já existe um fornecedor com o CNPJ '{FormatarCnpj(dto.Cnpj)}'.", 0);
            if (await _repo.ExisteRazaoSocialAsync(dto.RazaoSocial, ignorar))
                return (false, $"Já existe um fornecedor com a Razão Social '{dto.RazaoSocial}'.", 0);
            if (!string.IsNullOrWhiteSpace(dto.NomeFantasia))
            {
                if (await _repo.ExisteNomeFantasiaAsync(dto.NomeFantasia, ignorar))
                    return (false, $"Já existe um fornecedor com o Nome Fantasia '{dto.NomeFantasia}'.", 0);
            }

            if (dto.IdOriginal == 0)
            {
                var novoId = await _repo.InserirAsync(dto);
                return (true, "Fornecedor cadastrado com sucesso!", novoId);
            }
            else
            {
                await _repo.AtualizarAsync(dto);
                return (true, "Fornecedor atualizado com sucesso!", dto.Id);
            }
        }
        catch (Exception ex)
        {
            return (false, $"Erro ao salvar fornecedor: {ex.Message}", 0);
        }
    }

    public async Task<(bool sucesso, string mensagem)> AlterarStatusAsync(int id, bool ativar)
    {
        try
        {
            await _repo.AlterarStatusAsync(id, ativar);
            return (true, $"Fornecedor {(ativar ? "ativado" : "desativado")} com sucesso!");
        }
        catch (Exception ex)
        {
            return (false, $"Erro ao alterar status: {ex.Message}");
        }
    }
    private static string LimparDocumento(string doc)
        => new string(doc.Where(char.IsDigit).ToArray());

    public static string FormatarCnpj(string cnpj)
    {
        var d = LimparDocumento(cnpj);
        return d.Length == 14
            ? $"{d[..2]}.{d[2..5]}.{d[5..8]}/{d[8..12]}-{d[12..]}"
            : cnpj;
    }

    private static string? ValidarCnpj(string cnpj)
    {
        if (string.IsNullOrWhiteSpace(cnpj)) return "CNPJ é obrigatório.";
        if (cnpj.Length != 14) return "CNPJ deve ter 14 dígitos.";
        if (cnpj.Distinct().Count() == 1) return "CNPJ inválido.";

        int[] m1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] m2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        return ChecarDigito(cnpj, m1, 12) && ChecarDigito(cnpj, m2, 13) ? null : "CNPJ inválido.";
    }

    private static bool ChecarDigito(string doc, int[] mult, int tamanho)
    {
        var soma = mult.Select((m, i) => (doc[i] - '0') * m).Sum();
        var resto = soma % 11;
        var digito = resto < 2 ? 0 : 11 - resto;
        return (doc[tamanho] - '0') == digito;
    }
}