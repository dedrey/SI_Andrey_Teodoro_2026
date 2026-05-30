using System.ComponentModel.DataAnnotations;

namespace SI_Andrey_Teodoro_2026.DTOs;

public class MetodoPagamentoDto
{
    public int Id { get; set; }
    public int IdOriginal { get; set; }

    [Required(ErrorMessage = "Código é obrigatório")]
    [MinLength(1, ErrorMessage = "Deve ter pelo menos 1 caractere")]
    [MaxLength(4, ErrorMessage = "Deve ter no máximo 4 caracteres")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nome do método é obrigatório")]
    [MinLength(2, ErrorMessage = "Deve ter pelo menos 2 caracteres")]
    [MaxLength(25, ErrorMessage = "Deve ter no máximo 25 caracteres")]
    public string MetodoPagamento { get; set; } = string.Empty;

    public bool Ativo { get; set; } = true;
    public DateTime? AtualizadoEm { get; set; }
    public string? NomeAtualizadoPor { get; set; }
}