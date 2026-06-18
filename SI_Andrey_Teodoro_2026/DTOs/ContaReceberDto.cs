using System.ComponentModel.DataAnnotations;
namespace SI_Andrey_Teodoro_2026.DTOs;

public class ContaReceberDto
{
    public int Id { get; set; }
    public int IdOriginal { get; set; }

    [Required(ErrorMessage = "Cliente é obrigatório")]
    [Range(1, int.MaxValue, ErrorMessage = "Selecione um cliente")]
    public int ClienteId { get; set; }
    public string? NomeCliente { get; set; }

    public int? VendaId { get; set; }

    [Required(ErrorMessage = "Descrição é obrigatória")]
    [MinLength(3)]
    [MaxLength(100)]
    public string Descricao { get; set; } = string.Empty;

    [Required(ErrorMessage = "Data de vencimento é obrigatória")]
    public DateTime DataVencimento { get; set; } = DateTime.Today.AddDays(30);

    [Range(0.01, 999999999, ErrorMessage = "Valor deve ser maior que zero")]
    public decimal ValorOriginal { get; set; }

    public decimal ValorSaldo { get; set; }
    public decimal ValorRecebido => ValorOriginal - ValorSaldo;

    public string Status { get; set; } = "ABERTA";

    public DateTime? AtualizadoEm { get; set; }
    public string? NomeAtualizadoPor { get; set; }

    public List<ContaReceberBaixaDto> Baixas { get; set; } = new();
}

public class ContaReceberBaixaDto
{
    public int Id { get; set; }
    public int ContaReceberId { get; set; }
    public DateTime DataRecebimento { get; set; } = DateTime.Today;
    [Range(0.01, 999999999, ErrorMessage = "Valor deve ser maior que zero")]
    public decimal ValorRecebido { get; set; }
    public string? ComprovanteArquivo { get; set; }
    public string? Observacao { get; set; }
    public DateTime CriadoEm { get; set; }
}

public class ContaReceberListDto
{
    public int Id { get; set; }
    public string? NomeCliente { get; set; }
    public int? VendaId { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public DateTime DataVencimento { get; set; }
    public decimal ValorOriginal { get; set; }
    public decimal ValorSaldo { get; set; }
    public decimal ValorRecebido => ValorOriginal - ValorSaldo;
    public DateTime? DataUltimoRecebimento { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CriadoEm { get; set; }
}