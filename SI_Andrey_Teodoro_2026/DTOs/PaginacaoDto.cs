namespace SI_Andrey_Teodoro_2026.DTOs;

public class PaginacaoDto<T>
{
    public List<T> Itens { get; set; } = new();
    public int TotalItens { get; set; }
    public int Pagina { get; set; }
    public int TamanhoPagina { get; set; }
    public int TotalPaginas => (int)Math.Ceiling((double)TotalItens / TamanhoPagina);
}

public class FiltroConsultaDto
{
    public string? Busca { get; set; }
    public string StatusFiltro { get; set; } = "ativos";
    public string OrdenarPor { get; set; } = "nome";
    public int Pagina { get; set; } = 1;
    public int TamanhoPagina { get; set; } = 10;
}