namespace SI_Andrey_Teodoro_2026.DTOs;

public class CondicaoPagamentoListDto
{
    public int Id { get; set; }
    public string CondicaoPagamento { get; set; } = string.Empty;
    public int MetodoPagamentoId { get; set; }
    public string NomeMetodoPagamento { get; set; } = string.Empty;
    public string CodigoMetodo { get; set; } = string.Empty;
    public int NumeroParcelas { get; set; }
    public decimal EntradaMinimaPercentual { get; set; }
    public decimal DescontoPercentual { get; set; }
    public decimal AcrescimoPercentual { get; set; }
    public decimal MultaPercentual { get; set; }
    public decimal TaxaJurosPercentual { get; set; }
    public bool Ativo { get; set; }
    public DateTime CriadoEm { get; set; }
}