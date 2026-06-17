namespace SI_Andrey_Teodoro_2026.Models;

public class ContaPagar
{
    public int Id { get; set; }
    public int? FornecedorId { get; set; }
    public string? NomeFornecedor { get; set; }
    public int? MovimentacaoId { get; set; }
    public string? NumeroNfMovimentacao { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public DateTime DataVencimento { get; set; }
    public DateTime? DataPagamento { get; set; }
    public string? ComprovanteArquivo { get; set; }
    public decimal ValorOriginal { get; set; }
    public decimal ValorSaldo { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CriadoEm { get; set; }
    public int? CriadoPor { get; set; }
    public DateTime AtualizadoEm { get; set; }
    public string? NomeAtualizadoPor { get; set; }
}