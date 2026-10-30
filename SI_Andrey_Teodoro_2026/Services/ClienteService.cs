using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Helpers;
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
            CpfCnpj = DocumentoHelper.FormatarDocumento(c.CpfCnpj),
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

    public Task<int> ObterProximoIdAsync()
        => _repo.ObterProximoIdAsync();

    public async Task<(bool sucesso, string mensagem, int id)> SalvarAsync(ClienteDto dto)
    {
        try
        {
            var erroId = await ValidarIdAsync(dto.Id, dto.IdOriginal, _repo.ExisteIdAsync);
            if (erroId != null) return (false, erroId, 0);

            dto.NomeRazaoSocial = dto.NomeRazaoSocial.Trim().ToUpperInvariant();
            dto.ApelidoNomeFantasia = dto.ApelidoNomeFantasia?.Trim().ToUpperInvariant();
            dto.DocumentoEstrangeiro = dto.DocumentoEstrangeiro?.Trim().ToUpperInvariant();
            dto.InscricaoEstadual = dto.InscricaoEstadual?.Trim().ToUpperInvariant();
            dto.InscricaoMunicipal = dto.InscricaoMunicipal?.Trim().ToUpperInvariant();
            dto.Telefone = dto.Telefone?.Trim() ?? string.Empty;
            dto.Celular = dto.Celular?.Trim();
            dto.Email = dto.Email?.Trim().ToUpperInvariant() ?? string.Empty;
            if (!System.Text.RegularExpressions.Regex.IsMatch(dto.Email, @"^[^@\s]+@[^@\s]+\.[A-Za-z]{2,}$"))
                return (false, "E-mail inválido.", 0);
            dto.Endereco = dto.Endereco.Trim().ToUpperInvariant();
            dto.Numero = dto.Numero?.Trim().ToUpperInvariant() ?? string.Empty;
            dto.Bairro = dto.Bairro.Trim().ToUpperInvariant();
            dto.Complemento = dto.Complemento?.Trim().ToUpperInvariant() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(dto.Complemento))
                return (false, "Complemento é obrigatório.", 0);
            dto.Cep = dto.Cep.Trim();
            if (!dto.Estrangeiro && string.IsNullOrWhiteSpace(dto.Cep))
                return (false, "CEP é obrigatório.", 0);
            if (!dto.Estrangeiro && !DocumentoHelper.CepValido(dto.Cep))
                return (false, "CEP incompleto.", 0);

            if (!dto.CidadeId.HasValue)
                return (false, "Cidade é obrigatória.", 0);

            if (dto.TipoPessoa == "PJ" && string.IsNullOrWhiteSpace(dto.Celular))
                return (false, "Celular é obrigatório para Pessoa Jurídica.", 0);
            if (dto.TipoPessoa == "PF")
                dto.Celular = null;
            if (dto.TipoPessoa == "PJ")
                dto.Sexo = "NAO_INFORMAR";

            var erroTelefone = TelefoneHelper.ValidarObrigatorio(dto.Telefone, dto.Estrangeiro);
            if (erroTelefone != null)
                return (false, $"{(dto.TipoPessoa == "PJ" ? "Telefone" : "Celular")}: {erroTelefone}.", 0);
            if (dto.Celular != null && TelefoneHelper.ValidarObrigatorio(dto.Celular, dto.Estrangeiro) is string erroCelular)
                return (false, $"Celular: {erroCelular}.", 0);

            if (dto.IdOriginal > 0)
            {
                var atual = await _repo.ObterPorIdAsync(dto.IdOriginal);
                if (atual == null) return (false, "Cliente não encontrado.", 0);
                var docSalvo = !string.IsNullOrWhiteSpace(atual.CpfCnpj) || !string.IsNullOrWhiteSpace(atual.DocumentoEstrangeiro);
                if (docSalvo && (atual.TipoPessoa != dto.TipoPessoa || atual.Estrangeiro != dto.Estrangeiro
                    || DocumentoHelper.SomenteDigitos(atual.CpfCnpj) != DocumentoHelper.SomenteDigitos(dto.CpfCnpj)
                    || (atual.DocumentoEstrangeiro ?? "").Trim().ToUpperInvariant() != (dto.DocumentoEstrangeiro ?? "")))
                    return (false, "Tipo de pessoa, estrangeiro e documento não podem ser alterados após o cadastro.", 0);
            }
            if (dto.Estrangeiro ? string.IsNullOrWhiteSpace(dto.DocumentoEstrangeiro) : string.IsNullOrWhiteSpace(dto.CpfCnpj))
                return (false, dto.Estrangeiro ? "Informe o documento estrangeiro." : $"Informe o {(dto.TipoPessoa == "PF" ? "CPF" : "CNPJ")}.", 0);

            if (!dto.Estrangeiro && !string.IsNullOrWhiteSpace(dto.CpfCnpj))
            {
                var docLimpo = DocumentoHelper.SomenteDigitos(dto.CpfCnpj);
                string? erroDoc = DocumentoHelper.DocumentoValido(docLimpo, dto.TipoPessoa)
                    ? null
                    : dto.TipoPessoa == "PF" ? "CPF inválido." : "CNPJ inválido.";
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
}