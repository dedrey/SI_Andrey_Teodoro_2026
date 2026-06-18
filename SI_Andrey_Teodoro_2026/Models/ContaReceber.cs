namespace SI_Andrey_Teodoro_2026.Models;

public class ContaReceber
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public string? NomeCliente { get; set; }
    public int? VendaId { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public DateTime DataVencimento { get; set; }
    public decimal ValorOriginal { get; set; }
    public decimal ValorSaldo { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CriadoEm { get; set; }
    public int? CriadoPor { get; set; }
    public DateTime AtualizadoEm { get; set; }
    public string? NomeAtualizadoPor { get; set; }
}

public class ContaReceberBaixa
{
    public int Id { get; set; }
    public int ContaReceberId { get; set; }
    public DateTime DataRecebimento { get; set; }
    public decimal ValorRecebido { get; set; }
    public string? ComprovanteArquivo { get; set; }
    public string? Observacao { get; set; }
    public DateTime CriadoEm { get; set; }
}