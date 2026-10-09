using System.Text.Json;
using System.Text.Json.Serialization;
using MudBlazor;
using SI_Andrey_Teodoro_2026.Components.Shared;

namespace SI_Andrey_Teodoro_2026.Helpers;

public static class DialogServiceExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    public static string Snapshot(object dados) => JsonSerializer.Serialize(dados, _jsonOptions);

    public static async Task<bool> ConfirmarDescarteAsync(this IDialogService dialogService)
    {
        var param = new DialogParameters<ConfirmDialog>
        {
            { x => x.Titulo,        "Descartar alterações?" },
            { x => x.Mensagem,      "Existem alterações não salvas. Deseja realmente fechar?" },
            { x => x.TextoBotao,    "Fechar sem salvar" },
            { x => x.TextoCancelar, "Continuar editando" },
            { x => x.CorBotao,      Color.Error }
        };

        var dialog = await dialogService.ShowAsync<ConfirmDialog>(
            "Descartar alterações?", param,
            new DialogOptions { CloseOnEscapeKey = false, BackdropClick = false, MaxWidth = MaxWidth.ExtraSmall, FullWidth = true });

        return (await dialog.Result) is { Canceled: false };
    }

    public static async Task FecharComConfirmacaoAsync(
        this IDialogService dialogService,
        IMudDialogInstance dialog,
        bool somenteLeitura,
        string? snapshot,
        object dadosAtuais)
    {
        if (somenteLeitura || snapshot is null || snapshot == Snapshot(dadosAtuais) || await dialogService.ConfirmarDescarteAsync())
            dialog.Cancel();
    }
}
