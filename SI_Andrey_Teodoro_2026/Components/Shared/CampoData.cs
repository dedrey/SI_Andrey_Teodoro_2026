using System.Globalization;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace SI_Andrey_Teodoro_2026.Components.Shared;

public class CampoData : MudDatePicker
{
    private const string Formato = "dd/MM/yyyy";
    private const string MensagemInvalida = "Data inválida.";
    public static readonly DateTime DataMinima = new(2000, 1, 1);
    private static readonly CultureInfo CulturaBr = new("pt-BR");
    private static readonly string[] FormatosAceitos = [Formato, "d/M/yyyy", "ddMMyyyy"];

    private readonly Func<DateTime?, IEnumerable<string>> _validacao;
    private object? _validacaoExterna;

    public CampoData()
    {
        Culture = CulturaBr;
        DateFormat = Formato;
        Converter = new ConversorData();
        Editable = true;
        Placeholder = "dd/mm/aaaa";
        RequiredError = "Obrigatório";
        Variant = Variant.Outlined;
        Margin = Margin.Dense;
        MinDate = DataMinima;
        ShowToolbar = false;
        _validacao = Validar;
        Validation = _validacao;
    }

    private bool TextoInvalido => !string.IsNullOrWhiteSpace(Text) && !TentarLer(Text, out _);

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

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        if (!ReferenceEquals(Validation, _validacao))
        {
            _validacaoExterna = Validation;
            Validation = _validacao;
        }
        UserAttributes.TryAdd(nameof(MudTextField<string>.InputMode), InputMode.numeric);
        UserAttributes.TryAdd(nameof(MudTextField<string>.MaxLength), Formato.Length);
    }

    protected override async Task StringValueChangedAsync(string? value)
    {
        await base.StringValueChangedAsync(value);
        if (!_value.HasValue)
            return;
        var formatado = _value.Value.ToString(Formato, CulturaBr);
        if (Text != formatado)
            await SetTextAsync(formatado, false);
    }

    private IEnumerable<string> Validar(DateTime? data)
    {
        if (TextoInvalido)
            return [MensagemInvalida];
        return _validacaoExterna switch
        {
            Func<DateTime?, string?> validar => validar(data) is { Length: > 0 } mensagem ? [mensagem] : [],
            Func<DateTime?, IEnumerable<string>> validar => validar(data),
            Func<DateTime?, bool> validar => validar(data) ? [] : [MensagemInvalida],
            _ => []
        };
    }

    private static bool TentarLer(string? texto, out DateTime data)
    {
        data = default;
        if (string.IsNullOrWhiteSpace(texto))
            return false;
        return DateTime.TryParseExact(texto.Trim(), FormatosAceitos, CulturaBr, DateTimeStyles.None, out data) && data >= DataMinima;
    }

    private sealed class ConversorData : IReversibleConverter<DateTime?, string?>
    {
        public string? Convert(DateTime? input) => input?.ToString(Formato, CulturaBr);

        public DateTime? ConvertBack(string? input) => TentarLer(input, out var data) ? data : null;
    }
}
