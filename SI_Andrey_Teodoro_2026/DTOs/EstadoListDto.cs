namespace SI_Andrey_Teodoro_2026.DTOs;

public class EstadoListDto
{
    public int Id { get; set; }
    public int PaisId { get; set; }
    public string NomeEstado { get; set; } = string.Empty;
    public string Uf { get; set; } = string.Empty;
    public string NomePais { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public DateTime CriadoEm { get; set; }
}