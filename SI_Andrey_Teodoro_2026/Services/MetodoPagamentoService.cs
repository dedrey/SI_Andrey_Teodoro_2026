using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Repositories.Interfaces;
using SI_Andrey_Teodoro_2026.Services.Interfaces;
namespace SI_Andrey_Teodoro_2026.Services;
public class MetodoPagamentoService : BaseService<MetodoPagamentoDto, MetodoPagamentoListDto>, IMetodoPagamentoService
{
    private readonly IMetodoPagamentoRepository _repo;
    public MetodoPagamentoService(IMetodoPagamentoRepository repo) => _repo = repo;
    protected override string NomeEntidade => "Método de pagamento";
    public Task<PaginacaoDto<MetodoPagamentoListDto>> ObterTodosAsync(FiltroConsultaDto filtro)
        => _repo.ObterTodosAsync(filtro);
    public Task<IEnumerable<MetodoPagamentoListDto>> ObterTodosAtivosAsync()
        => _repo.ObterTodosAtivosAsync();
    public async Task<MetodoPagamentoDto?> ObterPorIdAsync(int id)
    {
        var m = await _repo.ObterPorIdAsync(id);
        if (m == null) return null;
        return new MetodoPagamentoDto
        {
            Id = m.Id,
            IdOriginal = m.Id,
            Codigo = m.Codigo,
            MetodoPagamento = m.NomeMetodoPagamento,
            Ativo = m.Ativo,
            AtualizadoEm = m.AtualizadoEm,
            NomeAtualizadoPor = m.NomeAtualizadoPor
        };
    }
    public async Task<(bool sucesso, string mensagem, int id)> SalvarAsync(MetodoPagamentoDto dto)
    {
        try
        {
            dto.Codigo = dto.Codigo.Trim().ToUpper();
            dto.MetodoPagamento = CapitalizarPrimeira(dto.MetodoPagamento.Trim());
            int? ignorar = dto.IdOriginal > 0 ? dto.IdOriginal : null;
            if (await _repo.ExisteCodigoAsync(dto.Codigo, ignorar))
                return (false, $"Já existe um método de pagamento com o código '{dto.Codigo}'.", 0);
            if (await _repo.ExisteNomeAsync(dto.MetodoPagamento, ignorar))
                return (false, $"Já existe um método de pagamento com o nome '{dto.MetodoPagamento}'.", 0);
            if (dto.IdOriginal == 0)
            {
                var novoId = await _repo.InserirAsync(dto);
                return (true, "Método de pagamento cadastrado com sucesso!", novoId);
            }
            await _repo.AtualizarAsync(dto);
            return (true, "Método de pagamento atualizado com sucesso!", dto.Id);
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
    private static string CapitalizarPrimeira(string v)
        => string.IsNullOrEmpty(v) ? v : char.ToUpper(v[0]) + v[1..];
}