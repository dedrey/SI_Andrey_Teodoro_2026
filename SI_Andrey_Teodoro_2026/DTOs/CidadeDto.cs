using System.ComponentModel.DataAnnotations;

namespace SI_Andrey_Teodoro_2026.DTOs;

public class CidadeDto
{
    public int Id { get; set; }
    public int IdOriginal { get; set; }

    [Required(ErrorMessage = "Nome da cidade é obrigatório")]
    [MinLength(2)]
    [MaxLength(50)]
    public string NomeCidade { get; set; } = string.Empty;

    [Required(ErrorMessage = "DDD é obrigatório")]
    [Range(1, 999, ErrorMessage = "DDD deve ser entre 1 e 999")]
    public short Ddd { get; set; }

    [Required(ErrorMessage = "Estado é obrigatório")]
    [Range(1, int.MaxValue, ErrorMessage = "Selecione um estado válido")]
    public int EstadoId { get; set; }

    public int PaisId { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime? AtualizadoEm { get; set; }   // ← novo
}