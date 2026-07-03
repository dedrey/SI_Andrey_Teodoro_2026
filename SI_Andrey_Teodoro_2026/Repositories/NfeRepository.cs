using Dapper;
using SI_Andrey_Teodoro_2026.Data;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Models;
using SI_Andrey_Teodoro_2026.Repositories.Interfaces;

namespace SI_Andrey_Teodoro_2026.Repositories;

public class NfeRepository : BaseRepository, INfeRepository
{
    public NfeRepository(DbConnectionFactory factory) : base(factory) { }

    protected override string Tabela => "nfes";

    public async Task<PaginacaoDto<NfeListDto>> ObterTodosAsync(FiltroConsultaDto filtro)
    {
        using var conn = _factory.CreateConnection();
        var where = new List<string>();
        if (!string.IsNullOrWhiteSpace(filtro.Busca))
            where.Add(@"(c.nome_razaosocial LIKE @Busca
                      OR e.nome_razaosocial LIKE @Busca
                      OR CAST(n.numero AS CHAR) = @BuscaExata
                      OR CAST(n.id AS CHAR) = @BuscaExata)");
        where.Add(filtro.StatusFiltro switch
        {
            "emitida" => "n.status_nfe = 'EMITIDA'",
            "cancelada" => "n.status_nfe = 'CANCELADA'",
            _ => "1=1"
        });
        var whereClause = "WHERE " + string.Join(" AND ", where);
        var orderBy = filtro.OrdenarPor switch
        {
            "id" => "n.id",
            "numero" => "n.numero DESC",
            _ => "n.data_emissao DESC"
        };

        var sqlCount = $@"SELECT COUNT(*) FROM nfes n
                          INNER JOIN emitentes e ON e.id = n.emitente_id
                          INNER JOIN clientes  c ON c.id = n.cliente_id
                          {whereClause}";

        var sqlData = $@"SELECT n.id, n.numero, n.serie, n.data_emissao AS DataEmissao,
                                 e.nome_razaosocial AS NomeEmitente,
                                 c.nome_razaosocial AS NomeCliente,
                                 n.venda_id AS VendaId,
                                 t.razaosocial AS NomeTransportadora,
                                 n.status_nfe AS StatusNfe,
                                 n.valor_total AS ValorTotal,
                                 n.criado_em AS CriadoEm
                          FROM nfes n
                          INNER JOIN emitentes e ON e.id = n.emitente_id
                          INNER JOIN clientes  c ON c.id = n.cliente_id
                          LEFT JOIN transportadoras t ON t.id = n.transportadora_id
                          {whereClause}
                          ORDER BY {orderBy} LIMIT @Limit OFFSET @Offset";

        var param = new
        {
            Busca = $"%{filtro.Busca}%",
            BuscaExata = filtro.Busca,
            Limit = filtro.TamanhoPagina,
            Offset = (filtro.Pagina - 1) * filtro.TamanhoPagina
        };

        var total = await conn.ExecuteScalarAsync<int>(sqlCount, param);
        var itens = await conn.QueryAsync<NfeListDto>(sqlData, param);
        return new PaginacaoDto<NfeListDto>
        { Itens = itens.ToList(), TotalItens = total, Pagina = filtro.Pagina, TamanhoPagina = filtro.TamanhoPagina };
    }

    public async Task<NfeDto?> ObterPorIdAsync(int id)
    {
        using var conn = _factory.CreateConnection();
        var nfe = await conn.QueryFirstOrDefaultAsync<Nfe>(
            @"SELECT n.id, n.numero, n.serie,
                     n.data_emissao AS DataEmissao, n.data_saida AS DataSaida,
                     n.emitente_id AS EmitenteId,
                     e.nome_razaosocial AS NomeEmitente, e.cnpj AS CnpjEmitente,
                     CONCAT(e.endereco, IFNULL(CONCAT(', ', e.complemento), ''), ' — ', e.bairro) AS EnderecoEmitente,
                     ce.cidade AS CidadeEmitente,
                     e.regime_tributario AS RegimeTributarioEmitente,
                     n.cliente_id AS ClienteId,
                     c.nome_razaosocial AS NomeCliente, c.cpf_cnpj AS CpfCnpjCliente,
                     CONCAT(c.endereco, IFNULL(CONCAT(', ', c.complemento), ''), ' — ', c.bairro) AS EnderecoCliente,
                     cc.cidade AS CidadeCliente,
                     n.venda_id AS VendaId,
                     cp.condicao_pagamento AS NomeCondicao,
                     n.transportadora_id AS TransportadoraId, t.razaosocial AS NomeTransportadora,
                     n.status_nfe AS StatusNfe,
                     n.valor_produtos AS ValorProdutos, n.valor_desconto AS ValorDesconto,
                     n.valor_total AS ValorTotal,
                     n.criado_em AS CriadoEm
              FROM nfes n
              INNER JOIN emitentes e ON e.id = n.emitente_id
              LEFT  JOIN cidades   ce ON ce.id = e.cidade_id
              INNER JOIN clientes  c ON c.id = n.cliente_id
              LEFT  JOIN cidades   cc ON cc.id = c.cidade_id
              LEFT  JOIN transportadoras t ON t.id = n.transportadora_id
              LEFT  JOIN vendas    v ON v.id = n.venda_id
              LEFT  JOIN condicoes_pagamentos cp ON cp.id = v.condicao_pagamento_id
              WHERE n.id = @id", new { id });

        if (nfe == null) return null;

        var itensModel = await conn.QueryAsync<NfeProduto>(
            @"SELECT np.id, np.nfe_id AS NfeId, np.numero_item AS NumeroItem,
                     np.produto_variacao_id AS ProdutoVariacaoId,
                     np.descricao_item AS DescricaoItem,
                     np.unidade_medida_id AS UnidadeMedidaId,
                     u.unidade_medida AS SiglaUnidade,
                     np.quantidade, np.valor_unitario AS ValorUnitario,
                     np.valor_desconto AS ValorDesconto, np.valor_total AS ValorTotal,
                     pv.cor AS Cor, pv.tamanho AS Tamanho
              FROM nfes_produtos np
              INNER JOIN unidades_medida u ON u.id = np.unidade_medida_id
              LEFT  JOIN produto_variacoes pv ON pv.id = np.produto_variacao_id
              WHERE np.nfe_id = @id
              ORDER BY np.numero_item", new { id });

        // Converte Model -> DTO (camada de apresentação)
        return new NfeDto
        {
            Id = nfe.Id,
            Numero = nfe.Numero,
            Serie = nfe.Serie,
            DataEmissao = nfe.DataEmissao,
            DataSaida = nfe.DataSaida,
            EmitenteId = nfe.EmitenteId,
            NomeEmitente = nfe.NomeEmitente,
            CnpjEmitente = nfe.CnpjEmitente,
            EnderecoEmitente = nfe.EnderecoEmitente,
            CidadeEmitente = nfe.CidadeEmitente,
            RegimeTributarioEmitente = nfe.RegimeTributarioEmitente,
            ClienteId = nfe.ClienteId,
            NomeCliente = nfe.NomeCliente,
            CpfCnpjCliente = nfe.CpfCnpjCliente,
            EnderecoCliente = nfe.EnderecoCliente,
            CidadeCliente = nfe.CidadeCliente,
            VendaId = nfe.VendaId,
            NomeCondicao = nfe.NomeCondicao,
            TransportadoraId = nfe.TransportadoraId,
            NomeTransportadora = nfe.NomeTransportadora,
            StatusNfe = nfe.StatusNfe,
            ValorProdutos = nfe.ValorProdutos,
            ValorDesconto = nfe.ValorDesconto,
            ValorTotal = nfe.ValorTotal,
            CriadoEm = nfe.CriadoEm,
            Itens = itensModel.Select(i => new NfeProdutoDto
            {
                Id = i.Id,
                NfeId = i.NfeId,
                NumeroItem = i.NumeroItem,
                ProdutoVariacaoId = i.ProdutoVariacaoId,
                DescricaoItem = i.DescricaoItem,
                UnidadeMedidaId = i.UnidadeMedidaId,
                SiglaUnidade = i.SiglaUnidade,
                Quantidade = i.Quantidade,
                ValorUnitario = i.ValorUnitario,
                ValorDesconto = i.ValorDesconto,
                ValorTotal = i.ValorTotal,
                Cor = i.Cor,
                Tamanho = i.Tamanho
            }).ToList()
        };
    }

    public async Task<IEnumerable<VendaParaNfeDto>> ObterVendasDisponiveisAsync()
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryAsync<VendaParaNfeDto>(
            @"SELECT v.id, v.cliente_id AS ClienteId,
                     COALESCE(c.nome_razaosocial, 'Sem cliente') AS NomeCliente,
                     cp.condicao_pagamento AS NomeCondicao,
                     v.valor_subtotal AS ValorSubtotal,
                     v.valor_desconto AS ValorDesconto,
                     v.valor_total AS ValorTotal,
                     v.criado_em AS CriadoEm
              FROM vendas v
              LEFT JOIN clientes c ON c.id = v.cliente_id
              LEFT JOIN condicoes_pagamentos cp ON cp.id = v.condicao_pagamento_id
              WHERE v.status_venda = 'FINALIZADA'
                AND v.nfe_id IS NULL
              ORDER BY v.criado_em DESC");
    }

    public async Task<List<VendaItemParaNfeDto>> ObterItensVendaParaNfeAsync(int vendaId)
    {
        using var conn = _factory.CreateConnection();
        var result = await conn.QueryAsync<VendaItemParaNfeDto>(
            @"SELECT vi.produto_variacao_id AS ProdutoVariacaoId,
                     p.produto AS NomeProduto,
                     pv.cor AS Cor, pv.tamanho AS Tamanho,
                     p.unidade_medida_id AS UnidadeMedidaId,
                     u.unidade_medida AS SiglaUnidade,
                     vi.quantidade AS Quantidade,
                     vi.valor_unitario AS ValorUnitario,
                     vi.valor_desconto AS ValorDesconto,
                     vi.valor_total AS ValorTotal
              FROM vendas_itens vi
              INNER JOIN produto_variacoes pv ON pv.id = vi.produto_variacao_id
              INNER JOIN produtos p ON p.id = pv.produto_id
              INNER JOIN unidades_medida u ON u.id = p.unidade_medida_id
              WHERE vi.venda_id = @vendaId
              ORDER BY p.produto, pv.cor, pv.tamanho", new { vendaId });
        return result.ToList();
    }

    public async Task<int> ProximoNumeroAsync(int emitenteId, short serie)
    {
        using var conn = _factory.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            "SELECT COALESCE(MAX(numero), 0) + 1 FROM nfes WHERE emitente_id = @emitenteId AND serie = @serie",
            new { emitenteId, serie });
    }

    public async Task<int> InserirAsync(NfeDto dto, List<NfeProdutoDto> itens)
    {
        using var conn = _factory.CreateConnection();
        var proximoId = await ProximoIdAsync();

        await conn.ExecuteAsync(
            @"INSERT INTO nfes
                (id, numero, serie, data_emissao, data_saida, emitente_id, cliente_id,
                 venda_id, transportadora_id, tipo_operacao, status_nfe,
                 valor_produtos, valor_desconto, valor_total)
              VALUES
                (@ProximoId, @Numero, @Serie, @DataEmissao, @DataSaida, @EmitenteId, @ClienteId,
                 @VendaId, @TransportadoraId, 'SAIDA', @StatusNfe,
                 @ValorProdutos, @ValorDesconto, @ValorTotal)",
            new
            {
                ProximoId = proximoId,
                dto.Numero,
                dto.Serie,
                dto.DataEmissao,
                dto.DataSaida,
                dto.EmitenteId,
                dto.ClienteId,
                dto.VendaId,
                dto.TransportadoraId,
                dto.StatusNfe,
                dto.ValorProdutos,
                dto.ValorDesconto,
                dto.ValorTotal
            });

        var numeroItem = 1;
        foreach (var item in itens)
        {
            await conn.ExecuteAsync(
                @"INSERT INTO nfes_produtos
                    (nfe_id, numero_item, produto_variacao_id, descricao_item,
                     unidade_medida_id, quantidade, valor_unitario, valor_desconto, valor_total)
                  VALUES
                    (@nfeId, @numeroItem, @ProdutoVariacaoId, @DescricaoItem,
                     @UnidadeMedidaId, @Quantidade, @ValorUnitario, @ValorDesconto, @ValorTotal)",
                new
                {
                    nfeId = proximoId,
                    numeroItem,
                    item.ProdutoVariacaoId,
                    item.DescricaoItem,
                    item.UnidadeMedidaId,
                    item.Quantidade,
                    item.ValorUnitario,
                    item.ValorDesconto,
                    item.ValorTotal
                });
            numeroItem++;
        }

        if (dto.VendaId.HasValue)
            await conn.ExecuteAsync(
                "UPDATE vendas SET nfe_id = @proximoId WHERE id = @vendaId",
                new { proximoId, vendaId = dto.VendaId.Value });

        return proximoId;
    }

    public async Task AlterarStatusAsync(int id, string status)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE nfes SET status_nfe = @status, atualizado_em = NOW() WHERE id = @id",
            new { id, status });

        if (status == "CANCELADA")
            await conn.ExecuteAsync("UPDATE vendas SET nfe_id = NULL WHERE nfe_id = @id", new { id });
    }
}