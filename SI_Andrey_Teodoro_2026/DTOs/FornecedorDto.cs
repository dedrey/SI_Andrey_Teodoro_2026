using System.ComponentModel.DataAnnotations;
namespace SI_Andrey_Teodoro_2026.DTOs;
public class FornecedorDto
{
    public int Id { get; set; }
    public int IdOriginal { get; set; }
    [Required(ErrorMessage = "Nome/Razão Social é obrigatório")]
    [MinLength(2)]
    [MaxLength(100)]
    public string RazaoSocial { get; set; } = string.Empty;
    public string TipoPessoa { get; set; } = "PJ";
    [Required(ErrorMessage = "CPF/CNPJ é obrigatório")]
    [MaxLength(18)]
    public string CpfCnpj { get; set; } = string.Empty;
    [MinLength(2)]
    [MaxLength(100)]
    public string? NomeFantasia { get; set; }
    public string? NomeCidade { get; set; }
    public int? CidadeId { get; set; }
    [Required(ErrorMessage = "Endereço é obrigatório")]
    [MinLength(5)]
    [MaxLength(100)]
    public string Endereco { get; set; } = string.Empty;
    [Required(ErrorMessage = "Número é obrigatório")]
    [MaxLength(10)] public string Numero { get; set; } = string.Empty;
    [Required(ErrorMessage = "Complemento é obrigatório")]
    [MaxLength(50)] public string Complemento { get; set; } = string.Empty;
    [Required(ErrorMessage = "Bairro é obrigatório")]
    [MinLength(2)]
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