namespace SI_Andrey_Teodoro_2026.Models;

public class MovimentacaoEstoque
{
    public int Id { get; set; }
    public string TipoMovimentacao { get; set; } = string.Empty;
    public string? Observacao { get; set; }
    public DateTime CriadoEm { get; set; }
    public int? CriadoPor { get; set; }
    public string? NomeCriadoPor { get; set; }
}

public class MovimentacaoEstoqueItem
{
    public int Id { get; set; }
    public int MovimentacaoId { get; set; }
    public int ProdutoVariacaoId { get; set; }
    public string NomeProduto { get; set; } = string.Empty;
    public string Cor { get; set; } = string.Empty;
    public string Tamanho { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
}