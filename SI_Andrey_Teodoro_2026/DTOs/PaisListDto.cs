namespace SI_Andrey_Teodoro_2026.DTOs;

public class PaisListDto
{
    public int Id { get; set; }
    public string Ddi { get; set; } = string.Empty;
    public string Sigla { get; set; } = string.Empty;
    public string Moeda { get; set; } = string.Empty;
    public string SimboleMoeda { get; set; } = string.Empty;
    public string NomePais { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public DateTime CriadoEm { get; set; }
}