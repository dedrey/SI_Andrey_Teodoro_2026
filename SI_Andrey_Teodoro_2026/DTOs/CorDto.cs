using System.ComponentModel.DataAnnotations;
namespace SI_Andrey_Teodoro_2026.DTOs;

public class CorDto
{
    public int Id { get; set; }
    public int IdOriginal { get; set; }

    [Required(ErrorMessage = "Nome da cor é obrigatório")]
    [MinLength(2, ErrorMessage = "Mínimo 2 caracteres")]
    [MaxLength(30, ErrorMessage = "Máximo 30 caracteres")]
    public string Nome { get; set; } = string.Empty;

    public bool Ativo { get; set; } = true;
}

public class CorListDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public bool Ativo { get; set; }
}