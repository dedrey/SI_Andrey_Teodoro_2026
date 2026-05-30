namespace SI_Andrey_Teodoro_2026.Models;

public class CondicaoPagamento
{
    public int Id { get; set; }
    public string NomeCondicaoPagamento { get; set; } = string.Empty;
    public int MetodoPagamentoId { get; set; }
    public string? NomeMetodoPagamento { get; set; }
    public string? CodigoMetodo { get; set; }
    public int NumeroParcelas { get; set; } = 1;
    public decimal EntradaMinimaPercentual { get; set; } = 0;
    public decimal DescontoPercentual { get; set; } = 0;
    public decimal AcrescimoPercentual { get; set; } = 0;
    public decimal MultaPercentual { get; set; } = 0;
    public decimal TaxaJurosPercentual { get; set; } = 0;
    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; } = DateTime.Now;
    public DateTime? AtualizadoEm { get; set; }
    public string? NomeAtualizadoPor { get; set; }
}