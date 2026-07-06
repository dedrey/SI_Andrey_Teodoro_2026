using System.ComponentModel.DataAnnotations;

namespace SI_Andrey_Teodoro_2026.DTOs;

public class EmitenteDto
{
    public int Id { get; set; }
    public int IdOriginal { get; set; }
    [Required(ErrorMessage = "Razão Social é obrigatória")]
    [MinLength(2)]
    [MaxLength(100)]
    public string NomeRazaoSocial { get; set; } = string.Empty;

    [Required(ErrorMessage = "CNPJ é obrigatório")]
    [MaxLength(18)]
    public string Cnpj { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nome Fantasia é obrigatório")]
    [MinLength(2)]
    [MaxLength(100)]
    public string ApelidoNomeFantasia { get; set; } = string.Empty;
    public int? CidadeId { get; set; }
    public string? NomeCidade { get; set; }

    [Required(ErrorMessage = "Endereço é obrigatório")]
    [MinLength(5)]
    [MaxLength(50)]
    public string Endereco { get; set; } = string.Empty;
    [Required(ErrorMessage = "Número é obrigatório")]
    [MaxLength(10)] public string Numero { get; set; } = string.Empty;
    [MaxLength(50)] public string? Complemento { get; set; }


    [Required(ErrorMessage = "Bairro é obrigatório")]
    [MinLength(2)]
    [MaxLength(50)]
    public string Bairro { get; set; } = string.Empty;

    [Required(ErrorMessage = "Telefone é obrigatório")]
    [MaxLength(15)]
    public string Telefone { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-mail é obrigatório")]
    [MaxLength(50)]
    [EmailAddress(ErrorMessage = "E-mail inválido")]
    public string Email { get; set; } = string.Empty;
    [MaxLength(20)] public string? InscricaoEstadual { get; set; }
    // [MaxLength(20)] public string? InscricaoMunicipal { get; set; }
    [Required]
    public string RegimeTributario { get; set; } = "SIMPLES";

    public bool Ativo { get; set; } = true;
    public DateTime? AtualizadoEm { get; set; }
    public string? NomeAtualizadoPor { get; set; }
}