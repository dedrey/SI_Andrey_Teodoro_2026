namespace SI_Andrey_Teodoro_2026.Models;

public class Cliente
{
    public int Id { get; set; }
    public string NomeRazaoSocial { get; set; } = string.Empty;
    public string? CpfCnpj { get; set; }
    public string TipoPessoa { get; set; } = "PF";
    public string Sexo { get; set; } = "NAO_INFORMAR";
    public bool Estrangeiro { get; set; } = false;
    public string? DocumentoEstrangeiro { get; set; }
    public string? PaisOrigem { get; set; }
    public string? ApelidoNomeFantasia { get; set; }
    public int? CidadeId { get; set; }
    public string? NomeCidade { get; set; }
    public string? Cep { get; set; }
    public string? Endereco { get; set; }
    public string? Numero { get; set; }
    public string? Complemento { get; set; }
    public string? Bairro { get; set; }
    public string? Telefone { get; set; }
    public string? Celular { get; set; }
    public string? Email { get; set; }
    public string? InscricaoEstadual { get; set; }
    public string? InscricaoMunicipal { get; set; }
    public decimal LimiteCredito { get; set; } = 0;
    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; } = DateTime.Now;
    public DateTime? AtualizadoEm { get; set; }
    public string? NomeAtualizadoPor { get; set; }
}