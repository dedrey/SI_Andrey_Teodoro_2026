using Microsoft.AspNetCore.Components;
using MudBlazor;
using SI_Andrey_Teodoro_2026.Components.Shared;
using SI_Andrey_Teodoro_2026.DTOs;

namespace SI_Andrey_Teodoro_2026.Pages;

public abstract class BasePage<TListDto, TDto> : ComponentBase
    where TDto : new()
{
    [Inject] protected ISnackbar Snackbar { get; set; } = null!;
    [Inject] protected IDialogService DialogService { get; set; } = null!;

    protected PaginacaoDto<TListDto>? _resultado;
    protected FiltroConsultaDto _filtro = new();

    protected TDto _dto = new TDto();

    protected override Task OnInitializedAsync()
    {
        InicializarDto();
        return base.OnInitializedAsync();
    }

    protected virtual void InicializarDto() { }

    protected MudForm _form = null!;
    protected bool _formValido;
    protected bool _carregando;
    protected bool _salvando;

    protected abstract Task CarregarDados();
    protected abstract string NomeEntidade { get; }

    protected async Task Pesquisar()
    {
        _filtro.Pagina = 1;
        await CarregarDados();
    }

    protected async Task LimparFiltros()
    {
        _filtro = new();
        await CarregarDados();
    }

    protected async Task MudarPagina(int pagina)
    {
        _filtro.Pagina = pagina;
        await CarregarDados();
    }

    protected async Task ConfirmarAlteracaoStatus(
        int id,
        string nome,
        bool ativoAtual,
        Func<int, bool, Task<(bool sucesso, string mensagem)>> alterarStatus,
        Func<Task> recarregar)
    {
        var param = new DialogParameters<ConfirmDialog>
        {
            { x => x.Titulo,     $"Confirmar {(ativoAtual ? "desativar" : "ativar")}" },
            { x => x.Mensagem,   $"Deseja realmente {(ativoAtual ? "desativar" : "ativar")} {NomeEntidade.ToLower()} \"{nome}\"?" },
            { x => x.TextoBotao, ativoAtual ? "Desativar" : "Ativar" },
            { x => x.CorBotao,   ativoAtual ? Color.Error : Color.Success }
        };

        var dialog = await DialogService.ShowAsync<ConfirmDialog>(
            "Confirmar", param,
            new DialogOptions { CloseOnEscapeKey = true, BackdropClick = false, MaxWidth = MaxWidth.Small });

        if ((await dialog.Result) is { Canceled: false })
        {
            var (sucesso, mensagem) = await alterarStatus(id, !ativoAtual);
            Snackbar.Add(mensagem, sucesso ? Severity.Success : Severity.Error);
            if (sucesso) await recarregar();
        }
    }
}