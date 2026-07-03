namespace SI_Andrey_Teodoro_2026.DTOs;

public class NfeDto
{
    public int Id { get; set; }
    public int Numero { get; set; }
    public short Serie { get; set; } = 1;
    public DateTime DataEmissao { get; set; } = DateTime.Now;
    public DateTime? DataSaida { get; set; }

    public int EmitenteId { get; set; }
    public string? NomeEmitente { get; set; }
    public string? CnpjEmitente { get; set; }
    public string? EnderecoEmitente { get; set; }
    public string? CidadeEmitente { get; set; }
    public string? RegimeTributarioEmitente { get; set; }

    public int ClienteId { get; set; }
    public string? NomeCliente { get; set; }
    public string? CpfCnpjCliente { get; set; }
    public string? EnderecoCliente { get; set; }
    public string? CidadeCliente { get; set; }

    public int? VendaId { get; set; }
    public string? NomeCondicao { get; set; }

    public int? TransportadoraId { get; set; }
    public string? NomeTransportadora { get; set; }

    public string StatusNfe { get; set; } = "EMITIDA";

    public decimal ValorProdutos { get; set; }
    public decimal ValorDesconto { get; set; }
    public decimal ValorTotal { get; set; }

    public DateTime CriadoEm { get; set; }

    public List<NfeProdutoDto> Itens { get; set; } = new();
}

public class NfeProdutoDto
{
    public int Id { get; set; }
    public int NfeId { get; set; }
    public int NumeroItem { get; set; }
    public int ProdutoVariacaoId { get; set; }
    public string DescricaoItem { get; set; } = string.Empty;
    public int UnidadeMedidaId { get; set; }
    public string? SiglaUnidade { get; set; }
    public decimal Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal ValorDesconto { get; set; }
    public decimal ValorTotal { get; set; }
    public string? Cor { get; set; }
    public string? Tamanho { get; set; }
}