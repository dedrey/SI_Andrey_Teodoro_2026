using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Repositories.Interfaces;
using SI_Andrey_Teodoro_2026.Services.Interfaces;

namespace SI_Andrey_Teodoro_2026.Services;

public class CondicaoPagamentoService : BaseService<CondicaoPagamentoDto, CondicaoPagamentoListDto>,
    ICondicaoPagamentoService
{
    private readonly ICondicaoPagamentoRepository _repo;

    public CondicaoPagamentoService(ICondicaoPagamentoRepository repo) => _repo = repo;

    protected override string NomeEntidade => "Condição de pagamento";
    protected override bool Feminino => true;

    public Task<PaginacaoDto<CondicaoPagamentoListDto>> ObterTodosAsync(FiltroConsultaDto filtro)
        => _repo.ObterTodosAsync(filtro);

    public Task<IEnumerable<CondicaoPagamentoListDto>> ObterTodosAtivosAsync()
        => _repo.ObterTodosAtivosAsync();

    public async Task<CondicaoPagamentoDto?> ObterPorIdAsync(int id)
    {
        var c = await _repo.ObterPorIdAsync(id);
        if (c == null) return null;

        return new CondicaoPagamentoDto
        {
            Id = c.Id,
            IdOriginal = c.Id,
            CondicaoPagamento = c.NomeCondicaoPagamento,
            MetodoPagamentoId = c.MetodoPagamentoId,
            NumeroParcelas = c.NumeroParcelas,
            EntradaMinimaPercentual = c.EntradaMinimaPercentual,
            DescontoPercentual = c.DescontoPercentual,
            AcrescimoPercentual = c.AcrescimoPercentual,
            MultaPercentual = c.MultaPercentual,
            TaxaJurosPercentual = c.TaxaJurosPercentual,
            Ativo = c.Ativo,
            AtualizadoEm = c.AtualizadoEm,
            NomeAtualizadoPor = c.NomeAtualizadoPor,
            Parcelas = await _repo.ObterParcelasAsync(id)
        };
    }

    public Task<bool> EmUsoAsync(int id) => _repo.EmUsoAsync(id);

    public Task<int> ObterProximoIdAsync()
        => _repo.ObterProximoIdAsync();

    public async Task<(bool sucesso, string mensagem, int id)> SalvarAsync(CondicaoPagamentoDto dto)
    {
        try
        {
            var erroId = await ValidarIdAsync(dto.Id, dto.IdOriginal, _repo.ExisteIdAsync);
            if (erroId != null) return (false, erroId, 0);

            dto.CondicaoPagamento = dto.CondicaoPagamento.Trim().ToUpperInvariant();

            if (dto.MetodoPagamentoId == 0)
                return (false, "Selecione um método de pagamento.", 0);
            if (dto.NumeroParcelas < 1)
                return (false, "Número de parcelas deve ser pelo menos 1.", 0);
            if (dto.EntradaMinimaPercentual is < 0 or > 100)
                return (false, "Entrada mínima deve ser entre 0% e 100%.", 0);
            if (dto.DescontoPercentual is < 0 or > 100)
                return (false, "Desconto deve ser entre 0% e 100%.", 0);
            if (dto.AcrescimoPercentual is < 0 or > 100)
                return (false, "Acréscimo deve ser entre 0% e 100%.", 0);
            if (dto.MultaPercentual is < 0 or > 100)
                return (false, "Multa deve ser entre 0% e 100%.", 0);
            if (dto.TaxaJurosPercentual is < 0 or > 100)
                return (false, "Taxa de juros deve ser entre 0% e 100%.", 0);
            if (dto.Parcelas.Count > 0 && dto.Parcelas.Count != dto.NumeroParcelas)
                return (false, "O número de parcelas configuradas não bate com o campo 'Nº parcelas'.", 0);
            if (dto.Parcelas.Any(p => p.DiasVencimento < 0))
                return (false, "Dias de vencimento não pode ser negativo.", 0);
            if (dto.Parcelas.Count > 1)
            {
                var ordenadas = dto.Parcelas.OrderBy(p => p.NumeroParcela).ToList();
                for (int i = 1; i < ordenadas.Count; i++)
                {
                    if (ordenadas[i].DiasVencimento <= ordenadas[i - 1].DiasVencimento)
                        return (false, $"Os dias de vencimento devem ser crescentes: a parcela {ordenadas[i].NumeroParcela} " +
                            $"({ordenadas[i].DiasVencimento} dias) deve ter um prazo maior que a parcela {ordenadas[i - 1].NumeroParcela} " +
                            $"({ordenadas[i - 1].DiasVencimento} dias).", 0);
                }
            }

            if (dto.IdOriginal > 0 && await _repo.EmUsoAsync(dto.IdOriginal))
            {
                var atual = await _repo.ObterPorIdAsync(dto.IdOriginal);
                if (atual == null) return (false, "Condição de pagamento não encontrada.", 0);
                var diasAtuais = (await _repo.ObterParcelasAsync(dto.IdOriginal)).OrderBy(p => p.NumeroParcela).Select(p => p.DiasVencimento);
                var diasNovos = dto.Parcelas.OrderBy(p => p.NumeroParcela).Select(p => p.DiasVencimento);
                if (atual.NumeroParcelas != dto.NumeroParcelas || !diasAtuais.SequenceEqual(diasNovos))
                    return (false, "Condição já utilizada: para mudar as parcelas, inative e cadastre outra.", 0);
            }

            int? ignorar = dto.IdOriginal > 0 ? dto.IdOriginal : null;
            if (await _repo.ExisteNomeAsync(dto.CondicaoPagamento, dto.MetodoPagamentoId, ignorar))
                return (false, $"Já existe uma condição de pagamento com o nome '{dto.CondicaoPagamento}' para esse método de pagamento.", 0);

            if (dto.IdOriginal == 0)
            {
                var novoId = await _repo.InserirAsync(dto);
                return (true, "Condição de pagamento cadastrada com sucesso!", novoId);
            }

            await _repo.AtualizarAsync(dto);
            return (true, "Condição de pagamento atualizada com sucesso!", dto.Id);
        }
        catch (Exception ex) { return (false, Erro(ex).mensagem, 0); }
    }

    public async Task<(bool sucesso, string mensagem)> AlterarStatusAsync(int id, bool ativar)
    {
        try { await _repo.AlterarStatusAsync(id, ativar); return SucessoStatus(ativar); }
        catch (Exception ex) { return ErroStatus(ex); }
    }

}