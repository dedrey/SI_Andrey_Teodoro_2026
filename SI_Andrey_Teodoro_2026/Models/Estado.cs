namespace SI_Andrey_Teodoro_2026.Models;
public class Estado
{
    public int Id { get; set; }
    public int PaisId { get; set; }
    public string NomeEstado { get; set; } = string.Empty;
    public string Uf { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; } = DateTime.Now;
    public DateTime? AtualizadoEm { get; set; }
    public string? NomePais { get; set; }
    public string? NomeAtualizadoPor { get; set; }
}