using System.ComponentModel.DataAnnotations;

namespace SI_Andrey_Teodoro_2026.DTOs;

public class UnidadeMedidaDto
{
    public int Id { get; set; }
    public int IdOriginal { get; set; }
    [Required(ErrorMessage = "Sigla é obrigatória")]
    [MinLength(1, ErrorMessage = "Sigla deve ter pelo menos 1 caractere")]
    [MaxLength(6, ErrorMessage = "Sigla deve ter no máximo 6 caracteres")]
    public string Sigla { get; set; } = string.Empty;
    [Required(ErrorMessage = "Descrição é obrigatória")]
    [MinLength(2, ErrorMessage = "Descrição deve ter pelo menos 2 caracteres")]
    [MaxLength(50, ErrorMessage = "Descrição deve ter no máximo 50 caracteres")]
    public string Descricao { get; set; } = string.Empty;

    public bool Ativo { get; set; } = true;
    public DateTime? AtualizadoEm { get; set; }
    public string? NomeAtualizadoPor { get; set; }
}