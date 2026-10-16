using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Repositories;

namespace SI_Andrey_Teodoro_2026.Services;

public abstract class BaseService<TDto, TListDto>
{
    protected abstract string NomeEntidade { get; }
    protected virtual bool Feminino => false;

    protected (bool sucesso, string mensagem) Erro(Exception ex)
        => ex is IdDuplicadoException
            ? (false, ex.Message)
            : (false, $"Erro ao salvar {NomeEntidade.ToLower()}: {ex.Message}");

    protected static async Task<string?> ValidarIdAsync(int id, int idOriginal, Func<int, Task<bool>> existeId)
    {
        if (idOriginal > 0)
            return id != idOriginal ? "O ID não pode ser alterado." : null;
        if (id <= 0)
            return "O ID deve ser maior que zero.";
        if (await existeId(id))
            return "Já existe um registro com este ID.";
        return null;
    }

    protected (bool sucesso, string mensagem) ErroStatus(Exception ex)
        => (false, $"Erro ao alterar status: {ex.Message}");

    protected (bool sucesso, string mensagem) SucessoStatus(bool ativar)
        => (true, $"{NomeEntidade} {(ativar ? "ativad" : "desativad")}{(Feminino ? "a" : "o")} com sucesso!");
}