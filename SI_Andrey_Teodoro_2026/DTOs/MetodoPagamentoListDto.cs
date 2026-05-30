namespace SI_Andrey_Teodoro_2026.DTOs;

public class MetodoPagamentoListDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string MetodoPagamento { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public DateTime CriadoEm { get; set; }
}