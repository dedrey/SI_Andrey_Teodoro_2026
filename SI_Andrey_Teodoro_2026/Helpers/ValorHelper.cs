namespace SI_Andrey_Teodoro_2026.Helpers;

// Arredondamento padrão dos valores de compra. Só arredondar no total da linha e nos totais;
// valores unitários ficam com 4 casas.
public static class ValorHelper
{
    public static decimal Unit(decimal v) => Math.Round(v, 4, MidpointRounding.AwayFromZero);
    public static decimal Moeda(decimal v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);
    public static decimal Perc(decimal v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);

    public static decimal BrutoLinha(int quantidade, decimal valorUnitario) => Moeda(quantidade * valorUnitario);
    public static decimal DescontoLinha(int quantidade, decimal descontoUnitario) => Moeda(quantidade * descontoUnitario);

    public static decimal DescontoPorPercentual(decimal valorUnitario, decimal percentual) => Unit(valorUnitario * percentual / 100);
    public static decimal PercentualPorDesconto(decimal valorUnitario, decimal descontoUnitario) =>
        valorUnitario > 0 ? Perc(descontoUnitario / valorUnitario * 100) : 0;
}
