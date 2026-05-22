namespace SI_Andrey_Teodoro_2026.DTOs;

public class FornecedorListDto
{
    public int Id { get; set; }
    public string RazaoSocial { get; set; } = string.Empty;
    public string Cnpj { get; set; } = string.Empty;
    public string? NomeFantasia { get; set; }
    public int? CidadeId { get; set; }
    public string? NomeCidade { get; set; }
    public string? Endereco { get; set; }
    public string? Telefone { get; set; }
    public string? Email { get; set; }
    public bool Ativo { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime? AtualizadoEm { get; set; }
}