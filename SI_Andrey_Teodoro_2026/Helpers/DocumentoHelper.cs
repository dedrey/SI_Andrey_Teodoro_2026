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

    public static bool CepValido(string? cep) => SomenteDigitos(cep).Length == 8;

    public static string? ValidarCep(string? cep) => SomenteDigitos(cep).Length switch
    {
        0 => "Obrigatório",
        8 => null,
        _ => "CEP incompleto."
    };

    private static bool Digito(string d, int[] pesos, int posicao)
    {
        var soma = pesos.Select((p, i) => (d[i] - '0') * p).Sum();
        var resto = soma % 11;
        return d[posicao] - '0' == (resto < 2 ? 0 : 11 - resto);
    }
}
