namespace SI_Andrey_Teodoro_2026.Models;

public class Venda
{
    public int Id { get; set; }
    public int? EmitenteId { get; set; }
    public int? ClienteId { get; set; }
    public string NomeCliente { get; set; } = string.Empty;
    public int? CondicaoPagamentoId { get; set; }
    public string? NomeCondicao { get; set; }
    public int? MovimentacaoId { get; set; }
    public decimal ValorSubtotal { get; set; }
    public decimal ValorDesconto { get; set; }
    public decimal ValorTotal { get; set; }
    public string StatusVenda { get; set; } = "ABERTA";
    public DateTime CriadoEm { get; set; }
    public DateTime AtualizadoEm { get; set; }
}

public class VendaItem
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