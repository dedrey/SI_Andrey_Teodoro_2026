namespace SI_Andrey_Teodoro_2026.DTOs;

public class ProdutoVariacaoListDto
{
    public int Id { get; set; }
    public int ProdutoId { get; set; }
    public string NomeProduto { get; set; } = string.Empty;
    public string Cor { get; set; } = string.Empty;
    public string Tamanho { get; set; } = string.Empty;
    public string? CodigoBarras { get; set; }
    public decimal Preco { get; set; }
    public int QuantidadeEstoque { get; set; }
    public bool Ativo { get; set; }
    public DateTime CriadoEm { get; set; }
}