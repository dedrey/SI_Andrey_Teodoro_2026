using SI_Andrey_Teodoro_2026.DTOs;

namespace SI_Andrey_Teodoro_2026.Services;

public abstract class BaseService<TDto, TListDto>
{
    protected abstract string NomeEntidade { get; }

    protected (bool sucesso, string mensagem) Erro(Exception ex)
        => (false, $"Erro ao salvar {NomeEntidade.ToLower()}: {ex.Message}");

    protected (bool sucesso, string mensagem) ErroStatus(Exception ex)
        => (false, $"Erro ao alterar status: {ex.Message}");

    protected (bool sucesso, string mensagem) SucessoStatus(bool ativar)
        => (true, $"{NomeEntidade} {(ativar ? "ativado" : "desativado")} com sucesso!");
}