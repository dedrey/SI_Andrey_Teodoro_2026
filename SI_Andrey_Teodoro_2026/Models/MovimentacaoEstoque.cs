namespace SI_Andrey_Teodoro_2026.Models;

public class MovimentacaoEstoque
{
    public int Id { get; set; }
    public string TipoMovimentacao { get; set; } = string.Empty;
    public string? Observacao { get; set; }
    public int? CompraId { get; set; }
    public int? VendaId { get; set; }
    public DateTime CriadoEm { get; set; }
    public int? CriadoPor { get; set; }
    public string? NomeCriadoPor { get; set; }
}