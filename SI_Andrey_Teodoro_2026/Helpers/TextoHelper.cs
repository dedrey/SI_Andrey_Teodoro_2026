namespace SI_Andrey_Teodoro_2026.Helpers;

public static class TextoHelper
{
    public static bool ApenasLetras(string? texto) =>
        System.Text.RegularExpressions.Regex.IsMatch((texto ?? "").Trim(), @"^[a-zA-ZÀ-ÿ\s]+$");

    public static string? ValidarApenasLetras(string? texto, string mensagemObrigatorio = "Obrigatório") =>
        string.IsNullOrWhiteSpace(texto) ? mensagemObrigatorio : !ApenasLetras(texto) ? "Apenas letras" : null;

    public static string? ValidarTamanhos(params (string? valor, int maximo, string campo)[] campos)
    {
        foreach (var (valor, maximo, campo) in campos)
            if ((valor?.Length ?? 0) > maximo)
                return $"O campo {campo} excede o tamanho permitido ({maximo} caracteres).";
        return null;
    }
}
