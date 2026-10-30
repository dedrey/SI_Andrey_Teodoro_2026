using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Helpers;
using SI_Andrey_Teodoro_2026.Repositories.Interfaces;
using SI_Andrey_Teodoro_2026.Services.Interfaces;

namespace SI_Andrey_Teodoro_2026.Services;

public class TransportadoraService : BaseService<TransportadoraDto, TransportadoraListDto>, ITransportadoraService
{
    private readonly ITransportadoraRepository _repo;

    public TransportadoraService(ITransportadoraRepository repo) => _repo = repo;

    protected override string NomeEntidade => "Transportadora";
    protected override bool Feminino => true;

    public Task<PaginacaoDto<TransportadoraListDto>> ObterTodosAsync(FiltroConsultaDto filtro)
        => _repo.ObterTodosAsync(filtro);

    public Task<IEnumerable<TransportadoraListDto>> ObterTodosAtivosAsync()
        => _repo.ObterTodosAtivosAsync();

    public async Task<TransportadoraDto?> ObterPorIdAsync(int id)
    {
        var t = await _repo.ObterPorIdAsync(id);
        if (t == null) return null;
        return new TransportadoraDto
        {
            Id = t.Id,
            IdOriginal = t.Id,
            RazaoSocial = t.RazaoSocial,
            NomeFantasia = t.NomeFantasia,
            Cnpj = DocumentoHelper.FormatarDocumento(t.Cnpj),
            InscricaoEstadual = t.InscricaoEstadual,
            NomeCidade = t.NomeCidade,
            CidadeId = t.CidadeId,
            Cep = t.Cep ?? string.Empty,
            Endereco = t.Endereco ?? string.Empty,
            Numero = t.Numero ?? string.Empty,
            Complemento = t.Complemento ?? string.Empty,
            Bairro = t.Bairro ?? string.Empty,
            Telefone = t.Telefone ?? string.Empty,
            Email = t.Email ?? string.Empty,
            Ativo = t.Ativo,
            AtualizadoEm = t.AtualizadoEm,
            NomeAtualizadoPor = t.NomeAtualizadoPor
        };
    }

    public Task<int> ObterProximoIdAsync()
        => _repo.ObterProximoIdAsync();

    public async Task<(bool sucesso, string mensagem, int id)> SalvarAsync(TransportadoraDto dto)
    {
        try
        {
            var erroId = await ValidarIdAsync(dto.Id, dto.IdOriginal, _repo.ExisteIdAsync);
            if (erroId != null) return (false, erroId, 0);

            dto.RazaoSocial = dto.RazaoSocial.Trim().ToUpperInvariant();
            dto.NomeFantasia = dto.NomeFantasia?.Trim().ToUpperInvariant();
            dto.InscricaoEstadual = dto.InscricaoEstadual?.Trim().ToUpperInvariant();
            dto.Endereco = dto.Endereco.Trim().ToUpperInvariant();
            dto.Numero = dto.Numero?.Trim().ToUpperInvariant() ?? string.Empty;
            dto.Complemento = dto.Complemento?.Trim().ToUpperInvariant() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(dto.Complemento))
                return (false, "Complemento é obrigatório.", 0);
            dto.Bairro = dto.Bairro.Trim().ToUpperInvariant();
            dto.Cep = dto.Cep.Trim();
            if (string.IsNullOrWhiteSpace(dto.Cep))
                return (false, "CEP é obrigatório.", 0);
            if (!DocumentoHelper.CepValido(dto.Cep))
                return (false, "CEP incompleto.", 0);

            if (!dto.CidadeId.HasValue)
                return (false, "Cidade é obrigatória.", 0);
            dto.Telefone = dto.Telefone.Trim();
            dto.Email = dto.Email.Trim().ToUpperInvariant();
            if (!System.Text.RegularExpressions.Regex.IsMatch(dto.Email, @"^[^@\s]+@[^@\s]+\.[A-Za-z]{2,}$"))
                return (false, "E-mail inválido.", 0);

            var cnpjLimpo = DocumentoHelper.SomenteDigitos(dto.Cnpj);

            if (dto.IdOriginal > 0)
            {
                var atual = await _repo.ObterPorIdAsync(dto.IdOriginal);
                if (atual == null) return (false, "Transportadora não encontrada.", 0);
                if (DocumentoHelper.SomenteDigitos(atual.Cnpj) != cnpjLimpo)
                    return (false, "O CNPJ não pode ser alterado após o cadastro.", 0);
            }

            var erroCnpj = DocumentoHelper.CnpjValido(cnpjLimpo) ? null : "CNPJ inválido.";
            if (erroCnpj != null) return (false, erroCnpj, 0);
            dto.Cnpj = cnpjLimpo;

            int? ignorar = dto.IdOriginal > 0 ? dto.IdOriginal : null;
            if (await _repo.ExisteCnpjAsync(dto.Cnpj, ignorar))
                return (false, $"Já existe uma transportadora com o CNPJ '{DocumentoHelper.FormatarDocumento(dto.Cnpj)}'.", 0);

            if (dto.IdOriginal == 0)
            {
                var novoId = await _repo.InserirAsync(dto);
                return (true, "Transportadora cadastrada com sucesso!", novoId);
            }

            await _repo.AtualizarAsync(dto);
            return (true, "Transportadora atualizada com sucesso!", dto.Id);
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