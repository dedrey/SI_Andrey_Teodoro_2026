namespace SI_Andrey_Teodoro_2026.DTOs;

public class EmitenteListDto
{
    public int Id { get; set; }
    public string NomeRazaoSocial { get; set; } = string.Empty;
    public string Cnpj { get; set; } = string.Empty;
    public string? ApelidoNomeFantasia { get; set; }
    public string? NomeCidade { get; set; }
    public string? Telefone { get; set; }
    public string? Email { get; set; }
    public string RegimeTributario { get; set; } = "SIMPLES";
    public bool Ativo { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime? AtualizadoEm { get; set; }
}