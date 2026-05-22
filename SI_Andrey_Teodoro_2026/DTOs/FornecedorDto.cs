using System.ComponentModel.DataAnnotations;

namespace SI_Andrey_Teodoro_2026.DTOs;

public class FornecedorDto
{
    public int Id { get; set; }
    public int IdOriginal { get; set; }
    [Required(ErrorMessage = "Razão Social é obrigatória")]
    [MinLength(2, ErrorMessage = "Razão Social deve ter pelo menos 2 caracteres")]
    [MaxLength(255, ErrorMessage = "Razão Social deve ter no máximo 255 caracteres")]
    public string RazaoSocial { get; set; } = string.Empty;
    [Required(ErrorMessage = "CNPJ é obrigatório")]
    [MaxLength(18, ErrorMessage = "CNPJ deve ter 14 dígitos")]
    public string Cnpj { get; set; } = string.Empty;
    [MaxLength(255, ErrorMessage = "Nome Fantasia deve ter no máximo 255 caracteres")]
    public string? NomeFantasia { get; set; }
    public int? PaisId { get; set; }
    public int? EstadoId { get; set; }
    public int? CidadeId { get; set; }
    [MaxLength(255, ErrorMessage = "Endereço deve ter no máximo 255 caracteres")]
    public string? Endereco { get; set; }
    [MaxLength(120, ErrorMessage = "Bairro deve ter no máximo 120 caracteres")]
    public string? Bairro { get; set; }
    [MaxLength(45, ErrorMessage = "Telefone deve ter no máximo 45 caracteres")]
    public string? Telefone { get; set; }
    [MaxLength(320, ErrorMessage = "E-mail deve ter no máximo 320 caracteres")]
    [EmailAddress(ErrorMessage = "E-mail inválido")]
    public string? Email { get; set; }

    public bool Ativo { get; set; } = true;
    public DateTime? AtualizadoEm { get; set; }
}