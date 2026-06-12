namespace SI_Andrey_Teodoro_2026.DTOs;

public class VeiculoListDto
{
    public int Id { get; set; }
    public int TransportadoraId { get; set; }
    public string NomeTransportadora { get; set; } = string.Empty;
    public string Placa { get; set; } = string.Empty;
    public string Uf { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public DateTime CriadoEm { get; set; }
    public string? MotivoCancelamento { get; set; }
}