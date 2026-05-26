using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Repositories.Interfaces;
using SI_Andrey_Teodoro_2026.Services.Interfaces;

namespace SI_Andrey_Teodoro_2026.Services;

public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _repo;
    public UsuarioService(IUsuarioRepository repo) => _repo = repo;

    public Task<PaginacaoDto<UsuarioListDto>> ObterTodosAsync(FiltroConsultaDto filtro)
        => _repo.ObterTodosAsync(filtro);

    public Task<IEnumerable<UsuarioListDto>> ObterTodosAtivosAsync()
        => _repo.ObterTodosAtivosAsync();

    public async Task<UsuarioDto?> ObterPorIdAsync(int id)
    {
        var u = await _repo.ObterPorIdAsync(id);
        if (u == null) return null;
        return new UsuarioDto
        {
            Id = u.Id,
            IdOriginal = u.Id,
            Nome = u.Nome,
            Email = u.Email,
            Cpf = FormatarCpf(u.Cpf),
            Telefone = u.Telefone ?? string.Empty,
            Ativo = u.Ativo,
            AtualizadoEm = u.AtualizadoEm,
            NomeAtualizadoPor = u.NomeAtualizadoPor
        };
    }

    public async Task<(bool sucesso, string mensagem, int id)> SalvarAsync(UsuarioDto dto)
    {
        try
        {
            dto.Nome = dto.Nome.Trim();
            dto.Email = dto.Email.Trim().ToLower();
            dto.Telefone = dto.Telefone.Trim();

            var cpfLimpo = LimparDigitos(dto.Cpf);
            var erroCpf = ValidarCpf(cpfLimpo);
            if (erroCpf != null) return (false, erroCpf, 0);
            dto.Cpf = cpfLimpo;

            int? ignorar = dto.IdOriginal > 0 ? dto.IdOriginal : null;
            if (await _repo.ExisteCpfAsync(dto.Cpf, ignorar))
                return (false, $"Já existe um usuário com o CPF '{FormatarCpf(dto.Cpf)}'.", 0);
            if (await _repo.ExisteEmailAsync(dto.Email, ignorar))
                return (false, $"Já existe um usuário com o e-mail '{dto.Email}'.", 0);

            if (dto.IdOriginal == 0)
            {
                var novoId = await _repo.InserirAsync(dto);
                return (true, "Usuário cadastrado com sucesso!", novoId);
            }
            await _repo.AtualizarAsync(dto);
            return (true, "Usuário atualizado com sucesso!", dto.Id);
        }
        catch (Exception ex) { return (false, $"Erro ao salvar usuário: {ex.Message}", 0); }
    }

    public async Task<(bool sucesso, string mensagem)> AlterarStatusAsync(int id, bool ativar)
    {
        try
        {
            await _repo.AlterarStatusAsync(id, ativar);
            return (true, $"Usuário {(ativar ? "ativado" : "desativado")} com sucesso!");
        }
        catch (Exception ex) { return (false, $"Erro ao alterar status: {ex.Message}"); }
    }

    private static string LimparDigitos(string v) => new string(v.Where(char.IsDigit).ToArray());

    public static string FormatarCpf(string cpf)
    {
        var d = LimparDigitos(cpf);
        return d.Length == 11 ? $"{d[..3]}.{d[3..6]}.{d[6..9]}-{d[9..]}" : cpf;
    }

    private static string? ValidarCpf(string cpf)
    {
        if (cpf.Length != 11) return "CPF deve ter 11 dígitos.";
        if (cpf.Distinct().Count() == 1) return "CPF inválido.";
        int[] m1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] m2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        var s1 = m1.Select((m, i) => (cpf[i] - '0') * m).Sum();
        var r1 = s1 % 11; var d1 = r1 < 2 ? 0 : 11 - r1;
        if ((cpf[9] - '0') != d1) return "CPF inválido.";
        var s2 = m2.Select((m, i) => (cpf[i] - '0') * m).Sum();
        var r2 = s2 % 11; var d2 = r2 < 2 ? 0 : 11 - r2;
        if ((cpf[10] - '0') != d2) return "CPF inválido.";
        return null;
    }
}