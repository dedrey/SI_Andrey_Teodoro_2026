using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Repositories.Interfaces;
using SI_Andrey_Teodoro_2026.Services.Interfaces;

namespace SI_Andrey_Teodoro_2026.Services;

public class TransportadoraService : BaseService<TransportadoraDto, TransportadoraListDto>, ITransportadoraService
{
    private readonly ITransportadoraRepository _repo;

    public TransportadoraService(ITransportadoraRepository repo) => _repo = repo;

    protected override string NomeEntidade => "Transportadora";

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
            Cnpj = FormatarCnpj(t.Cnpj),
            InscricaoEstadual = t.InscricaoEstadual,
            NomeCidade = t.NomeCidade,
            CidadeId = t.CidadeId,
            Cep = t.Cep,
            Endereco = t.Endereco,
            Complemento = t.Complemento,
            Bairro = t.Bairro,
            Telefone = t.Telefone ?? string.Empty,
            Email = t.Email ?? string.Empty,
            Ativo = t.Ativo,
            AtualizadoEm = t.AtualizadoEm,
            NomeAtualizadoPor = t.NomeAtualizadoPor
        };
    }

    public async Task<(bool sucesso, string mensagem, int id)> SalvarAsync(TransportadoraDto dto)
    {
        try
        {
            dto.RazaoSocial = dto.RazaoSocial.Trim();
            dto.NomeFantasia = dto.NomeFantasia?.Trim();
            dto.InscricaoEstadual = dto.InscricaoEstadual?.Trim();
            dto.Endereco = dto.Endereco?.Trim();
            dto.Complemento = dto.Complemento?.Trim();
            dto.Bairro = dto.Bairro?.Trim();
            dto.Telefone = dto.Telefone.Trim();
            dto.Email = dto.Email.Trim().ToLower();

            var cnpjLimpo = LimparDigitos(dto.Cnpj);
            var erroCnpj = ValidarCnpj(cnpjLimpo);
            if (erroCnpj != null) return (false, erroCnpj, 0);
            dto.Cnpj = cnpjLimpo;

            int? ignorar = dto.IdOriginal > 0 ? dto.IdOriginal : null;
            if (await _repo.ExisteCnpjAsync(dto.Cnpj, ignorar))
                return (false, $"Já existe uma transportadora com o CNPJ '{FormatarCnpj(dto.Cnpj)}'.", 0);

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