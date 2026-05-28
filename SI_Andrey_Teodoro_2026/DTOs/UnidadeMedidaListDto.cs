namespace SI_Andrey_Teodoro_2026.DTOs;

public class UnidadeMedidaListDto
{
    public int Id { get; set; }
    public string Sigla { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public DateTime CriadoEm { get; set; }
}