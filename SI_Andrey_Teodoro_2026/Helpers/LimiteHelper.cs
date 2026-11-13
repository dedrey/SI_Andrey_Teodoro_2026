namespace SI_Andrey_Teodoro_2026.Helpers;

public static class LimiteHelper
{
    public const string Negativo = "Não pode ser negativo.";

    public static string? NaoNegativo(decimal v) => v < 0 ? Negativo : null;

    public static string? NaoNegativo(decimal? v) => v < 0 ? Negativo : null;

    public static string? Positivo(decimal v) => v < 0 ? Negativo : v == 0 ? "Obrigatório" : null;

    public static string? Percentual(decimal v) => v < 0 ? Negativo : v > 100 ? "Não pode ser maior que 100%." : null;

    public static string? Faixa(int? v, int minimo, int maximo) =>
        v < minimo || v > maximo ? $"Informe um valor de {minimo} a {maximo}." : null;
}
