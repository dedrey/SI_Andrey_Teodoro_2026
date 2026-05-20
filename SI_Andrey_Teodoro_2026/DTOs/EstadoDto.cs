using System.ComponentModel.DataAnnotations;

namespace SI_Andrey_Teodoro_2026.DTOs;

public class EstadoDto
{
    public int Id { get; set; }
    public int IdOriginal { get; set; }

    [Required(ErrorMessage = "País é obrigatório")]
    [Range(1, int.MaxValue, ErrorMessage = "Selecione um país válido")]
    public int PaisId { get; set; }

    [Required(ErrorMessage = "Nome do estado é obrigatório")]
    [MinLength(2, ErrorMessage = "Nome deve ter pelo menos 2 caracteres")]
    [MaxLength(50, ErrorMessage = "Nome deve ter no máximo 50 caracteres")]
    public string NomeEstado { get; set; } = string.Empty;

    [Required(ErrorMessage = "UF é obrigatória")]
    [MinLength(2, ErrorMessage = "UF deve ter 2 caracteres")]
    [MaxLength(2, ErrorMessage = "UF deve ter 2 caracteres")]
    public string Uf { get; set; } = string.Empty;

    public bool Ativo { get; set; } = true;
}