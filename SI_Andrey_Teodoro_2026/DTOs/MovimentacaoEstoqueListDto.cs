namespace SI_Andrey_Teodoro_2026.DTOs;

public class MovimentacaoEstoqueListDto
{
    public int Id { get; set; }
    public string TipoMovimentacao { get; set; } = string.Empty;
    public string? Observacao { get; set; }
    public int TotalItens { get; set; }
    public int TotalQuantidade { get; set; }
    public decimal ValorTotal { get; set; }
    public DateTime CriadoEm { get; set; }
    public string? NomeCriadoPor { get; set; }
}

public class MovimentacaoEstoqueItemListDto
{
    public int Id { get; set; }
    public int MovimentacaoId { get; set; }
    public int ProdutoVariacaoId { get; set; }
    public string NomeProduto { get; set; } = string.Empty;
    public string Cor { get; set; } = string.Empty;
    public string Tamanho { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal ValorTotal { get; set; }
}