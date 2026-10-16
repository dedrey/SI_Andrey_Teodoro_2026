using MudBlazor;

namespace SI_Andrey_Teodoro_2026.Helpers;

public enum TipoStatus { Conta, Compra, Venda, Nfe }

public static class StatusHelper
{
    public static string Texto(TipoStatus tipo, string? status, DateTime? vencimento = null) => tipo switch
    {
        TipoStatus.Conta => status switch
        {
            "PAGA" => "Paga",
            "RECEBIDA" => "Recebida",
            "CANCELADA" => "Cancelada",
            _ => EmAtraso(vencimento) ? "Em Atraso" : "Aberta"
        },
        TipoStatus.Compra => status == "LANCADO" ? "Lançado" : "Cancelado",
        TipoStatus.Nfe => status == "EMITIDA" ? "Emitida" : "Cancelada",
        _ => status ?? ""
    };

    public static Color Cor(TipoStatus tipo, string? status, DateTime? vencimento = null) => tipo switch
    {
        TipoStatus.Conta => status switch
        {
            "PAGA" or "RECEBIDA" => Color.Success,
            "CANCELADA" => Color.Default,
            _ => EmAtraso(vencimento) ? Color.Error : Color.Warning
        },
        TipoStatus.Compra => status == "LANCADO" ? Color.Success : Color.Error,
        TipoStatus.Venda => status switch
        {
            "ABERTA" => Color.Info,
            "FINALIZADA" => Color.Success,
            _ => Color.Error
        },
        TipoStatus.Nfe => status == "EMITIDA" ? Color.Success : Color.Default,
        _ => Color.Default
    };

    private static bool EmAtraso(DateTime? vencimento) => vencimento.HasValue && vencimento.Value.Date < DateTime.Today;
}
