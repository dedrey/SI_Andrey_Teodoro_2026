namespace SI_Andrey_Teodoro_2026.Models;
public class Fornecedor
{
    public int Id { get; set; }
    public string RazaoSocial { get; set; } = string.Empty;
    public string Cnpj { get; set; } = string.Empty;
    public string? NomeFantasia { get; set; }
    public int? CidadeId { get; set; }
    public int? EstadoId { get; set; }
    public int? PaisId { get; set; }
    public string? NomeCidade { get; set; }
    public string? Cep { get; set; }
    public string? Endereco { get; set; }
    public string? Complemento { get; set; }
    public string? Bairro { get; set; }
    public string? Telefone { get; set; }
    public string? Email { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; } = DateTime.Now;
    public DateTime? AtualizadoEm { get; set; }
    public string? NomeAtualizadoPor { get; set; }
}