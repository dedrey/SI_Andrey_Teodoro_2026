using System.ComponentModel.DataAnnotations;
namespace SI_Andrey_Teodoro_2026.DTOs;

public class TamanhoDto
{
    public int Id { get; set; }
    public int IdOriginal { get; set; }

    [Required(ErrorMessage = "Nome do tamanho é obrigatório")]
    [MinLength(1, ErrorMessage = "Mínimo 1 caractere")]
    [MaxLength(10, ErrorMessage = "Máximo 10 caracteres")]
    public string Nome { get; set; } = string.Empty;

    [Range(0, 999, ErrorMessage = "Ordem inválida")]
    public int Ordem { get; set; } = 99;

    public bool Ativo { get; set; } = true;
}

public class TamanhoListDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int Ordem { get; set; }
    public bool Ativo { get; set; }
}