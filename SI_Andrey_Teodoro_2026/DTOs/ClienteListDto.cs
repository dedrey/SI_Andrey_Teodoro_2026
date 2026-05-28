namespace SI_Andrey_Teodoro_2026.DTOs;

public class ClienteListDto
{
    public int Id { get; set; }
    public string NomeRazaoSocial { get; set; } = string.Empty;
    public string? CpfCnpj { get; set; }
    public string TipoPessoa { get; set; } = "PF";
    public bool Estrangeiro { get; set; }
    public string? DocumentoEstrangeiro { get; set; }
    public string? Nacionalidade { get; set; }
    public string? ApelidoNomeFantasia { get; set; }
    public string? Telefone { get; set; }
    public string? Email { get; set; }
    public decimal LimiteCredito { get; set; }
    public bool Ativo { get; set; }
    public DateTime CriadoEm { get; set; }
}