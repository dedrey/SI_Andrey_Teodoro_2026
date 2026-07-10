namespace SI_Andrey_Teodoro_2026.Models;

public class Produto
{
    public int Id { get; set; }
    public string NomeProduto { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string? CodigoBarras { get; set; }
    public decimal PrecoCompra { get; set; }
    public decimal Frete { get; set; }
    public decimal PrecoCusto { get; set; }
    public int CategoriaId { get; set; }
    public string? NomeCategoria { get; set; }
    public int MarcaId { get; set; }
    public string? NomeMarca { get; set; }
    public int UnidadeMedidaId { get; set; }
    public string? SiglaUnidade { get; set; }
    public int? FornecedorId { get; set; }
    public string? NomeFornecedor { get; set; }
    public bool Ativo { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime? AtualizadoEm { get; set; }
    public string? NomeAtualizadoPor { get; set; }
    public string? NumeroNfUltimaEntrada { get; set; }

}