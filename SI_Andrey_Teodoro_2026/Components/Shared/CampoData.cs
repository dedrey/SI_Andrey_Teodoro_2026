using System.Globalization;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace SI_Andrey_Teodoro_2026.Components.Shared;

public class CampoData : MudDatePicker
{
    private const string Formato = "dd/MM/yyyy";
    private static readonly CultureInfo CulturaBr = new("pt-BR");

    public CampoData()
    {
        Culture = CulturaBr;
        DateFormat = Formato;
        Converter = new ConversorData();
        Mask = new DateMask(Formato);
        Editable = true;
        Placeholder = "dd/mm/aaaa";
        RequiredError = "Obrigatório";
        Variant = Variant.Outlined;
        Margin = Margin.Dense;
    }

    private bool TextoIncompleto => !string.IsNullOrEmpty(Text) && Text.Length < Formato.Length;

    public override Task SetParametersAsync(ParameterView parameters)
    {
        var ignorarData = parameters.TryGetValue<DateTime?>(nameof(Date), out var data) && data == _value;
        var manterErroValidacao = parameters.TryGetValue<bool>(nameof(Error), out var erro) && !erro && ValidationErrors.Count > 0;
        if (ignorarData || manterErroValidacao)
        {
            var valores = parameters.ToDictionary().ToDictionary(p => p.Key, p => p.Value);
            if (ignorarData)
                valores.Remove(nameof(Date));
            if (manterErroValidacao)
            {
                valores.Remove(nameof(Error));
                valores.Remove(nameof(ErrorText));
            }
            parameters = ParameterView.FromDictionary(valores!);
        }
        return base.SetParametersAsync(parameters);
    }

    protected override async Task StringValueChangedAsync(string? value)
    {
        if (!TextoIncompleto)
        {
            await base.StringValueChangedAsync(value);
            return;
        }
        Touched = true;
        if (ValidationErrors.Count > 0)
            await ResetValidationAsync();
        if (_value.HasValue)
        {
            _value = null;
            await DateChanged.InvokeAsync(null);
        }
    }

    protected override Task ValidateValue()
    {
        if (TextoIncompleto)
            ConvertGet(Text);
        return base.ValidateValue();
    }

    private sealed class ConversorData : IReversibleConverter<DateTime?, string?>
    {
        public string? Convert(DateTime? input) => input?.ToString(Formato, CulturaBr);

        public DateTime? ConvertBack(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;
            if (DateTime.TryParseExact(input, Formato, CulturaBr, DateTimeStyles.None, out var data))
                return data;
            throw new FormatException("Data inválida.");
        }
    }
}
