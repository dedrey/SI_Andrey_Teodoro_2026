using System.ComponentModel.DataAnnotations;

namespace SI_Andrey_Teodoro_2026.DTOs;

public class PaisDto
{
    public int Id { get; set; }
    public int IdOriginal { get; set; }

    [Required(ErrorMessage = "DDI é obrigatório")]
    [MaxLength(5)]
    public string Ddi { get; set; } = string.Empty;

    [Required(ErrorMessage = "Sigla é obrigatória")]
    [MinLength(1)]
    [MaxLength(3)]
    public string Sigla { get; set; } = string.Empty;

    [Required(ErrorMessage = "Moeda é obrigatória")]
    [MaxLength(30)]
    public string Moeda { get; set; } = string.Empty;

    [Required(ErrorMessage = "Símbolo é obrigatório")]
    [MaxLength(5)]
    public string SimboleMoeda { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nome do país é obrigatório")]
    [MinLength(2)]
    [MaxLength(60)]
    public string NomePais { get; set; } = string.Empty;

    public bool Ativo { get; set; } = true;
    public DateTime? AtualizadoEm { get; set; }
}