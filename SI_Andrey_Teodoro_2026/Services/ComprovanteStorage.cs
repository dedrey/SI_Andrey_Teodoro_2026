using Microsoft.AspNetCore.Components.Forms;
using SI_Andrey_Teodoro_2026.DTOs;

namespace SI_Andrey_Teodoro_2026.Services;

public class ComprovanteStorage
{
    private const long TamanhoMaximoBytes = 5 * 1024 * 1024;
    private static readonly string[] ExtensoesPermitidas = { ".pdf", ".jpg", ".jpeg", ".png" };
    private readonly IWebHostEnvironment _env;

    public ComprovanteStorage(IWebHostEnvironment env) => _env = env;

    public string? Validar(IBrowserFile arquivo)
    {
        var ext = Path.GetExtension(arquivo.Name).ToLowerInvariant();
        if (!ExtensoesPermitidas.Contains(ext))
            return "Apenas arquivos PDF, JPG ou PNG são aceitos.";
        if (arquivo.Size > TamanhoMaximoBytes)
            return "Arquivo muito grande (máximo 5 MB).";
        return null;
    }

    public async Task<(ComprovanteArquivo? arquivo, string? erro)> LerAsync(IBrowserFile arquivo)
    {
        try
        {
            using var stream = arquivo.OpenReadStream(TamanhoMaximoBytes);
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            return (new ComprovanteArquivo
            {
                Conteudo = ms.ToArray(),
                NomeOriginal = arquivo.Name,
                Extensao = Path.GetExtension(arquivo.Name).ToLowerInvariant(),
                Tamanho = arquivo.Size
            }, null);
        }
        catch (Exception ex)
        {
            return (null, $"Erro ao ler o arquivo: {ex.Message}");
        }
    }

    public async Task<string> SalvarAsync(ComprovanteArquivo arquivo, string prefixo, int id)
    {
        var pastaDestino = Path.Combine(_env.WebRootPath, "uploads", "comprovantes");
        Directory.CreateDirectory(pastaDestino);
        var nomeArquivo = $"{prefixo}_{id}_{DateTime.Now:yyyyMMddHHmmss}{arquivo.Extensao}";
        await File.WriteAllBytesAsync(Path.Combine(pastaDestino, nomeArquivo), arquivo.Conteudo);
        return $"/uploads/comprovantes/{nomeArquivo}";
    }
}
