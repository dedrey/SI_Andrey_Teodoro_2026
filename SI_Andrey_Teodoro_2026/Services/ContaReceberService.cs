using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Repositories.Interfaces;
using SI_Andrey_Teodoro_2026.Services.Interfaces;

namespace SI_Andrey_Teodoro_2026.Services;

public class ContaReceberService : IContaReceberService
{
    private readonly IContaReceberRepository _repo;

    public ContaReceberService(IContaReceberRepository repo) => _repo = repo;

    public Task<PaginacaoDto<ContaReceberListDto>> ObterTodosAsync(FiltroConsultaDto filtro)
        => _repo.ObterTodosAsync(filtro);
    public Task<PaginacaoDto<ContaReceberVendaGrupoListDto>> ObterTodosAgrupadosAsync(FiltroConsultaDto filtro)
        => _repo.ObterTodosAgrupadosAsync(filtro);

    public async Task<ContaReceberDto?> ObterPorIdAsync(int id)
    {
        var c = await _repo.ObterPorIdAsync(id);
        if (c == null) return null;
        return new ContaReceberDto
        {
            Id = c.Id,
            IdOriginal = c.Id,
            ClienteId = c.ClienteId,
            NomeCliente = c.NomeCliente,
            VendaId = c.VendaId,
            Descricao = c.Descricao,
            DataVencimento = c.DataVencimento,
            ValorOriginal = c.ValorOriginal,
            ValorSaldo = c.ValorSaldo,
            Status = c.Status,
            AtualizadoEm = c.AtualizadoEm,
            NomeAtualizadoPor = c.NomeAtualizadoPor,
            Baixas = await _repo.ObterBaixasAsync(id)
        };
    }

    public async Task<(bool sucesso, string mensagem, int id)> SalvarAsync(ContaReceberDto dto)
    {
        try
        {
            dto.Descricao = dto.Descricao.Trim();

            if (dto.ClienteId <= 0)
                return (false, "Selecione um cliente.", 0);
            if (string.IsNullOrWhiteSpace(dto.Descricao))
                return (false, "Informe a descrição da conta.", 0);
            if (dto.ValorOriginal <= 0)
                return (false, "Informe um valor maior que zero.", 0);
            if (dto.DataVencimento == default)
                return (false, "Informe a data de vencimento.", 0);

            if (dto.IdOriginal == 0)
            {
                var novoId = await _repo.InserirAsync(dto);
                return (true, "Conta a receber cadastrada com sucesso!", novoId);
            }
            var contaAtual = await _repo.ObterPorIdAsync(dto.IdOriginal);
            if (contaAtual != null)
            {
                var jaRecebido = contaAtual.ValorOriginal - contaAtual.ValorSaldo;
                if (jaRecebido > 0 && dto.ValorOriginal < jaRecebido)
                    return (false, $"Valor não pode ser menor que o já recebido (R$ {jaRecebido:N2}).", 0);
            }

            await _repo.AtualizarAsync(dto);
            return (true, "Conta a receber atualizada com sucesso!", dto.Id);
        }
        catch (Exception ex)
        {
            return (false, $"Erro: {ex.Message}", 0);
        }
    }

    public async Task<(bool sucesso, string mensagem)> RegistrarRecebimentoAsync(int contaReceberId,
        DateTime dataRecebimento, decimal valorRecebido, string? comprovanteArquivo, string? observacao)
    {
        try
        {
            if (valorRecebido <= 0)
                return (false, "Informe um valor de recebimento maior que zero.");

            var conta = await _repo.ObterPorIdAsync(contaReceberId);
            if (conta == null)
                return (false, "Conta a receber não encontrada.");
            if (conta.Status != "ABERTA")
                return (false, "Apenas contas abertas podem receber baixas.");
            if (valorRecebido > conta.ValorSaldo)
                return (false, $"Valor recebido (R$ {valorRecebido:N2}) é maior que o saldo devido (R$ {conta.ValorSaldo:N2}).");

            await _repo.RegistrarBaixaAsync(contaReceberId, dataRecebimento, valorRecebido, comprovanteArquivo, observacao);

            var saldoRestante = conta.ValorSaldo - valorRecebido;
            var mensagem = saldoRestante <= 0
                ? "Recebimento registrado! Conta totalmente paga."
                : $"Recebimento parcial registrado! Saldo restante: R$ {saldoRestante:N2}.";

            return (true, mensagem);
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

    public async Task<(bool sucesso, string mensagem)> AnexarComprovanteAsync(int baixaId, string comprovanteArquivo)
    {
        try
        {
            await _repo.AtualizarComprovanteBaixaAsync(baixaId, comprovanteArquivo);
            return (true, "Comprovante anexado com sucesso!");
        }
        catch (Exception ex) { return (false, $"Erro ao anexar comprovante: {ex.Message}"); }
    }

    public Task<List<ContaReceberResumoVendaDto>> ObterContasDaVendaAsync(int vendaId)
        => _repo.ObterContasDaVendaAsync(vendaId);
}