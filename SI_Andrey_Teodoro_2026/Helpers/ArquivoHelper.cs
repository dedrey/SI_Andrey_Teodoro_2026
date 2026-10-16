namespace SI_Andrey_Teodoro_2026.Helpers;

public static class ArquivoHelper
{
    public static string FormatarTamanho(long bytes)
        => bytes < 1024 * 1024 ? $"{bytes / 1024.0:N0} KB" : $"{bytes / (1024.0 * 1024):N1} MB";
}
