namespace SI_Andrey_Teodoro_2026.Helpers;

public static class ListaExtensions
{
    public static TItem ObterOuAdicionar<TItem>(this List<TItem> lista, TItem item, Func<TItem, int> id)
    {
        var chave = id(item);
        var existente = lista.FirstOrDefault(i => id(i) == chave);
        if (existente != null) return existente;
        lista.Add(item);
        return item;
    }
}
