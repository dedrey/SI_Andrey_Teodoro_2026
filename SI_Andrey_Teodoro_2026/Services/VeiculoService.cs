using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Repositories.Interfaces;
using SI_Andrey_Teodoro_2026.Services.Interfaces;

namespace SI_Andrey_Teodoro_2026.Services;
public class VeiculoService : BaseService<VeiculoDto, VeiculoListDto>, IVeiculoService
{
    private readonly IVeiculoRepository _repo;

    public VeiculoService(IVeiculoRepository repo) => _repo = repo;

    protected override string NomeEntidade => "Veículo";

    public Task<PaginacaoDto<VeiculoListDto>> ObterTodosAsync(FiltroConsultaDto filtro)
        => _repo.ObterTodosAsync(filtro);

    public Task<IEnumerable<VeiculoListDto>> ObterTodosAtivosAsync()
        => _repo.ObterTodosAtivosAsync();
    public async Task<VeiculoDto?> ObterPorIdAsync(int id)
    {
        var v = await _repo.ObterPorIdAsync(id);
        if (v == null) return null;
        return new VeiculoDto
        {
            Id = v.Id,
            IdOriginal = v.Id,
            TransportadoraId = v.TransportadoraId,
            Placa = v.Placa,
            Uf = v.Uf,
            Ativo = v.Ativo,
            AtualizadoEm = v.AtualizadoEm,
            NomeAtualizadoPor = v.NomeAtualizadoPor
        };
    }
    public async Task<(bool sucesso, string mensagem, int id)> SalvarAsync(VeiculoDto dto)
    {
        try
        {
            dto.Placa = new string(dto.Placa.Where(char.IsLetterOrDigit).ToArray()).ToUpper();
            dto.Uf = dto.Uf.Trim().ToUpper();

            if (dto.TransportadoraId == 0)
                return (false, "Selecione uma transportadora.", 0);

            if (dto.Placa.Length != 7)
                return (false, "Placa deve ter exatamente 7 caracteres (ex: ABC1234 ou ABC1D23).", 0);

            if (!ValidarPlaca(dto.Placa))
                return (false, "Formato de placa inválido. Use o padrão antigo (ABC1234) ou Mercosul (ABC1D23).", 0);

            if (dto.Uf.Length != 2)
                return (false, "UF deve ter 2 letras.", 0);

            int? ignorar = dto.IdOriginal > 0 ? dto.IdOriginal : null;

            if (await _repo.ExistePlacaAsync(dto.Placa, ignorar))
                return (false, $"Já existe um veículo com a placa '{dto.Placa}'.", 0);

            if (dto.IdOriginal == 0)
            {
                var novoId = await _repo.InserirAsync(dto);
                return (true, "Veículo cadastrado com sucesso!", novoId);
            }

            await _repo.AtualizarAsync(dto);
            return (true, "Veículo atualizado com sucesso!", dto.Id);
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
    private static bool ValidarPlaca(string placa)
    {
        if (placa.Length != 7) return false;
        var antigo = placa[..3].All(char.IsLetter) && placa[3..].All(char.IsDigit);
        var mercosul = placa[..3].All(char.IsLetter)
                    && char.IsDigit(placa[3])
                    && char.IsLetter(placa[4])
                    && char.IsDigit(placa[5])
                    && char.IsDigit(placa[6]);
        return antigo || mercosul;
    }
}