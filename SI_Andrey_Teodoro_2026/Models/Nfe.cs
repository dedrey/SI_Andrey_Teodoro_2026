namespace SI_Andrey_Teodoro_2026.Models;

public class Nfe
{
    public int Id { get; set; }
    public int Numero { get; set; }
    public short Serie { get; set; }
    public DateTime DataEmissao { get; set; }
    public DateTime? DataSaida { get; set; }

    public int EmitenteId { get; set; }
    public string? NomeEmitente { get; set; }
    public string? CnpjEmitente { get; set; }
    public string? EnderecoEmitente { get; set; }
    public string? NumeroEmitente { get; set; }
    public string? ComplementoEmitente { get; set; }
    public string? BairroEmitente { get; set; }
    public string? CepEmitente { get; set; }
    public string? CidadeEmitente { get; set; }
    public string? UfEmitente { get; set; }
    public string? RegimeTributarioEmitente { get; set; }

    public int ClienteId { get; set; }
    public string? NomeCliente { get; set; }
    public string? CpfCnpjCliente { get; set; }
    public bool EstrangeiroCliente { get; set; }
    public string? EnderecoCliente { get; set; }
    public string? NumeroCliente { get; set; }
    public string? ComplementoCliente { get; set; }
    public string? BairroCliente { get; set; }
    public string? CepCliente { get; set; }
    public string? CidadeCliente { get; set; }
    public string? UfCliente { get; set; }
    public string? PaisCliente { get; set; }

    public int? VendaId { get; set; }
    public string? NomeCondicao { get; set; }

    public int? TransportadoraId { get; set; }
    public string? NomeTransportadora { get; set; }
    public string? EnderecoTransportadora { get; set; }
    public string? NumeroTransportadora { get; set; }
    public string? ComplementoTransportadora { get; set; }
    public string? BairroTransportadora { get; set; }
    public string? CepTransportadora { get; set; }
    public string? CidadeTransportadora { get; set; }
    public string? UfTransportadora { get; set; }

    public string StatusNfe { get; set; } = "EMITIDA";
    public decimal ValorProdutos { get; set; }
    public decimal ValorDesconto { get; set; }
    public decimal ValorTotal { get; set; }

    public DateTime CriadoEm { get; set; }
    public int? CriadoPor { get; set; }
    public DateTime AtualizadoEm { get; set; }
}

public class NfeProduto
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
    public DateTime CriadoEm { get; set; }
}