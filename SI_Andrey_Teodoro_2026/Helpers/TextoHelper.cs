namespace SI_Andrey_Teodoro_2026.Helpers;

public static class TextoHelper
{
    public static bool ApenasLetras(string? texto) =>
        System.Text.RegularExpressions.Regex.IsMatch((texto ?? "").Trim(), @"^[a-zA-ZÀ-ÿ\s]+$");

    public static string? ValidarApenasLetras(string? texto, string mensagemObrigatorio = "Obrigatório") =>
        string.IsNullOrWhiteSpace(texto) ? mensagemObrigatorio : !ApenasLetras(texto) ? "Apenas letras" : null;
}
