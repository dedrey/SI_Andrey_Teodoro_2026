namespace SI_Andrey_Teodoro_2026.Models;

public class Emitente
{
    public int Id { get; set; }
    public string NomeRazaoSocial { get; set; } = string.Empty;
    public string Cnpj { get; set; } = string.Empty;
    public string? ApelidoNomeFantasia { get; set; }
    public int? CidadeId { get; set; }
    public string? NomeCidade { get; set; }
    public string? Endereco { get; set; }
    public string? Numero { get; set; }
    public string? Complemento { get; set; }
    public string? Bairro { get; set; }
    public string? Telefone { get; set; }
    public string? Email { get; set; }
    public string? InscricaoEstadual { get; set; }
    public string? InscricaoMunicipal { get; set; }
    public string RegimeTributario { get; set; } = "SIMPLES";
    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; } = DateTime.Now;
    public DateTime? AtualizadoEm { get; set; }
    public string? NomeAtualizadoPor { get; set; }
}