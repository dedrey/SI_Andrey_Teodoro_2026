using System.ComponentModel.DataAnnotations;

namespace SI_Andrey_Teodoro_2026.DTOs;

public class CondicaoPagamentoDto
{
    public int Id { get; set; }
    public int IdOriginal { get; set; }

    [Required(ErrorMessage = "Condição de pagamento é obrigatória")]
    [MinLength(2, ErrorMessage = "Deve ter pelo menos 2 caracteres")]
    [MaxLength(50, ErrorMessage = "Deve ter no máximo 50 caracteres")]
    public string CondicaoPagamento { get; set; } = string.Empty;

    [Required(ErrorMessage = "Método de pagamento é obrigatório")]
    [Range(1, int.MaxValue, ErrorMessage = "Selecione um método de pagamento válido")]
    public int MetodoPagamentoId { get; set; }

    [Required(ErrorMessage = "Número de parcelas é obrigatório")]
    [Range(1, 999, ErrorMessage = "Número de parcelas deve ser entre 1 e 999")]
    public int NumeroParcelas { get; set; } = 1;

    [Range(0, 100, ErrorMessage = "Entrada mínima deve ser entre 0 e 100")]
    public decimal EntradaMinimaPercentual { get; set; } = 0;

    [Range(0, 100, ErrorMessage = "Desconto deve ser entre 0 e 100")]
    public decimal DescontoPercentual { get; set; } = 0;

    [Range(0, 100, ErrorMessage = "Acréscimo deve ser entre 0 e 100")]
    public decimal AcrescimoPercentual { get; set; } = 0;

    [Range(0, 100, ErrorMessage = "Multa deve ser entre 0 e 100")]
    public decimal MultaPercentual { get; set; } = 0;

    [Range(0, 100, ErrorMessage = "Taxa de juros deve ser entre 0 e 100")]
    public decimal TaxaJurosPercentual { get; set; } = 0;

    public bool Ativo { get; set; } = true;
    public DateTime? AtualizadoEm { get; set; }
    public string? NomeAtualizadoPor { get; set; }
}