namespace SI_Andrey_Teodoro_2026.Helpers;

public static class ChaveNfeHelper
{
    public static bool Validar(string? chave)
    {
        if (chave == null || chave.Length != 44 || !chave.All(char.IsDigit)) return false;
        return CalcularDigito(chave[..43]) == chave[43] - '0';
    }

    public static int CalcularDigito(string chave43)
    {
        int soma = 0, peso = 2;
        for (int i = chave43.Length - 1; i >= 0; i--)
        {
            soma += (chave43[i] - '0') * peso;
            peso = peso == 9 ? 2 : peso + 1;
        }
        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }

    public static (string cnpj, string modelo, int serie, string numero, int ano, int mes) Decompor(string chave)
    {
        var numero = chave[25..34].TrimStart('0');
        return (chave[6..20],
                chave[20..22],
                int.Parse(chave[22..25]),
                numero.Length == 0 ? "0" : numero,
                2000 + int.Parse(chave[2..4]),
                int.Parse(chave[4..6]));
    }

    public static string Formatar(string? chave)
    {
        var digitos = DocumentoHelper.SomenteDigitos(chave);
        return string.Join(" ", Enumerable.Range(0, (digitos.Length + 3) / 4)
            .Select(i => digitos.Substring(i * 4, Math.Min(4, digitos.Length - i * 4))));
    }
}
