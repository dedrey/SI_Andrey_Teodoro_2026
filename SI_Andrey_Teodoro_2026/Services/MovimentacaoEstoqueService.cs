using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Repositories.Interfaces;
using SI_Andrey_Teodoro_2026.Services.Interfaces;

namespace SI_Andrey_Teodoro_2026.Services;

public class MovimentacaoEstoqueService : BaseService<MovimentacaoEstoqueDto, MovimentacaoEstoqueListDto>,
    IMovimentacaoEstoqueService
{
    private readonly IMovimentacaoEstoqueRepository _repo;
    private readonly IContaPagarService _contaPagarService;

    public MovimentacaoEstoqueService(IMovimentacaoEstoqueRepository repo, IContaPagarService contaPagarService)
    {
        _repo = repo;
        _contaPagarService = contaPagarService;
    }

    protected override string NomeEntidade => "Movimentação de estoque";

    public Task<PaginacaoDto<MovimentacaoEstoqueListDto>> ObterTodosAsync(FiltroConsultaDto filtro)
        => _repo.ObterTodosAsync(filtro);

    public async Task<MovimentacaoEstoqueDto?> ObterPorIdAsync(int id)
    {
        var m = await _repo.ObterPorIdAsync(id);
        if (m == null) return null;
        var itens = await _repo.ObterItensPorMovimentacaoAsync(id);
        return new MovimentacaoEstoqueDto
        {
            Id = m.Id,
            IdOriginal = m.Id,
            TipoMovimentacao = m.TipoMovimentacao,
            Observacao = m.Observacao,
            NumeroNf = m.NumeroNf,
            FornecedorId = m.FornecedorId,
            NomeFornecedor = m.NomeFornecedor,
            CriadoEm = m.CriadoEm,
            Itens = itens.Select(i => new MovimentacaoEstoqueItemDto
            {
                Id = i.Id,
                MovimentacaoId = i.MovimentacaoId,
                ProdutoVariacaoId = i.ProdutoVariacaoId,
                NomeProduto = i.NomeProduto,
                Cor = i.Cor,
                Tamanho = i.Tamanho,
                Quantidade = i.Quantidade,
                ValorUnitario = i.ValorUnitario
            }).ToList()
        };
    }

    public Task<List<MovimentacaoEstoqueItemListDto>> ObterItensAsync(int movimentacaoId)
        => _repo.ObterItensPorMovimentacaoAsync(movimentacaoId);

    public async Task<(bool sucesso, string mensagem, int id)> SalvarAsync(MovimentacaoEstoqueDto dto)
    {
        try
        {
            var itensValidos = dto.Itens.Where(i => !i.Removido).ToList();
            if (itensValidos.Count == 0)
                return (false, "Adicione pelo menos um item à movimentação.", 0);
            if (dto.TipoMovimentacao == "ENTRADA")
            {
                if (string.IsNullOrWhiteSpace(dto.NumeroNf))
                    return (false, "Informe o número da Nota Fiscal para entrada de estoque.", 0);
                if (!dto.FornecedorId.HasValue)
                    return (false, "Selecione o fornecedor para entrada de estoque.", 0);
            }

            foreach (var item in itensValidos)
            {
                if (item.ProdutoVariacaoId == 0)
                    return (false, "Selecione a variação de todos os itens.", 0);
                if (item.ValorUnitario < 0)
                    return (false, "Valor unitário não pode ser negativo.", 0);

                if (dto.TipoMovimentacao == "AJUSTE")
                {
                    if (item.QuantidadeReal < 0)
                        return (false, $"{item.NomeProduto} {item.Cor}/{item.Tamanho}: informe a quantidade real.", 0);
                    var estoqueAtual = await _repo.ObterEstoqueAtualAsync(item.ProdutoVariacaoId);
                    item.EstoqueAtual = estoqueAtual;
                    var deltaCheck = item.QuantidadeReal - estoqueAtual;
                    item.Quantidade = Math.Abs(deltaCheck) == 0 ? 0 : Math.Abs(deltaCheck);
                    if (deltaCheck == 0) { item.Removido = true; continue; }
                }
                else
                {
                    if (item.Quantidade <= 0)
                        return (false, $"{item.NomeProduto} {item.Cor}/{item.Tamanho}: quantidade deve ser maior que zero.", 0);
                    if (dto.TipoMovimentacao == "SAIDA")
                    {
                        var estoqueAtual = await _repo.ObterEstoqueAtualAsync(item.ProdutoVariacaoId);
                        if (estoqueAtual < item.Quantidade)
                            return (false, $"{item.NomeProduto} {item.Cor}/{item.Tamanho}: estoque insuficiente. Disponível: {estoqueAtual} un.", 0);
                    }
                }
            }

            var itensParaGravar = dto.Itens.Where(i => !i.Removido).ToList();
            if (itensParaGravar.Count == 0)
                return (false, "Nenhum item com diferença de estoque encontrado.", 0);

            var movId = await _repo.InserirAsync(dto);

            decimal valorTotalEntrada = 0;

            foreach (var item in itensParaGravar)
            {
                await _repo.InserirItemAsync(item, movId);

                int delta = dto.TipoMovimentacao == "AJUSTE"
                    ? item.QuantidadeReal - item.EstoqueAtual
                    : dto.TipoMovimentacao == "ENTRADA" ? +item.Quantidade : -item.Quantidade;

                await _repo.AtualizarEstoqueAsync(item.ProdutoVariacaoId, delta);

                bool houvEntrada = dto.TipoMovimentacao == "ENTRADA" ||
                                   (dto.TipoMovimentacao == "AJUSTE" && delta > 0);
                if (houvEntrada)
                {
                    if (item.ValorUnitario > 0)
                        await _repo.AtualizarPrecoCustoAsync(item.ProdutoVariacaoId, item.ValorUnitario);
                    await _repo.AtualizarDataUltimaCompraAsync(item.ProdutoVariacaoId, DateTime.Today);

                    if (dto.TipoMovimentacao == "ENTRADA")
                        valorTotalEntrada += item.Quantidade * item.ValorUnitario;
                }
            }
            if (dto.TipoMovimentacao == "ENTRADA" && dto.PrazoPagamentoDias.HasValue && dto.PrazoPagamentoDias > 0
                && valorTotalEntrada > 0)
            {
                await _contaPagarService.GerarContaAutomaticaAsync(
                    dto.FornecedorId, movId, dto.NumeroNf ?? "", DateTime.Today,
                    dto.PrazoPagamentoDias.Value, valorTotalEntrada);
            }

            var tipo = dto.TipoMovimentacao switch
            {
                "ENTRADA" => "entrada",
                "SAIDA" => "saída",
                "AJUSTE" => "ajuste de inventário",
                _ => dto.TipoMovimentacao.ToLower()
            };
            return (true, $"Movimentação de {tipo} registrada com sucesso!", movId);
        }
        catch (Exception ex) { return (false, Erro(ex).mensagem, 0); }
    }
}