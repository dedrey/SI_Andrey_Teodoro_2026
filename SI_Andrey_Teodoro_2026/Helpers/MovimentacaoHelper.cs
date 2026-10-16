namespace SI_Andrey_Teodoro_2026.Helpers;

public static class MovimentacaoHelper
{
    public static string Quantidade(int quantidade, string? sentido) => sentido switch
    {
        "E" => $"+{quantidade}",
        "S" => $"−{quantidade}",
        _ => quantidade.ToString()
    };

    public static string QuantidadeTotal(string tipoMovimentacao, int total) =>
        tipoMovimentacao != "AJUSTE" ? total.ToString()
        : total > 0 ? $"+{total}"
        : total < 0 ? $"−{-total}"
        : "0";
}
