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
    [MaxLength(30)]
    public string? InscricaoEstadual { get; set; }
    public string? NomeCidade { get; set; }
    public int? CidadeId { get; set; }
    [Required(ErrorMessage = "Endereço é obrigatório")]
    [MaxLength(100)]
    public string Endereco { get; set; } = string.Empty;
    [Required(ErrorMessage = "Número é obrigatório")]
    [MaxLength(10)] public string Numero { get; set; } = string.Empty;
    [Required(ErrorMessage = "Complemento é obrigatório")]
    [MaxLength(50)]
    public string Complemento { get; set; } = string.Empty;
    [Required(ErrorMessage = "Bairro é obrigatório")]
    [MaxLength(60)]
    public string Bairro { get; set; } = string.Empty;
    [Required(ErrorMessage = "CEP é obrigatório")]
    [MaxLength(9)] public string Cep { get; set; } = string.Empty;
    [Required(ErrorMessage = "Telefone é obrigatório")]
    [MaxLength(15)]
    public string Telefone { get; set; } = string.Empty;
    [Required(ErrorMessage = "E-mail é obrigatório")]
    [MaxLength(100)]
    [EmailAddress(ErrorMessage = "E-mail inválido")]
    public string Email { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
    public DateTime? AtualizadoEm { get; set; }
    public string? NomeAtualizadoPor { get; set; }
}