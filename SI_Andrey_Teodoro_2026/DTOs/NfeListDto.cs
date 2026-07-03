namespace SI_Andrey_Teodoro_2026.DTOs;

public class NfeListDto
{
    public int Id { get; set; }
    public int Numero { get; set; }
    public short Serie { get; set; }
    public DateTime DataEmissao { get; set; }
    public string NomeEmitente { get; set; } = string.Empty;
    public string NomeCliente { get; set; } = string.Empty;
    public int? VendaId { get; set; }
    public string? NomeTransportadora { get; set; }
    public string StatusNfe { get; set; } = string.Empty;
    public decimal ValorTotal { get; set; }
    public DateTime CriadoEm { get; set; }
}
public class VendaParaNfeDto
{
    public int Id { get; set; }
    public int? ClienteId { get; set; }
    public string NomeCliente { get; set; } = string.Empty;
    public string? NomeCondicao { get; set; }
    public decimal ValorSubtotal { get; set; }
    public decimal ValorDesconto { get; set; }
    public decimal ValorTotal { get; set; }
    public DateTime CriadoEm { get; set; }
}
public class VendaItemParaNfeDto
{
    public int ProdutoVariacaoId { get; set; }
    public string NomeProduto { get; set; } = string.Empty;
    public string Cor { get; set; } = string.Empty;
    public string Tamanho { get; set; } = string.Empty;
    public int UnidadeMedidaId { get; set; }
    public string? SiglaUnidade { get; set; }
    public decimal Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal ValorDesconto { get; set; }
    public decimal ValorTotal { get; set; }
}