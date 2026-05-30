namespace SI_Andrey_Teodoro_2026.Models;

public class Veiculo
{
    public int Id { get; set; }
    public int TransportadoraId { get; set; }
    public string? NomeTransportadora { get; set; }
    public string Placa { get; set; } = string.Empty;
    public string Uf { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; } = DateTime.Now;
    public DateTime? AtualizadoEm { get; set; }
    public string? NomeAtualizadoPor { get; set; }
}