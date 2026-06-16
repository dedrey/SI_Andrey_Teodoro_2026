using System.ComponentModel.DataAnnotations;
namespace SI_Andrey_Teodoro_2026.DTOs;
public class ClienteDto
{
    public int Id { get; set; }
    public int IdOriginal { get; set; }
    [Required(ErrorMessage = "Nome / Razão Social é obrigatório")]
    [MinLength(2)]
    [MaxLength(100)]
    public string NomeRazaoSocial { get; set; } = string.Empty;
    [Required] public string TipoPessoa { get; set; } = "PF";
    [Required] public string Sexo { get; set; } = "NAO_INFORMAR";
    public bool Estrangeiro { get; set; } = false;
    [MaxLength(14)] public string? CpfCnpj { get; set; }
    [MaxLength(30)] public string? DocumentoEstrangeiro { get; set; }
    [MaxLength(100)] public string? ApelidoNomeFantasia { get; set; }
    public string? NomeCidade { get; set; }

    public int? CidadeId { get; set; }
    [Required(ErrorMessage = "Endereço é obrigatório")]
    [MinLength(5)]
    [MaxLength(100)]
    public string Endereco { get; set; } = string.Empty;
    [MaxLength(50)] public string? Complemento { get; set; }
    [Required(ErrorMessage = "Bairro é obrigatório")]
    [MinLength(2)]
    [MaxLength(60)]
    public string Bairro { get; set; } = string.Empty;
    [MaxLength(9)] public string? Cep { get; set; }
    [Required(ErrorMessage = "Telefone é obrigatório")]
    [MaxLength(15)]
    public string Telefone { get; set; } = string.Empty;
    [Required(ErrorMessage = "E-mail é obrigatório")]
    [MaxLength(100)]
    [EmailAddress(ErrorMessage = "E-mail inválido")]
    public string Email { get; set; } = string.Empty;
    [MaxLength(20)] public string? InscricaoEstadual { get; set; }
    [MaxLength(20)] public string? InscricaoMunicipal { get; set; }
    public decimal LimiteCredito { get; set; } = 0;
    public bool Ativo { get; set; } = true;
    public DateTime? AtualizadoEm { get; set; }
    public string? NomeAtualizadoPor { get; set; }
}