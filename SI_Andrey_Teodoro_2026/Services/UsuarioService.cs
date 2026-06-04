using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Repositories.Interfaces;
using SI_Andrey_Teodoro_2026.Services.Interfaces;

namespace SI_Andrey_Teodoro_2026.Services;

public class UsuarioService : BaseService<UsuarioDto, UsuarioListDto>, IUsuarioService
{
    private readonly IUsuarioRepository _repo;

    public UsuarioService(IUsuarioRepository repo) => _repo = repo;

    protected override string NomeEntidade => "Usuário";

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
            Cpf = u.Cpf,
            Telefone = u.Telefone,
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
            dto.Cpf = new string(dto.Cpf.Where(char.IsDigit).ToArray());
            dto.Telefone = dto.Telefone.Trim();

            if (dto.Nome.Length < 2)
                return (false, "Nome deve ter pelo menos 2 caracteres.", 0);

            if (dto.Cpf.Length != 11)
                return (false, "CPF deve ter 11 dígitos.", 0);

            int? ignorar = dto.IdOriginal > 0 ? dto.IdOriginal : null;

            if (await _repo.ExisteCpfAsync(dto.Cpf, ignorar))
                return (false, $"Já existe um usuário com este CPF.", 0);

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