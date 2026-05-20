using System.ComponentModel.DataAnnotations;

namespace SI_Andrey_Teodoro_2026.DTOs;

public class PaisDto
{
    public int Id { get; set; }
    public int IdOriginal { get; set; }

    [Required(ErrorMessage = "DDI é obrigatório")]
    [MaxLength(5, ErrorMessage = "DDI deve ter no máximo 5 caracteres")]
    public string Ddi { get; set; } = string.Empty;

    [Required(ErrorMessage = "Sigla é obrigatória")]
    [MinLength(1, ErrorMessage = "Sigla deve ter pelo menos 1 caractere")]
    [MaxLength(3, ErrorMessage = "Sigla deve ter no máximo 3 caracteres")]
    public string Sigla { get; set; } = string.Empty;

    [Required(ErrorMessage = "Moeda é obrigatória")]
    [MaxLength(30, ErrorMessage = "Moeda deve ter no máximo 30 caracteres")]
    public string Moeda { get; set; } = string.Empty;

    [Required(ErrorMessage = "Símbolo da moeda é obrigatório")]
    [MaxLength(5, ErrorMessage = "Símbolo deve ter no máximo 5 caracteres")]
    public string SimboleMoeda { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nome do país é obrigatório")]
    [MinLength(2, ErrorMessage = "Nome deve ter pelo menos 2 caracteres")]
    [MaxLength(60, ErrorMessage = "Nome deve ter no máximo 60 caracteres")]
    public string NomePais { get; set; } = string.Empty;

    public bool Ativo { get; set; } = true;
}