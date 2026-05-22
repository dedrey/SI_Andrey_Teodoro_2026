namespace SI_Andrey_Teodoro_2026.Models;

public class Pais
{
    public int Id { get; set; }
    public string Ddi { get; set; } = string.Empty;
    public string Sigla { get; set; } = string.Empty;
    public string Moeda { get; set; } = string.Empty;
    public string SimboleMoeda { get; set; } = string.Empty;
    public string NomePais { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; } = DateTime.Now;
    public DateTime? AtualizadoEm { get; set; }
}