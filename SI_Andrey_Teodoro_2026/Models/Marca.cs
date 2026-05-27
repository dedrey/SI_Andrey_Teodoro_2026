namespace SI_Andrey_Teodoro_2026.Models;

public class Marca
{
    public int Id { get; set; }
    public string NomeMarca { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; } = DateTime.Now;
    public DateTime? AtualizadoEm { get; set; }
    public string? NomeAtualizadoPor { get; set; }
}