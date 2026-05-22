namespace SI_Andrey_Teodoro_2026.Models;

public class Cidade
{
    public int Id { get; set; }
    public string NomeCidade { get; set; } = string.Empty;
    public short Ddd { get; set; }
    public int EstadoId { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; } = DateTime.Now;
    public DateTime? AtualizadoEm { get; set; }
    public string? NomeEstado { get; set; }
    public string? NomePais { get; set; }
    public string? Uf { get; set; }
}