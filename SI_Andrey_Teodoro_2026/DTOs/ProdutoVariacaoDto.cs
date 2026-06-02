using System.ComponentModel.DataAnnotations;

namespace SI_Andrey_Teodoro_2026.DTOs;

public class ProdutoVariacaoDto
{
    public int Id { get; set; }
    public int IdOriginal { get; set; }
    public int ProdutoId { get; set; }

    [Required(ErrorMessage = "Cor é obrigatória")]
    [MinLength(2, ErrorMessage = "Deve ter pelo menos 2 caracteres")]
    [MaxLength(30, ErrorMessage = "Deve ter no máximo 30 caracteres")]
    public string Cor { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tamanho é obrigatório")]
    [MinLength(1, ErrorMessage = "Deve ter pelo menos 1 caractere")]
    [MaxLength(10, ErrorMessage = "Deve ter no máximo 10 caracteres")]
    public string Tamanho { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? CodigoBarras { get; set; }

    [Required(ErrorMessage = "Preço é obrigatório")]
    [Range(0.01, 999999.99, ErrorMessage = "Preço deve ser maior que zero")]
    public decimal Preco { get; set; }

    public bool Ativo { get; set; } = true;
    public int QuantidadeEstoque { get; set; }
    public DateTime? AtualizadoEm { get; set; }
    public string? NomeAtualizadoPor { get; set; }

    public bool IsNova => IdOriginal == 0 && Id == 0;
    public bool Removida { get; set; } = false;
}