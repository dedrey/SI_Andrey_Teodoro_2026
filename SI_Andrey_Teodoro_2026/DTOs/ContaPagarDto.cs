using System.ComponentModel.DataAnnotations;
namespace SI_Andrey_Teodoro_2026.DTOs;

public class ContaPagarDto
{
    public int Id { get; set; }
    public int IdOriginal { get; set; }

    public int? FornecedorId { get; set; }
    public string? NomeFornecedor { get; set; }

    public int? MovimentacaoId { get; set; }
    public string? NumeroNfMovimentacao { get; set; }

    [Required(ErrorMessage = "Descrição é obrigatória")]
    [MinLength(3)]
    [MaxLength(150)]
    public string Descricao { get; set; } = string.Empty;

    [Required(ErrorMessage = "Data de vencimento é obrigatória")]
    public DateTime DataVencimento { get; set; } = DateTime.Today.AddDays(30);

    public DateTime? DataPagamento { get; set; }
    public string? ComprovanteArquivo { get; set; }

    [Range(0.01, 999999999, ErrorMessage = "Valor deve ser maior que zero")]
    public decimal ValorOriginal { get; set; }

    public decimal ValorSaldo { get; set; }

    public string Status { get; set; } = "ABERTA";

    public DateTime? AtualizadoEm { get; set; }
    public string? NomeAtualizadoPor { get; set; }
}

public class ContaPagarListDto
{
    public int Id { get; set; }
    public string? NomeFornecedor { get; set; }
    public int? MovimentacaoId { get; set; }
    public string? NumeroNfMovimentacao { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public DateTime DataVencimento { get; set; }
    public DateTime? DataPagamento { get; set; }
    public string? ComprovanteArquivo { get; set; }
    public decimal ValorOriginal { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CriadoEm { get; set; }
}