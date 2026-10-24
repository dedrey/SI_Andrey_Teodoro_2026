using Microsoft.AspNetCore.Components;
using MudBlazor;
using SI_Andrey_Teodoro_2026.Components.Shared;
using SI_Andrey_Teodoro_2026.DTOs;

namespace SI_Andrey_Teodoro_2026.Pages;

public abstract class BaseListagemPage<TListDto> : ComponentBase
{
    [Inject] protected ISnackbar Snackbar { get; set; } = null!;
    [Inject] protected IDialogService DialogService { get; set; } = null!;

    protected PaginacaoDto<TListDto>? _resultado;
    protected FiltroConsultaDto _filtro;
    protected bool _carregando;
    private int _consultaAtual;

    protected BaseListagemPage()
    {
        _filtro = CriarFiltro();
        _filtro.TamanhoPagina = TamanhoPaginaPadrao;
    }

    protected const int TamanhoPaginaPadrao = 25;

    protected virtual FiltroConsultaDto CriarFiltro() => new();

    protected abstract Task<PaginacaoDto<TListDto>> BuscarDados(FiltroConsultaDto filtro);

    protected async Task CarregarDados()
    {
        var consulta = ++_consultaAtual;
        _carregando = true;
        try
        {
            var resultado = await BuscarDados(_filtro);
            if (consulta == _consultaAtual) _resultado = resultado;
        }
        catch (Exception ex)
        {
            if (consulta != _consultaAtual) return;
            Snackbar.Add($"Erro ao carregar: {ex.Message}", Severity.Error);
            _resultado = new();
        }
        finally
        {
            if (consulta == _consultaAtual) _carregando = false;
        }
    }

    protected Task Pesquisar()
    {
        _filtro.Pagina = 1;
        return CarregarDados();
    }

    protected Task AplicarBusca(string? texto)
    {
        if ((texto ?? "").Trim() == (_filtro.Busca ?? "").Trim()) return Task.CompletedTask;
        _filtro.Busca = texto;
        return Pesquisar();
    }

    protected Task AplicarFiltro(Action alterar)
    {
        alterar();
        return Pesquisar();
    }

    protected Task LimparFiltros()
    {
        var tamanho = _filtro.TamanhoPagina;
        _filtro = CriarFiltro();
        _filtro.TamanhoPagina = tamanho;
        return CarregarDados();
    }

    protected Task MudarPagina(int pagina)
    {
        _filtro.Pagina = pagina;
        return CarregarDados();
    }

    protected Task MudarTamanhoPagina(int tamanho)
    {
        _filtro.TamanhoPagina = tamanho;
        return Pesquisar();
    }
}

public abstract class BasePage<TListDto, TDto> : BaseListagemPage<TListDto>
    where TDto : new()
{
    protected TDto _dto = new TDto();

    protected override Task OnInitializedAsync()
    {
        InicializarDto();
        return base.OnInitializedAsync();
    }

    protected virtual void InicializarDto() { }

    protected MudForm _form = null!;
    protected bool _formValido;
    protected bool _salvando;

    protected abstract string NomeEntidade { get; }

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
            new DialogOptions { CloseOnEscapeKey = false, BackdropClick = false, MaxWidth = MaxWidth.Small });

        if ((await dialog.Result) is { Canceled: false })
        {
            var (sucesso, mensagem) = await alterarStatus(id, !ativoAtual);
            Snackbar.Add(mensagem, sucesso ? Severity.Success : Severity.Error);
            if (sucesso) await recarregar();
        }
    }
}