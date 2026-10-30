using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Helpers;
using SI_Andrey_Teodoro_2026.Repositories.Interfaces;
using SI_Andrey_Teodoro_2026.Services.Interfaces;

namespace SI_Andrey_Teodoro_2026.Services;

public class EmitenteService : BaseService<EmitenteDto, EmitenteListDto>, IEmitenteService
{
    private readonly IEmitenteRepository _repo;

    public EmitenteService(IEmitenteRepository repo) => _repo = repo;

    protected override string NomeEntidade => "Emitente";

    public Task<PaginacaoDto<EmitenteListDto>> ObterTodosAsync(FiltroConsultaDto filtro)
        => _repo.ObterTodosAsync(filtro);

    public Task<IEnumerable<EmitenteListDto>> ObterTodosAtivosAsync()
        => _repo.ObterTodosAtivosAsync();

    public async Task<EmitenteDto?> ObterPorIdAsync(int id)
    {
        var e = await _repo.ObterPorIdAsync(id);
        if (e == null) return null;
        return new EmitenteDto
        {
            Id = e.Id,
            IdOriginal = e.Id,
            NomeRazaoSocial = e.NomeRazaoSocial,
            ApelidoNomeFantasia = e.ApelidoNomeFantasia,
            Cnpj = DocumentoHelper.FormatarDocumento(e.Cnpj),
            InscricaoEstadual = e.InscricaoEstadual,
            InscricaoMunicipal = e.InscricaoMunicipal,
            RegimeTributario = e.RegimeTributario,
            CidadeId = e.CidadeId,
            NomeCidade = e.NomeCidade,
            Endereco = e.Endereco,
            Numero = e.Numero ?? string.Empty,
            Complemento = e.Complemento,
            Bairro = e.Bairro,
            Cep = e.Cep ?? string.Empty,
            Telefone = e.Telefone ?? string.Empty,
            Email = e.Email ?? string.Empty,
            Ativo = e.Ativo,
            AtualizadoEm = e.AtualizadoEm,
            NomeAtualizadoPor = e.NomeAtualizadoPor
        };
    }

    public Task<int> ObterProximoIdAsync()
        => _repo.ObterProximoIdAsync();

    public async Task<(bool sucesso, string mensagem, int id)> SalvarAsync(EmitenteDto dto)
    {
        try
        {
            var erroId = await ValidarIdAsync(dto.Id, dto.IdOriginal, _repo.ExisteIdAsync);
            if (erroId != null) return (false, erroId, 0);

            dto.NomeRazaoSocial = dto.NomeRazaoSocial.Trim().ToUpperInvariant();
            dto.ApelidoNomeFantasia = dto.ApelidoNomeFantasia.Trim().ToUpperInvariant();
            dto.InscricaoEstadual = dto.InscricaoEstadual?.Trim().ToUpperInvariant();
            dto.InscricaoMunicipal = dto.InscricaoMunicipal?.Trim().ToUpperInvariant();
            dto.Telefone = dto.Telefone?.Trim() ?? string.Empty;
            dto.Email = dto.Email?.Trim().ToUpperInvariant() ?? string.Empty;
            if (!System.Text.RegularExpressions.Regex.IsMatch(dto.Email, @"^[^@\s]+@[^@\s]+\.[A-Za-z]{2,}$"))
                return (false, "E-mail inválido.", 0);
            dto.Endereco = dto.Endereco.Trim().ToUpperInvariant();
            dto.Numero = dto.Numero?.Trim().ToUpperInvariant() ?? string.Empty;
            dto.Complemento = dto.Complemento?.Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(dto.Complemento))
                return (false, "Complemento é obrigatório.", 0);

            if (!dto.CidadeId.HasValue)
                return (false, "Cidade é obrigatória.", 0);
            dto.Bairro = dto.Bairro.Trim().ToUpperInvariant();
            dto.Cep = CepHelper.Formatar(dto.Cep);
            if (string.IsNullOrWhiteSpace(dto.Cep))
                return (false, "CEP é obrigatório.", 0);
            if (!CepHelper.Valido(dto.Cep))
                return (false, "CEP incompleto.", 0);

            var cnpjLimpo = DocumentoHelper.SomenteDigitos(dto.Cnpj);

            if (dto.IdOriginal > 0)
            {
                var atual = await _repo.ObterPorIdAsync(dto.IdOriginal);
                if (atual == null) return (false, "Emitente não encontrado.", 0);
                if (DocumentoHelper.SomenteDigitos(atual.Cnpj) != cnpjLimpo)
                    return (false, "O CNPJ não pode ser alterado após o cadastro.", 0);
            }

            var erroCnpj = DocumentoHelper.CnpjValido(cnpjLimpo) ? null : "CNPJ inválido.";
            if (erroCnpj != null) return (false, erroCnpj, 0);
            dto.Cnpj = cnpjLimpo;

            if (string.IsNullOrWhiteSpace(dto.RegimeTributario))
                return (false, "Selecione o regime tributário.", 0);

            int? ignorar = dto.IdOriginal > 0 ? dto.IdOriginal : null;

            if (await _repo.ExisteCnpjAsync(dto.Cnpj, ignorar))
                return (false, $"Já existe um emitente com o CNPJ '{DocumentoHelper.FormatarDocumento(dto.Cnpj)}'.", 0);

            if (dto.IdOriginal == 0)
            {
                var novoId = await _repo.InserirAsync(dto);
                return (true, "Emitente cadastrado com sucesso!", novoId);
            }

            await _repo.AtualizarAsync(dto);
            return (true, "Emitente atualizado com sucesso!", dto.Id);
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