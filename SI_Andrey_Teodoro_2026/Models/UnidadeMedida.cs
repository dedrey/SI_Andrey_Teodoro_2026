namespace SI_Andrey_Teodoro_2026.Models;

public class UnidadeMedida
{
    public int Id { get; set; }
    public string Sigla { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; } = DateTime.Now;
    public DateTime? AtualizadoEm { get; set; }
    public string? NomeAtualizadoPor { get; set; }
}