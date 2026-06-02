namespace SI_Andrey_Teodoro_2026.Models;

public class ProdutoVariacao
{
    public int Id { get; set; }
    public int ProdutoId { get; set; }
    public string? NomeProduto { get; set; }
    public string Cor { get; set; } = string.Empty;
    public string Tamanho { get; set; } = string.Empty;
    public string? CodigoBarras { get; set; }
    public decimal Preco { get; set; }
    public int QuantidadeEstoque { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; } = DateTime.Now;
    public DateTime? AtualizadoEm { get; set; }
    public string? NomeAtualizadoPor { get; set; }
}