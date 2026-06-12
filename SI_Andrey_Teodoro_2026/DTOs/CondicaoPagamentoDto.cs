using System.ComponentModel.DataAnnotations;
namespace SI_Andrey_Teodoro_2026.DTOs;

public class CondicaoPagamentoParcelaDto
{
    public int Id { get; set; }
    public int NumeroParcela { get; set; }

    [Range(1, 3650, ErrorMessage = "Dias de vencimento deve ser entre 1 e 3650")]
    public int DiasVencimento { get; set; }
}

public class CondicaoPagamentoDto
{
    public int Id { get; set; }
    public int IdOriginal { get; set; }

    [Required(ErrorMessage = "Condição de pagamento é obrigatória")]
    [MinLength(2)]
    [MaxLength(50)]
    public string CondicaoPagamento { get; set; } = string.Empty;

    [Required(ErrorMessage = "Método de pagamento é obrigatório")]
    [Range(1, int.MaxValue, ErrorMessage = "Selecione um método de pagamento válido")]
    public int MetodoPagamentoId { get; set; }

    [Required]
    [Range(1, 999)]
    public int NumeroParcelas { get; set; } = 1;

    [Range(0, 100)] public decimal EntradaMinimaPercentual { get; set; } = 0;
    [Range(0, 100)] public decimal DescontoPercentual { get; set; } = 0;
    [Range(0, 100)] public decimal AcrescimoPercentual { get; set; } = 0;
    [Range(0, 100)] public decimal MultaPercentual { get; set; } = 0;
    [Range(0, 100)] public decimal TaxaJurosPercentual { get; set; } = 0;

    public bool Ativo { get; set; } = true;
    public DateTime? AtualizadoEm { get; set; }
    public string? NomeAtualizadoPor { get; set; }
    public List<CondicaoPagamentoParcelaDto> Parcelas { get; set; } = new();
}

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