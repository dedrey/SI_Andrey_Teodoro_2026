namespace SI_Andrey_Teodoro_2026.DTOs;

public class MarcaListDto
{
    public int Id { get; set; }
    public string NomeMarca { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public DateTime CriadoEm { get; set; }
}