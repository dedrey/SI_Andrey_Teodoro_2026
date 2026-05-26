namespace SI_Andrey_Teodoro_2026.DTOs;

public class UsuarioListDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public bool Ativo { get; set; }
    public DateTime CriadoEm { get; set; }
}