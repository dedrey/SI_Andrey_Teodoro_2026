using System.Text.RegularExpressions;
using MySqlConnector;
using SI_Andrey_Teodoro_2026.Repositories;

namespace SI_Andrey_Teodoro_2026.Helpers;

public static class ErroBancoHelper
{
    public static ILogger? Logger { get; set; }

    private static readonly Dictionary<string, (bool feminino, string nome)> Campos = new(StringComparer.OrdinalIgnoreCase)
    {
        ["PRIMARY"] = (false, "ID"),
        ["id"] = (false, "ID"),
        ["pais"] = (false, "país"),
        ["sigla"] = (true, "sigla"),
        ["ddi"] = (false, "DDI"),
        ["moeda"] = (true, "moeda"),
        ["simbolo_moeda"] = (false, "símbolo da moeda"),
        ["estado"] = (false, "estado"),
        ["uf"] = (true, "UF"),
        ["unique_uf_pais"] = (true, "UF neste país"),
        ["cidade"] = (true, "cidade"),
        ["unique_cidade_estado"] = (true, "cidade neste estado"),
        ["ddd"] = (false, "DDD"),
        ["razaosocial"] = (true, "razão social"),
        ["nome_razaosocial"] = (false, "nome/razão social"),
        ["cpf_cnpj"] = (false, "CPF/CNPJ"),
        ["cnpj"] = (false, "CNPJ"),
        ["cpf"] = (false, "CPF"),
        ["nomefantasia"] = (false, "nome fantasia"),
        ["apelido_nomefantasia"] = (false, "apelido/nome fantasia"),
        ["documento_estrangeiro"] = (false, "documento estrangeiro"),
        ["endereco"] = (false, "endereço"),
        ["numero"] = (false, "número"),
        ["complemento"] = (false, "complemento"),
        ["bairro"] = (false, "bairro"),
        ["cep"] = (false, "CEP"),
        ["telefone"] = (false, "telefone"),
        ["celular"] = (false, "celular"),
        ["email"] = (false, "e-mail"),
        ["inscricao_estadual"] = (true, "inscrição estadual"),
        ["inscricao_municipal"] = (true, "inscrição municipal"),
        ["categoria"] = (true, "categoria"),
        ["marca"] = (true, "marca"),
        ["unidade_medida"] = (true, "unidade de medida"),
        ["descricao"] = (true, "descrição"),
        ["nome"] = (false, "nome"),
        ["codigo"] = (false, "código"),
        ["metodo_pagamento"] = (false, "método de pagamento"),
        ["condicao_pagamento"] = (true, "condição de pagamento"),
        ["placa"] = (true, "placa"),
        ["produto"] = (false, "produto"),
        ["codigo_barras"] = (false, "código de barras"),
        ["uq_produtos_codigo_barras"] = (false, "código de barras"),
        ["unique_variacao"] = (true, "combinação de cor e tamanho"),
        ["cor"] = (true, "cor"),
        ["tamanho"] = (false, "tamanho"),
        ["observacao"] = (true, "observação"),
        ["motivo_cancelamento"] = (false, "motivo do cancelamento"),
        ["numero_nf"] = (false, "número da nota"),
        ["chave_acesso"] = (true, "chave de acesso"),
        ["unique_nfe"] = (true, "série e número de nota"),
        ["natureza_operacao"] = (true, "natureza da operação"),
        ["comprovante_arquivo"] = (false, "comprovante")
    };

    public static string Mensagem(Exception ex, string acao)
    {
        if (ex is IdDuplicadoException) return ex.Message;

        var mysql = Encontrar(ex);
        if (mysql?.ErrorCode == MySqlErrorCode.DuplicateKeyEntry)
        {
            var chave = Regex.Match(mysql.Message, @"for key '(?:[^'.]+\.)?([^']+)'").Groups[1].Value;
            Registrar(ex, acao, LogLevel.Information);
            return Descrever(chave) is { } campo
                ? $"Já existe um registro com {(campo.feminino ? "esta" : "este")} {campo.nome}."
                : "Já existe um registro com estes dados.";
        }
        if (mysql?.ErrorCode == MySqlErrorCode.DataTooLong)
        {
            var coluna = Regex.Match(mysql.Message, @"column '([^']+)'").Groups[1].Value;
            Registrar(ex, acao, LogLevel.Warning);
            return $"O campo {(Descrever(coluna)?.nome ?? coluna)} excede o tamanho permitido.";
        }

        Registrar(ex, acao, LogLevel.Error);
        return $"Não foi possível {acao}. Tente novamente e, se o problema continuar, contate o suporte.";
    }

    private static MySqlException? Encontrar(Exception? ex)
    {
        while (ex != null)
        {
            if (ex is MySqlException m) return m;
            ex = ex.InnerException;
        }
        return null;
    }

    private static (bool feminino, string nome)? Descrever(string chave)
    {
        if (string.IsNullOrWhiteSpace(chave)) return null;
        if (Campos.TryGetValue(chave, out var campo)) return campo;
        var semSufixo = Regex.Replace(chave, @"_\d+$", "");
        return Campos.TryGetValue(semSufixo, out campo) ? campo : null;
    }

    private static void Registrar(Exception ex, string acao, LogLevel nivel)
        => Logger?.Log(nivel, ex, "Erro ao {Acao}", acao);
}
