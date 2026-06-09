namespace SI_Andrey_Teodoro_2026.DTOs;

public class VendaDto
{
    public int Id { get; set; }
    public int IdOriginal { get; set; }

    public int? ClienteId { get; set; }
    public string NomeCliente { get; set; } = string.Empty;

    public int? CondicaoPagamentoId { get; set; }
    public string NomeCondicao { get; set; } = string.Empty;

    public int? EmitenteId { get; set; }

    public decimal ValorSubtotal { get; set; }
    public decimal ValorDesconto { get; set; }
    public decimal ValorTotal { get; set; }

    public string StatusVenda { get; set; } = "ABERTA";
    public DateTime CriadoEm { get; set; }
    public DateTime? AtualizadoEm { get; set; }

    public List<VendaItemDto> Itens { get; set; } = new();
}

public class VendaItemDto
{
    public int Id { get; set; }
    public int VendaId { get; set; }
    public int ProdutoVariacaoId { get; set; }
    public int Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal ValorDesconto { get; set; }
    public decimal ValorTotal => (ValorUnitario * Quantidade) - ValorDesconto;
    public int ProdutoId { get; set; }
    public string NomeProduto { get; set; } = string.Empty;
    public string Cor { get; set; } = string.Empty;
    public string Tamanho { get; set; } = string.Empty;
    public int EstoqueAtual { get; set; }
    public bool Removido { get; set; } = false;
    public bool EstoqueInsuficiente => EstoqueAtual > 0 && Quantidade > EstoqueAtual;
}