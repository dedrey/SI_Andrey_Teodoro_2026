namespace SI_Andrey_Teodoro_2026.Helpers;

public static class TelefoneHelper
{
    public const string RegexEstrangeiroParcial = @"^[0-9 +\-()]{0,15}$";

    public static string Mascara(string? telefone)
    {
        var d = DocumentoHelper.SomenteDigitos(telefone);
        if (d.Length > 11) d = d[..11];
        return d.Length switch
        {
            0 => "",
            <= 2 => $"({d}",
            <= 6 => $"({d[..2]}) {d[2..]}",
            <= 10 => $"({d[..2]}) {d[2..6]}-{d[6..]}",
            _ => $"({d[..2]}) {d[2..7]}-{d[7..]}"
        };
    }

    public static string Formatar(string? telefone, bool estrangeiro) => estrangeiro ? telefone ?? "" : Mascara(telefone);

    public static string? Validar(string? telefone) => DocumentoHelper.SomenteDigitos(telefone).Length switch
    {
        10 or 11 => null,
        < 10 => "Incompleto",
        _ => "Máx 11 dígitos"
    };

    public static string? ValidarObrigatorio(string? telefone, bool estrangeiro)
    {
        if (estrangeiro) return ValidarEstrangeiro(telefone);
        return DocumentoHelper.SomenteDigitos(telefone).Length == 0 ? "Obrigatório" : Validar(telefone);
    }

    public static string? ValidarEstrangeiro(string? telefone)
    {
        var t = (telefone ?? "").Trim();
        if (t.Length == 0) return "Obrigatório";
        if (t.Length > 15) return "Máx 15 caracteres";
        if (!System.Text.RegularExpressions.Regex.IsMatch(t, @"^[0-9 +\-()]+$")) return "Use apenas dígitos, espaços, +, - e parênteses";
        return DocumentoHelper.SomenteDigitos(t).Length == 0 ? "Informe os dígitos" : null;
    }
}
