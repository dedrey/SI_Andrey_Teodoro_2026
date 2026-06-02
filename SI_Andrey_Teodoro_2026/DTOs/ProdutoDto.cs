using System.ComponentModel.DataAnnotations;

namespace SI_Andrey_Teodoro_2026.DTOs;

public class ProdutoDto
{
    public int Id { get; set; }
    public int IdOriginal { get; set; }

    [Required(ErrorMessage = "Nome do produto é obrigatório")]
    [MinLength(2, ErrorMessage = "Deve ter pelo menos 2 caracteres")]
    [MaxLength(100, ErrorMessage = "Deve ter no máximo 100 caracteres")]
    public string Produto { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "Descrição deve ter no máximo 500 caracteres")]
    public string? Descricao { get; set; }

    [Required(ErrorMessage = "Categoria é obrigatória")]
    [Range(1, int.MaxValue, ErrorMessage = "Selecione uma categoria válida")]
    public int CategoriaId { get; set; }

    [Required(ErrorMessage = "Marca é obrigatória")]
    [Range(1, int.MaxValue, ErrorMessage = "Selecione uma marca válida")]
    public int MarcaId { get; set; }

    [Required(ErrorMessage = "Unidade de medida é obrigatória")]
    [Range(1, int.MaxValue, ErrorMessage = "Selecione uma unidade de medida válida")]
    public int UnidadeMedidaId { get; set; }

    public bool Ativo { get; set; } = true;
    public DateTime? AtualizadoEm { get; set; }
    public string? NomeAtualizadoPor { get; set; }

    public List<ProdutoVariacaoDto> Variacoes { get; set; } = new();
}