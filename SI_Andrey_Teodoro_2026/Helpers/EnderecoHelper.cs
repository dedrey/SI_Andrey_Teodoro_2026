namespace SI_Andrey_Teodoro_2026.Helpers;

public static class EnderecoHelper
{
    public static string Formatar(string? logradouro, string? numero, string? complemento, string? bairro,
                                  string? cidade, string? uf, string? cep)
    {
        var cidadeUf = Preenchido(cidade) && Preenchido(uf) ? $"{cidade!.Trim()} ({uf!.Trim()})" : cidade;
        var cepFormatado = Preenchido(cep) ? $"CEP {CepHelper.Formatar(cep)}" : null;
        return Juntar(" — ", Juntar(", ", logradouro, numero, complemento), bairro, cidadeUf, cepFormatado);
    }

    public static string FormatarEstrangeiro(string? logradouro, string? numero, string? complemento, string? bairro,
                                             string? cidade, string? pais)
        => Juntar(" — ", Juntar(", ", logradouro, numero, complemento), bairro, cidade, pais);

    private static bool Preenchido(string? s) => !string.IsNullOrWhiteSpace(s);

    private static string Juntar(string separador, params string?[] partes)
        => string.Join(separador, partes.Where(Preenchido).Select(p => p!.Trim()));
}
