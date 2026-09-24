using System.Data;
using SI_Andrey_Teodoro_2026.Data;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Repositories.Interfaces;
using SI_Andrey_Teodoro_2026.Services.Interfaces;

namespace SI_Andrey_Teodoro_2026.Services;

public class MovimentacaoEstoqueService : BaseService<MovimentacaoEstoqueDto, MovimentacaoEstoqueListDto>,
    IMovimentacaoEstoqueService
{
    private readonly IMovimentacaoEstoqueRepository _repo;
    private readonly DbConnectionFactory _factory;

    public MovimentacaoEstoqueService(IMovimentacaoEstoqueRepository repo, DbConnectionFactory factory)
    {
        _repo = repo;
        _factory = factory;
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
            CompraId = m.CompraId,
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
            if (dto.TipoMovimentacao == "ENTRADA")
                return (false, "Entradas de estoque são geradas automaticamente pelo lançamento de Compras.", 0);
            if (dto.TipoMovimentacao is not ("SAIDA" or "AJUSTE"))
                return (false, "Tipo de movimentação inválido.", 0);

            dto.Observacao = string.IsNullOrWhiteSpace(dto.Observacao) ? null : dto.Observacao.Trim();
            if (dto.Observacao?.Length > 200)
                return (false, "A observação deve ter no máximo 200 caracteres.", 0);

            var itensValidos = dto.Itens.Where(i => !i.Removido).ToList();
            if (itensValidos.Count == 0)
                return (false, "Adicione pelo menos um item à movimentação.", 0);

            foreach (var item in itensValidos)
            {
                if (item.ProdutoVariacaoId == 0)
                    return (false, "Selecione a variação de todos os itens.", 0);
                if (item.ValorUnitario < 0)
                    return (false, $"{Desc(item)}: valor unitário não pode ser negativo.", 0);

                if (dto.TipoMovimentacao == "AJUSTE")
                {
                    if (item.QuantidadeReal < 0)
                        return (false, $"{Desc(item)}: informe a quantidade real.", 0);
                }
                else if (item.Quantidade <= 0)
                    return (false, $"{Desc(item)}: quantidade deve ser maior que zero.", 0);
            }

            var duplicada = itensValidos.GroupBy(i => i.ProdutoVariacaoId).FirstOrDefault(g => g.Count() > 1);
            if (duplicada != null)
                return (false, $"{Desc(duplicada.First())} foi adicionado mais de uma vez. " +
                               "Junte as quantidades em um único item.", 0);

            using var conn = _factory.CreateConnection();
            if (conn.State != ConnectionState.Open) conn.Open();
            using var tx = conn.BeginTransaction();

            var movId = await _repo.InserirAsync(dto.TipoMovimentacao, dto.Observacao, null, tx);
            var gravados = 0;

            foreach (var item in itensValidos)
            {
                int delta;
                if (dto.TipoMovimentacao == "AJUSTE")
                {
                    var estoqueAtual = await _repo.ObterEstoqueAtualAsync(item.ProdutoVariacaoId, tx);
                    delta = item.QuantidadeReal - estoqueAtual;
                    if (delta == 0) continue;
                }
                else
                {
                    delta = -item.Quantidade;
                }

                if (!await _repo.AtualizarEstoqueAsync(item.ProdutoVariacaoId, delta, tx))
                {
                    var disponivel = await _repo.ObterEstoqueAtualAsync(item.ProdutoVariacaoId, tx);
                    return (false, $"{Desc(item)}: estoque insuficiente. Disponível: {disponivel} un.", 0);
                }

                await _repo.InserirItemAsync(movId, item.ProdutoVariacaoId, Math.Abs(delta), item.ValorUnitario, tx);
                gravados++;
            }

            if (gravados == 0)
                return (false, "Nenhum item com diferença de estoque encontrado.", 0);

            tx.Commit();

            var tipo = dto.TipoMovimentacao == "SAIDA" ? "saída" : "ajuste de inventário";
            return (true, $"Movimentação de {tipo} registrada com sucesso!", movId);
        }
        catch (Exception ex) { return (false, Erro(ex).mensagem, 0); }
    }

    private static string Desc(MovimentacaoEstoqueItemDto i) => $"{i.NomeProduto} {i.Cor}/{i.Tamanho}";
}