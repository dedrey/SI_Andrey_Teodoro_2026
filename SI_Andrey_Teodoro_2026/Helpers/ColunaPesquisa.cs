namespace SI_Andrey_Teodoro_2026.Helpers;

public class ColunaPesquisa<TItem>
{
    public string Titulo { get; set; } = string.Empty;
    public Func<TItem, object?> Valor { get; set; } = _ => null;
    public bool AlinharDireita { get; set; }
    public string? Formato { get; set; }

    public ColunaPesquisa() { }

    public ColunaPesquisa(string titulo, Func<TItem, object?> valor, bool alinharDireita = false, string? formato = null)
    {
        Titulo = titulo;
        Valor = valor;
        AlinharDireita = alinharDireita;
        Formato = formato;
    }

    public string Texto(TItem item)
    {
        var valor = Valor(item);
        return valor is IFormattable f && !string.IsNullOrEmpty(Formato) ? f.ToString(Formato, null) : valor?.ToString() ?? "";
    }
}
