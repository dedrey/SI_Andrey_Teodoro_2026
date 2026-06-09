namespace SI_Andrey_Teodoro_2026.DTOs;

public class VendaListDto
{
    public int Id { get; set; }
    public string NomeCliente { get; set; } = string.Empty;
    public string? NomeCondicao { get; set; }
    public int TotalItens { get; set; }
    public decimal ValorSubtotal { get; set; }
    public decimal ValorDesconto { get; set; }
    public decimal ValorTotal { get; set; }
    public decimal DescontoPercentual { get; set; }
    public decimal AcrescimoPercentual { get; set; }
    public decimal TaxaJurosPercentual { get; set; }
    public int NumeroParcelas { get; set; }
    public string StatusVenda { get; set; } = string.Empty;
    public string? MotivoCancelamento { get; set; }
    public DateTime CriadoEm { get; set; }
}

public class VendaItemListDto
{
    public int Id { get; set; }
    public int VendaId { get; set; }
    public int ProdutoVariacaoId { get; set; }
    public string NomeProduto { get; set; } = string.Empty;
    public string Cor { get; set; } = string.Empty;
    public string Tamanho { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal ValorDesconto { get; set; }
    public decimal ValorTotal { get; set; }
}