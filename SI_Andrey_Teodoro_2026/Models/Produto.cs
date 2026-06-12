namespace SI_Andrey_Teodoro_2026.Models;

public class Produto
{
    public int Id { get; set; }
    public string NomeProduto { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public int CategoriaId { get; set; }
    public string? NomeCategoria { get; set; }
    public int MarcaId { get; set; }
    public string? NomeMarca { get; set; }
    public int UnidadeMedidaId { get; set; }
    public string? SiglaUnidade { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; } = DateTime.Now;
    public DateTime? AtualizadoEm { get; set; }
    public string? NomeAtualizadoPor { get; set; }
    public int? FornecedorId { get; set; }
}