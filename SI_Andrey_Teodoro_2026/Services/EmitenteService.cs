using SI_Andrey_Teodoro_2026.DTOs;
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
            Cnpj = FormatarCnpj(e.Cnpj),
            InscricaoEstadual = e.InscricaoEstadual,
            RegimeTributario = e.RegimeTributario,
            CidadeId = e.CidadeId,
            Endereco = e.Endereco,
            Complemento = e.Complemento,
            Bairro = e.Bairro,
            Telefone = e.Telefone ?? string.Empty,
            Email = e.Email ?? string.Empty,
            Ativo = e.Ativo,
            AtualizadoEm = e.AtualizadoEm,
            NomeAtualizadoPor = e.NomeAtualizadoPor
        };
    }

    public async Task<(bool sucesso, string mensagem, int id)> SalvarAsync(EmitenteDto dto)
    {
        try
        {
            dto.NomeRazaoSocial = dto.NomeRazaoSocial.Trim();
            dto.ApelidoNomeFantasia = dto.ApelidoNomeFantasia.Trim();
            dto.InscricaoEstadual = dto.InscricaoEstadual?.Trim();
            dto.Telefone = dto.Telefone?.Trim() ?? string.Empty;
            dto.Email = dto.Email?.Trim().ToLower() ?? string.Empty;
            dto.Endereco = dto.Endereco.Trim();
            dto.Bairro = dto.Bairro.Trim();

            var cnpjLimpo = LimparDigitos(dto.Cnpj);
            var erroCnpj = ValidarCnpj(cnpjLimpo);
            if (erroCnpj != null) return (false, erroCnpj, 0);
            dto.Cnpj = cnpjLimpo;

            if (string.IsNullOrWhiteSpace(dto.RegimeTributario))
                return (false, "Selecione o regime tributário.", 0);

            int? ignorar = dto.IdOriginal > 0 ? dto.IdOriginal : null;

            if (await _repo.ExisteCnpjAsync(dto.Cnpj, ignorar))
                return (false, $"Já existe um emitente com o CNPJ '{FormatarCnpj(dto.Cnpj)}'.", 0);

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