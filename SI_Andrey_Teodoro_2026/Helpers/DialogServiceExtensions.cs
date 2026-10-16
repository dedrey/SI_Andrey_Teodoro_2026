using System.Text.Json;
using System.Text.Json.Serialization;
using MudBlazor;
using SI_Andrey_Teodoro_2026.Components.Shared;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Modals;

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

    public static async Task<TItem?> PesquisarAsync<TItem>(
        this IDialogService dialogService,
        string titulo,
        Func<FiltroConsultaDto, Task<PaginacaoDto<TItem>>> buscar,
        List<ColunaPesquisa<TItem>> colunas,
        string? statusFiltro = null,
        Func<Task<TItem?>>? novo = null,
        Func<TItem, int>? obterId = null,
        int? selecionado = null) where TItem : class
    {
        var result = await AbrirPesquisaAsync(dialogService, titulo, buscar, colunas, statusFiltro, novo, obterId,
            selecionado is > 0 ? new[] { selecionado.Value } : null, false);
        return result is { Canceled: false } ? result.Data as TItem : null;
    }

    public static async Task<List<TItem>?> PesquisarVariosAsync<TItem>(
        this IDialogService dialogService,
        string titulo,
        Func<FiltroConsultaDto, Task<PaginacaoDto<TItem>>> buscar,
        List<ColunaPesquisa<TItem>> colunas,
        Func<TItem, int> obterId,
        IEnumerable<int>? selecionados,
        IEnumerable<TItem> itensDoCampo,
        string? statusFiltro = null,
        Func<Task<TItem?>>? novo = null) where TItem : class
    {
        var result = await AbrirPesquisaAsync(dialogService, titulo, buscar, colunas, statusFiltro, novo, obterId, selecionados, true);
        if (result is not { Canceled: false } || result.Data is not SelecaoPesquisa<TItem> selecao) return null;
        var mantidos = itensDoCampo.Where(i => selecao.IdsSemItem.Contains(obterId(i)));
        return selecao.Itens.Concat(mantidos).ToList();
    }

    private static async Task<DialogResult?> AbrirPesquisaAsync<TItem>(
        IDialogService dialogService,
        string titulo,
        Func<FiltroConsultaDto, Task<PaginacaoDto<TItem>>> buscar,
        List<ColunaPesquisa<TItem>> colunas,
        string? statusFiltro,
        Func<Task<TItem?>>? novo,
        Func<TItem, int>? obterId,
        IEnumerable<int>? selecionados,
        bool multiSelecao) where TItem : class
    {
        var param = new DialogParameters<ModalPesquisa<TItem>>
        {
            { x => x.Titulo,               titulo },
            { x => x.Buscar,               buscar },
            { x => x.Colunas,              colunas },
            { x => x.StatusFiltro,         statusFiltro },
            { x => x.Novo,                 novo },
            { x => x.ObterId,              obterId },
            { x => x.SelecionadosIniciais, selecionados },
            { x => x.MultiSelecao,         multiSelecao }
        };

        var dialog = await dialogService.ShowAsync<ModalPesquisa<TItem>>(
            titulo, param,
            new DialogOptions { CloseOnEscapeKey = false, BackdropClick = false, MaxWidth = MaxWidth.Large, FullWidth = true, DefaultFocus = DefaultFocus.None });

        return await dialog.Result;
    }
}
