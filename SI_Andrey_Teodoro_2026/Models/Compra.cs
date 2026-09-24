namespace SI_Andrey_Teodoro_2026.Models;

public class Compra
{
    public int Id { get; set; }
    public int? FornecedorId { get; set; }
    public string NomeFornecedor { get; set; } = string.Empty;
    public string? NumeroNf { get; set; }
    public DateTime DataEmissao { get; set; }
    public DateTime DataChegada { get; set; }
    public int? CondicaoPagamentoId { get; set; }
    public string? NomeCondicaoPagamento { get; set; }
    public decimal ValorSubtotal { get; set; }
    public decimal ValorFrete { get; set; }
    public decimal ValorOutrosAcrescimos { get; set; }
    public decimal ValorDesconto { get; set; }
    public decimal ValorTotal { get; set; }
    public string StatusCompra { get; set; } = "LANCADO";
    public string? MotivoCancelamento { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime AtualizadoEm { get; set; }
}

public class CompraItem
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
    public bool AtualizarPrecoCusto { get; set; }
}
