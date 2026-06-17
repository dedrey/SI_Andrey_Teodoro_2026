using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Repositories.Interfaces;
using SI_Andrey_Teodoro_2026.Services.Interfaces;

namespace SI_Andrey_Teodoro_2026.Services;

public class ContaPagarService : IContaPagarService
{
    private readonly IContaPagarRepository _repo;

    public ContaPagarService(IContaPagarRepository repo) => _repo = repo;

    public Task<PaginacaoDto<ContaPagarListDto>> ObterTodosAsync(FiltroConsultaDto filtro)
        => _repo.ObterTodosAsync(filtro);

    public async Task<ContaPagarDto?> ObterPorIdAsync(int id)
    {
        var c = await _repo.ObterPorIdAsync(id);
        if (c == null) return null;
        return new ContaPagarDto
        {
            Id = c.Id,
            IdOriginal = c.Id,
            FornecedorId = c.FornecedorId,
            NomeFornecedor = c.NomeFornecedor,
            MovimentacaoId = c.MovimentacaoId,
            NumeroNfMovimentacao = c.NumeroNfMovimentacao,
            Descricao = c.Descricao,
            DataVencimento = c.DataVencimento,
            DataPagamento = c.DataPagamento,
            ComprovanteArquivo = c.ComprovanteArquivo,
            ValorOriginal = c.ValorOriginal,
            ValorSaldo = c.ValorSaldo,
            Status = c.Status,
            AtualizadoEm = c.AtualizadoEm,
            NomeAtualizadoPor = c.NomeAtualizadoPor
        };
    }

    public async Task<(bool sucesso, string mensagem, int id)> SalvarAsync(ContaPagarDto dto)
    {
        try
        {
            dto.Descricao = dto.Descricao.Trim();

            if (string.IsNullOrWhiteSpace(dto.Descricao))
                return (false, "Informe a descrição da conta.", 0);
            if (dto.ValorOriginal <= 0)
                return (false, "Informe um valor maior que zero.", 0);
            if (dto.DataVencimento == default)
                return (false, "Informe a data de vencimento.", 0);

            if (dto.IdOriginal == 0)
            {
                var novoId = await _repo.InserirAsync(dto);
                return (true, "Conta a pagar cadastrada com sucesso!", novoId);
            }

            await _repo.AtualizarAsync(dto);
            return (true, "Conta a pagar atualizada com sucesso!", dto.Id);
        }
        catch (Exception ex)
        {
            return (false, $"Erro: {ex.Message}", 0);
        }
    }

    public async Task<(bool sucesso, string mensagem)> MarcarComoPagaAsync(int id, DateTime dataPagamento, string? comprovanteArquivo = null)
    {
        try
        {
            await _repo.AtualizarStatusAsync(id, "PAGA", dataPagamento, comprovanteArquivo);
            return (true, "Conta marcada como paga!");
        }
        catch (Exception ex) { return (false, $"Erro: {ex.Message}"); }
    }

    public async Task<(bool sucesso, string mensagem)> CancelarAsync(int id)
    {
        try
        {
            await _repo.AtualizarStatusAsync(id, "CANCELADA");
            return (true, "Conta cancelada.");
        }
        catch (Exception ex) { return (false, $"Erro: {ex.Message}"); }
    }

    public async Task GerarContaAutomaticaAsync(int? fornecedorId, int movimentacaoId, string numeroNf,
        DateTime dataEntrada, int diasPrazo, decimal valorTotal)
    {
        if (diasPrazo <= 0 || valorTotal <= 0) return;

        var descricao = string.IsNullOrWhiteSpace(numeroNf)
            ? $"Entrada de mercadoria #{movimentacaoId}"
            : $"NF {numeroNf} — Entrada de mercadoria";

        var vencimento = dataEntrada.AddDays(diasPrazo);

        await _repo.InserirAutomaticaAsync(fornecedorId, movimentacaoId, descricao, vencimento, valorTotal);
    }
}