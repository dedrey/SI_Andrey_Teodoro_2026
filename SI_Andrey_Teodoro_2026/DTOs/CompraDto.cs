namespace SI_Andrey_Teodoro_2026.DTOs;

public class CompraDto
{
    public int Id { get; set; }
    public int IdOriginal { get; set; }

    public int? FornecedorId { get; set; }
    public string NomeFornecedor { get; set; } = string.Empty;

    public string? NumeroNf { get; set; }
    public DateTime? DataEmissao { get; set; }
    public DateTime? DataChegada { get; set; }

    public int? CondicaoPagamentoId { get; set; }
    public string NomeCondicaoPagamento { get; set; } = string.Empty;

    public decimal ValorSubtotal { get; set; }
    public decimal ValorFrete { get; set; }
    public decimal ValorOutrosAcrescimos { get; set; }
    public decimal ValorDesconto { get; set; }
    public decimal ValorTotal { get; set; }

    public string StatusCompra { get; set; } = "LANCADO";
    public string? MotivoCancelamento { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime? AtualizadoEm { get; set; }

    public List<CompraItemDto> Itens { get; set; } = new();
}

public class CompraItemDto
{
    public int Id { get; set; }
    public int CompraId { get; set; }
    public int ProdutoVariacaoId { get; set; }
    public int Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal ValorDesconto { get; set; }
    public decimal ValorTotal => (ValorUnitario * Quantidade) - ValorDesconto;

    public decimal CustoUnitarioEfetivo { get; set; }

    public int ProdutoId { get; set; }
    public string NomeProduto { get; set; } = string.Empty;
    public string Cor { get; set; } = string.Empty;
    public string Tamanho { get; set; } = string.Empty;
    public bool Removido { get; set; } = false;
}