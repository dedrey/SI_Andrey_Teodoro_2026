namespace SI_Andrey_Teodoro_2026.Helpers;

public static class DataHelper
{
    public static string? ValidarNaoFutura(DateTime? data, string descricao) =>
        data.HasValue && data.Value.Date > DateTime.Today
            ? $"A {descricao} não pode ser maior que a data atual."
            : null;

    public static string? ValidarNaoAnterior(DateTime? data, DateTime? dataMinima, string descricao, string descricaoMinima) =>
        data.HasValue && dataMinima.HasValue && data.Value.Date < dataMinima.Value.Date
            ? $"A {descricao} não pode ser anterior à {descricaoMinima}."
            : null;
}
