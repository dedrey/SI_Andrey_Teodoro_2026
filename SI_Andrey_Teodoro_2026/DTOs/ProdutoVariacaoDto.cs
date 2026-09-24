using System.ComponentModel.DataAnnotations;
namespace SI_Andrey_Teodoro_2026.DTOs;

public class ProdutoVariacaoDto
{
    public int Id { get; set; }
    public int IdOriginal { get; set; }
    public int ProdutoId { get; set; }

    [Required(ErrorMessage = "Cor é obrigatória")]
    public int CorId { get; set; }
    public string Cor { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tamanho é obrigatório")]
    public int TamanhoId { get; set; }
    public string Tamanho { get; set; } = string.Empty;

    [Required(ErrorMessage = "Preço de venda é obrigatório")]
    [Range(0.01, 999999.99, ErrorMessage = "Preço deve ser maior que zero")]
    public decimal Preco { get; set; }

    /// Somente leitura: custo efetivo da última compra lançada (definido pelo módulo de Compras).
    /// Não é gravado pelo cadastro de Produto.
    public decimal PrecoCusto { get; set; }

    public DateTime? DataUltimaCompra { get; set; }

    public bool Ativo { get; set; } = true;
    public int QuantidadeEstoque { get; set; }
    public DateTime? AtualizadoEm { get; set; }
    public string? NomeAtualizadoPor { get; set; }

    public bool IsNova => IdOriginal == 0 && Id == 0;
    public bool Removida { get; set; } = false;

    /// Fast Fashion: variação cuja última compra foi há mais de 90 dias pode ser vendida abaixo do custo.
    public bool PermiteVendaAbaixoCusto =>
        DataUltimaCompra.HasValue &&
        (DateTime.Today - DataUltimaCompra.Value).TotalDays > 90;
}