using System.ComponentModel.DataAnnotations;

namespace SI_Andrey_Teodoro_2026.DTOs;

public class VeiculoDto
{
    public int Id { get; set; }
    public int IdOriginal { get; set; }

    [Required(ErrorMessage = "Transportadora é obrigatória")]
    [Range(1, int.MaxValue, ErrorMessage = "Selecione uma transportadora")]
    public int TransportadoraId { get; set; }
    [Required(ErrorMessage = "Placa é obrigatória")]
    [MinLength(7, ErrorMessage = "Placa deve ter 7 caracteres")]
    [MaxLength(7, ErrorMessage = "Placa deve ter 7 caracteres")]
    public string Placa { get; set; } = string.Empty;

    [Required(ErrorMessage = "UF é obrigatória")]
    [MinLength(2, ErrorMessage = "UF deve ter 2 letras")]
    [MaxLength(2, ErrorMessage = "UF deve ter 2 letras")]
    public string Uf { get; set; } = string.Empty;

    public bool Ativo { get; set; } = true;
    public DateTime? AtualizadoEm { get; set; }
    public string? NomeAtualizadoPor { get; set; }
}