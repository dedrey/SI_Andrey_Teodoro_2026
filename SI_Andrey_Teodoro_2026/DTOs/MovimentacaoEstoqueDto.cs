namespace SI_Andrey_Teodoro_2026.DTOs;

public class MovimentacaoEstoqueDto
{
    public int Id { get; set; }
    public int IdOriginal { get; set; }
    public string TipoMovimentacao { get; set; } = "ENTRADA";
    public string? Observacao { get; set; }
    public string? NumeroNf { get; set; }
    public int? FornecedorId { get; set; }
    public string? NomeFornecedor { get; set; }
    public DateTime CriadoEm { get; set; }
    public List<MovimentacaoEstoqueItemDto> Itens { get; set; } = new();
}

public class MovimentacaoEstoqueItemDto
{
    public int Id { get; set; }
    public int MovimentacaoId { get; set; }
    public int ProdutoVariacaoId { get; set; }
    public int Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
    public int ProdutoId { get; set; }
    public string NomeProduto { get; set; } = string.Empty;
    public string Cor { get; set; } = string.Empty;
    public string Tamanho { get; set; } = string.Empty;
    public int EstoqueAtual { get; set; }
    public bool Removido { get; set; } = false;
    public int QuantidadeReal { get; set; } = -1;
}