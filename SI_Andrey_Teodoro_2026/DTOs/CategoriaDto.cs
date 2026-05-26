using System.ComponentModel.DataAnnotations;

namespace SI_Andrey_Teodoro_2026.DTOs;

public class CategoriaDto
{
    public int Id { get; set; }
    public int IdOriginal { get; set; }

    [Required(ErrorMessage = "Nome da categoria é obrigatório")]
    [MinLength(2, ErrorMessage = "Deve ter pelo menos 2 caracteres")]
    [MaxLength(50, ErrorMessage = "Deve ter no máximo 50 caracteres")]
    public string NomeCategoria { get; set; } = string.Empty;

    public bool Ativo { get; set; } = true;
    public DateTime? AtualizadoEm { get; set; }
    public string? NomeAtualizadoPor { get; set; }
}