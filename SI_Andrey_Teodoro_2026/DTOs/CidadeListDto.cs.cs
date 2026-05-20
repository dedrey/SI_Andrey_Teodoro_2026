namespace SI_Andrey_Teodoro_2026.DTOs;

public class CidadeListDto
{
    public int Id { get; set; }
    public string NomeCidade { get; set; } = string.Empty;
    public short Ddd { get; set; }
    public int EstadoId { get; set; }
    public string NomeEstado { get; set; } = string.Empty;
    public string NomePais { get; set; } = string.Empty;
    public string Uf { get; set; } = string.Empty;
    public string Ddi { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public DateTime CriadoEm { get; set; }
}