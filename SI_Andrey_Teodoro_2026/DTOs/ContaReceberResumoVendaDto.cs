namespace SI_Andrey_Teodoro_2026.DTOs;
public class ContaReceberResumoVendaDto
{
    public int Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public DateTime DataVencimento { get; set; }
    public decimal ValorOriginal { get; set; }
    public decimal ValorRecebido { get; set; }
    public decimal ValorSaldo { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? DataUltimoRecebimento { get; set; }
    public List<ContaReceberBaixaDto> Baixas { get; set; } = new();
}