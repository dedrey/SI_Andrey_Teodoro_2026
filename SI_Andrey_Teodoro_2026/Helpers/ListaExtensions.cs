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

    public static async Task IncluirSeAusenteAsync<TItem>(this List<TItem> lista, int? id, Func<TItem, int> chave, Func<int, Task<TItem?>> obter) where TItem : class
    {
        if (id is not > 0 || lista.Any(i => chave(i) == id)) return;
        if (await obter(id.Value) is TItem item) lista.Add(item);
    }
}
