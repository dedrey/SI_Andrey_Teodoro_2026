namespace SI_Andrey_Teodoro_2026.DTOs;

public class ProdutoListDto
{
    public int Id { get; set; }
    public string Produto { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string? CodigoBarras { get; set; }
    public int CategoriaId { get; set; }
    public string NomeCategoria { get; set; } = string.Empty;
    public int MarcaId { get; set; }
    public string NomeMarca { get; set; } = string.Empty;
    public int UnidadeMedidaId { get; set; }
    public string SiglaUnidade { get; set; } = string.Empty;
    public int? FornecedorId { get; set; }
    public string? NomeFornecedor { get; set; }
    public int TotalVariacoes { get; set; }
    public int TotalEstoque { get; set; }
    public bool Ativo { get; set; }
    public DateTime CriadoEm { get; set; }
}