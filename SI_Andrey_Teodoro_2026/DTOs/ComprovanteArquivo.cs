namespace SI_Andrey_Teodoro_2026.DTOs;

public class ComprovanteArquivo
{
    public byte[] Conteudo { get; init; } = Array.Empty<byte>();
    public string NomeOriginal { get; init; } = string.Empty;
    public string Extensao { get; init; } = string.Empty;
    public long Tamanho { get; init; }
}
