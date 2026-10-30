namespace SI_Andrey_Teodoro_2026.Helpers;

public static class DocumentoHelper
{
    public static string SomenteDigitos(string? texto) => new((texto ?? "").Where(char.IsDigit).ToArray());

    public static bool CpfValido(string? cpf)
    {
        var d = SomenteDigitos(cpf);
        if (d.Length != 11 || d.Distinct().Count() == 1) return false;
        return Digito(d, new[] { 10, 9, 8, 7, 6, 5, 4, 3, 2 }, 9) && Digito(d, new[] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 }, 10);
    }

    public static bool CnpjValido(string? cnpj)
    {
        var d = SomenteDigitos(cnpj);
        if (d.Length != 14 || d.Distinct().Count() == 1) return false;
        return Digito(d, new[] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 }, 12) && Digito(d, new[] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 }, 13);
    }

    public static bool DocumentoValido(string? documento, string tipoPessoa) =>
        tipoPessoa == "PF" ? CpfValido(documento) : CnpjValido(documento);

    public static string? ValidarCpf(string? cpf)
    {
        var d = SomenteDigitos(cpf);
        if (d.Length == 0) return "Obrigatório";
        if (d.Length != 11) return "CPF deve ter 11 dígitos";
        return CpfValido(d) ? null : "CPF inválido";
    }

    public static string? ValidarCnpj(string? cnpj)
    {
        var d = SomenteDigitos(cnpj);
        if (d.Length == 0) return "Obrigatório";
        if (d.Length != 14) return "CNPJ deve ter 14 dígitos";
        return CnpjValido(d) ? null : "CNPJ inválido";
    }

    public static string? ValidarDocumento(string? documento, string tipoPessoa) =>
        tipoPessoa == "PF" ? ValidarCpf(documento) : ValidarCnpj(documento);

    public static string FormatarCpf(string? cpf)
    {
        var d = SomenteDigitos(cpf);
        if (d.Length > 11) d = d[..11];
        return d.Length == 11 ? $"{d[..3]}.{d[3..6]}.{d[6..9]}-{d[9..]}" : d;
    }

    public static string FormatarCnpj(string? cnpj, bool parcial = false)
    {
        var d = SomenteDigitos(cnpj);
        if (d.Length > 14) d = d[..14];
        if (!parcial) return d.Length == 14 ? $"{d[..2]}.{d[2..5]}.{d[5..8]}/{d[8..12]}-{d[12..]}" : d;
        return d.Length switch
        {
            <= 2 => d,
            <= 5 => $"{d[..2]}.{d[2..]}",
            <= 8 => $"{d[..2]}.{d[2..5]}.{d[5..]}",
            <= 12 => $"{d[..2]}.{d[2..5]}.{d[5..8]}/{d[8..]}",
            _ => $"{d[..2]}.{d[2..5]}.{d[5..8]}/{d[8..12]}-{d[12..]}"
        };
    }

    public static string FormatarDocumento(string? documento)
    {
        if (string.IsNullOrWhiteSpace(documento)) return string.Empty;
        return SomenteDigitos(documento).Length switch
        {
            11 => FormatarCpf(documento),
            14 => FormatarCnpj(documento),
            _ => documento
        };
    }

    public static string? ValidarCodigoBarras(string? codigo)
    {
        var c = (codigo ?? "").Trim();
        if (c.Length == 0) return null;
        return System.Text.RegularExpressions.Regex.IsMatch(c, @"^[0-9]{8,14}$") ? null : "Somente dígitos, de 8 a 14";
    }

    private static bool Digito(string d, int[] pesos, int posicao)
    {
        var soma = pesos.Select((p, i) => (d[i] - '0') * p).Sum();
        var resto = soma % 11;
        return d[posicao] - '0' == (resto < 2 ? 0 : 11 - resto);
    }
}
