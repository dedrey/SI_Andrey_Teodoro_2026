using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Helpers;
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
            CpfCnpj = DocumentoHelper.FormatarDocumento(f.CpfCnpj),
            NomeCidade = f.NomeCidade,
            CidadeId = f.CidadeId,
            Cep = f.Cep ?? string.Empty,
            Endereco = f.Endereco,
            Numero = f.Numero ?? string.Empty,
            Complemento = f.Complemento ?? string.Empty,
            Bairro = f.Bairro,
            Telefone = f.Telefone ?? string.Empty,
            Email = f.Email ?? string.Empty,
            CondicaoPagamentoId = f.CondicaoPagamentoId,
            NomeCondicaoPagamento = f.NomeCondicaoPagamento,
            Ativo = f.Ativo,
            AtualizadoEm = f.AtualizadoEm,
            NomeAtualizadoPor = f.NomeAtualizadoPor
        };
    }

    public Task<int> ObterProximoIdAsync()
        => _repo.ObterProximoIdAsync();

    public async Task<(bool sucesso, string mensagem, int id)> SalvarAsync(FornecedorDto dto)
    {
        try
        {
            var erroId = await ValidarIdAsync(dto.Id, dto.IdOriginal, _repo.ExisteIdAsync);
            if (erroId != null) return (false, erroId, 0);

            dto.RazaoSocial = dto.RazaoSocial.Trim().ToUpperInvariant();
            dto.NomeFantasia = dto.NomeFantasia?.Trim().ToUpperInvariant();
            dto.Telefone = dto.Telefone?.Trim() ?? string.Empty;
            dto.Email = dto.Email?.Trim().ToUpperInvariant() ?? string.Empty;
            if (!System.Text.RegularExpressions.Regex.IsMatch(dto.Email, @"^[^@\s]+@[^@\s]+\.[A-Za-z]{2,}$"))
                return (false, "E-mail inválido.", 0);
            dto.Endereco = dto.Endereco.Trim().ToUpperInvariant();
            dto.Numero = dto.Numero?.Trim().ToUpperInvariant() ?? string.Empty;
            dto.Bairro = dto.Bairro.Trim().ToUpperInvariant();
            dto.Complemento = dto.Complemento?.Trim().ToUpperInvariant() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(dto.Complemento))
                return (false, "Complemento é obrigatório.", 0);
            dto.Cep = CepHelper.Formatar(dto.Cep);
            if (string.IsNullOrWhiteSpace(dto.Cep))
                return (false, "CEP é obrigatório.", 0);
            if (!CepHelper.Valido(dto.Cep))
                return (false, "CEP incompleto.", 0);

            if (!dto.CidadeId.HasValue)
                return (false, "Cidade é obrigatória.", 0);

            if (!dto.CondicaoPagamentoId.HasValue)
                return (false, "Selecione a condição de pagamento.", 0);

            if (dto.TipoPessoa == "PJ" && string.IsNullOrWhiteSpace(dto.NomeFantasia))
                return (false, "Nome Fantasia é obrigatório para Pessoa Jurídica.", 0);
            if (dto.TipoPessoa == "PF")
                dto.NomeFantasia = null;

            var docLimpo = DocumentoHelper.SomenteDigitos(dto.CpfCnpj);

            if (dto.IdOriginal > 0)
            {
                var atual = await _repo.ObterPorIdAsync(dto.IdOriginal);
                if (atual == null) return (false, "Fornecedor não encontrado.", 0);
                if (atual.TipoPessoa != dto.TipoPessoa || DocumentoHelper.SomenteDigitos(atual.CpfCnpj) != docLimpo)
                    return (false, "Tipo de pessoa e CPF/CNPJ não podem ser alterados após o cadastro.", 0);
            }

            var erroDoc = DocumentoHelper.DocumentoValido(docLimpo, dto.TipoPessoa) ? null : dto.TipoPessoa == "PF" ? "CPF inválido." : "CNPJ inválido.";
            if (erroDoc != null) return (false, erroDoc, 0);
            dto.CpfCnpj = docLimpo;

            int? ignorar = dto.IdOriginal > 0 ? dto.IdOriginal : null;

            if (await _repo.ExisteCpfCnpjAsync(dto.CpfCnpj, ignorar))
                return (false, $"Já existe um fornecedor com o documento '{DocumentoHelper.FormatarDocumento(dto.CpfCnpj)}'.", 0);

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
}