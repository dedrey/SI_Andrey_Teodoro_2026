using System.ComponentModel.DataAnnotations;

namespace SI_Andrey_Teodoro_2026.DTOs;

public class FornecedorDto
{
    public int Id { get; set; }
    public int IdOriginal { get; set; }

    [Required(ErrorMessage = "Razão Social é obrigatória")]
    [MinLength(2, ErrorMessage = "Razão Social deve ter pelo menos 2 caracteres")]
    [MaxLength(100)]
    public string RazaoSocial { get; set; } = string.Empty;

    [Required(ErrorMessage = "CNPJ é obrigatório")]
    [MaxLength(18)]
    public string Cnpj { get; set; } = string.Empty;
    [Required(ErrorMessage = "Nome Fantasia é obrigatório")]
    [MinLength(2, ErrorMessage = "Nome Fantasia deve ter pelo menos 2 caracteres")]
    [MaxLength(100)]
    public string NomeFantasia { get; set; } = string.Empty;

    [Required(ErrorMessage = "Endereço é obrigatório")]
    [MinLength(5, ErrorMessage = "Endereço deve ter pelo menos 5 caracteres")]
    [MaxLength(100)]
    public string Endereco { get; set; } = string.Empty;

    [Required(ErrorMessage = "Bairro é obrigatório")]
    [MinLength(2, ErrorMessage = "Bairro deve ter pelo menos 2 caracteres")]
    [MaxLength(60)]
    public string Bairro { get; set; } = string.Empty;

    [Required(ErrorMessage = "Telefone é obrigatório")]
    [MaxLength(15)]
    public string Telefone { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-mail é obrigatório")]
    [MaxLength(100)]
    [EmailAddress(ErrorMessage = "E-mail inválido")]
    public string Email { get; set; } = string.Empty;
    public int? PaisId { get; set; }
    public int? EstadoId { get; set; }
    public int? CidadeId { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime? AtualizadoEm { get; set; }
    public string? NomeAtualizadoPor { get; set; }
}