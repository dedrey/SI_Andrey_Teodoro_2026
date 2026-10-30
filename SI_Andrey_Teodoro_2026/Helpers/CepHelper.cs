namespace SI_Andrey_Teodoro_2026.Helpers;

public static class CepHelper
{
    public static string Mascara(string? cep)
    {
        if (string.IsNullOrEmpty(cep)) return "";
        var d = DocumentoHelper.SomenteDigitos(cep);
        if (d.Length > 8) d = d[..8];
        return d.Length <= 5 ? d : $"{d[..5]}-{d[5..]}";
    }

    public static string Formatar(string? cep)
    {
        var d = DocumentoHelper.SomenteDigitos(cep);
        return d.Length == 8 ? $"{d[..5]}-{d[5..]}" : (cep ?? "").Trim();
    }

    public static bool Valido(string? cep) => DocumentoHelper.SomenteDigitos(cep).Length == 8;

    public static string? Validar(string? cep) => DocumentoHelper.SomenteDigitos(cep).Length switch
    {
        0 => "Obrigatório",
        8 => null,
        _ => "CEP incompleto."
    };
}
