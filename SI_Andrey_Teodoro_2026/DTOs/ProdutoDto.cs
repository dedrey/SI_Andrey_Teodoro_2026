using System.ComponentModel.DataAnnotations;
namespace SI_Andrey_Teodoro_2026.DTOs;

public class ProdutoDto
{
    public int Id { get; set; }
    public int IdOriginal { get; set; }

    [Required(ErrorMessage = "Nome do produto é obrigatório")]
    [MinLength(2)]
    [MaxLength(100)]
    public string Produto { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Descricao { get; set; }

    public int CategoriaId { get; set; }
    public string? NomeCategoria { get; set; }

    public int MarcaId { get; set; }
    public string? NomeMarca { get; set; }

    public int UnidadeMedidaId { get; set; }
    public string? SiglaUnidade { get; set; }

    public int? FornecedorId { get; set; }
    public string? NomeFornecedor { get; set; }

    public bool Ativo { get; set; } = true;
    public DateTime? AtualizadoEm { get; set; }
    public string? NomeAtualizadoPor { get; set; }

    public List<ProdutoVariacaoDto> Variacoes { get; set; } = new();
    public string? NumeroNfUltimaEntrada { get; set; }

}