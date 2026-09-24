namespace SI_Andrey_Teodoro_2026.DTOs;

public class CompraListDto
{
    public int Id { get; set; }
    public string NomeFornecedor { get; set; } = string.Empty;
    public string? NumeroNf { get; set; }
    public int TotalItens { get; set; }
    public decimal ValorSubtotal { get; set; }
    public decimal ValorFrete { get; set; }
    public decimal ValorOutrosAcrescimos { get; set; }
    public decimal ValorDesconto { get; set; }
    public decimal ValorTotal { get; set; }
    public DateTime DataEmissao { get; set; }
    public DateTime DataChegada { get; set; }
    public string StatusCompra { get; set; } = string.Empty;
    public string? MotivoCancelamento { get; set; }
    public DateTime CriadoEm { get; set; }
}

public class CompraItemListDto
{
    public int Id { get; set; }
    public int CompraId { get; set; }
    public int ProdutoVariacaoId { get; set; }
    public string NomeProduto { get; set; } = string.Empty;
    public string Cor { get; set; } = string.Empty;
    public string Tamanho { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal ValorDesconto { get; set; }
    public decimal ValorTotal { get; set; }
    public decimal CustoUnitarioEfetivo { get; set; }
}