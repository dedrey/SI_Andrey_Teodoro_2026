namespace SI_Andrey_Teodoro_2026.Helpers;

public class GruposProgressivos
{
    private readonly Func<bool> _ativo;
    private readonly Func<int, string?> _erro;
    private readonly string[] _nomes;

    public GruposProgressivos(Func<bool> ativo, Func<int, string?> erro, params string[] nomes)
    {
        _ativo = ativo;
        _erro = erro;
        _nomes = nomes;
    }

    public int Atual { get; private set; } = 1;
    public int? Editando { get; private set; }

    public bool Travado(int g) => _ativo() && g < Atual;
    public bool Liberado(int g) => Pendente(g) == 0;
    public bool Bloqueado(int g) => _ativo() && (Editando.HasValue ? Editando != g : Travado(g) || !Liberado(g));

    public string? Ajuda(int g)
    {
        if (!_ativo() || Editando.HasValue || Travado(g)) return null;
        var p = Pendente(g);
        return p == 0 ? null : $"Complete {_nomes[p - 1]} primeiro.";
    }

    public void Alterou(int g) { if (_ativo() && g > Atual) Atual = g; }
    public void Editar(int g) => Editando = g;

    public string? Confirmar()
    {
        if (Editando is not int g) return null;
        var erro = _erro(g);
        if (erro == null) Editando = null;
        return erro;
    }

    private int Pendente(int g) => Enumerable.Range(1, g - 1).FirstOrDefault(i => _erro(i) != null);
}
