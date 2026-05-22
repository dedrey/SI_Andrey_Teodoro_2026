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
    [MinLength(2)]
    [MaxLength(50)]
    public string NomeEstado { get; set; } = string.Empty;

    [Required(ErrorMessage = "UF é obrigatória")]
    [MinLength(2)]
    [MaxLength(2)]
    public string Uf { get; set; } = string.Empty;

    public bool Ativo { get; set; } = true;
    public DateTime? AtualizadoEm { get; set; }   // ← novo
}