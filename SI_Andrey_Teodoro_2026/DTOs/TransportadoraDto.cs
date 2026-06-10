using System.ComponentModel.DataAnnotations;

namespace SI_Andrey_Teodoro_2026.DTOs;

public class TransportadoraDto
{
    public int Id { get; set; }
    public int IdOriginal { get; set; }

    [Required(ErrorMessage = "Razão Social é obrigatória")]
    [MinLength(2)]
    [MaxLength(100)]
    public string RazaoSocial { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? NomeFantasia { get; set; }

    [Required(ErrorMessage = "CNPJ é obrigatório")]
    [MaxLength(18)]
    public string Cnpj { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? InscricaoEstadual { get; set; }

    public int? CidadeId { get; set; }

    [MaxLength(100)]
    public string? Endereco { get; set; }

    [MaxLength(50)]
    public string? Complemento { get; set; }

    [MaxLength(60)]
    public string? Bairro { get; set; }

    [Required(ErrorMessage = "Telefone é obrigatório")]
    [MaxLength(15)]
    public string Telefone { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-mail é obrigatório")]
    [MaxLength(50)]
    public string Email { get; set; } = string.Empty;

    public bool Ativo { get; set; } = true;
    public DateTime? AtualizadoEm { get; set; }
    public string? NomeAtualizadoPor { get; set; }
}