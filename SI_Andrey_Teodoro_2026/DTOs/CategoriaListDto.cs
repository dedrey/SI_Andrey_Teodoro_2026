namespace SI_Andrey_Teodoro_2026.DTOs;

public class CategoriaListDto
{
    public int Id { get; set; }
    public string NomeCategoria { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public DateTime CriadoEm { get; set; }
}