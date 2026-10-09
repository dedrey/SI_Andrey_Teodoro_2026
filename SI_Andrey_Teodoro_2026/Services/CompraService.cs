using System.Data;
using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using SI_Andrey_Teodoro_2026.Data;
using SI_Andrey_Teodoro_2026.DTOs;
using SI_Andrey_Teodoro_2026.Helpers;
using SI_Andrey_Teodoro_2026.Repositories.Interfaces;
using SI_Andrey_Teodoro_2026.Services.Interfaces;

namespace SI_Andrey_Teodoro_2026.Services;

public class CompraService : BaseService<CompraDto, CompraListDto>, ICompraService
{
    private readonly ICompraRepository _repo;
    private readonly IContaPagarRepository _contaPagarRepo;
    private readonly ICondicaoPagamentoRepository _condicaoRepo;
    private readonly IMovimentacaoEstoqueRepository _movRepo;
    private readonly IFornecedorRepository _fornecedorRepo;
    private readonly ITransportadoraRepository _transportadoraRepo;
    private readonly DbConnectionFactory _factory;

    private static readonly string[] ModelosNf = { "55", "65", "01", "04" };
    private const long TamanhoMaximoXml = 2 * 1024 * 1024;
    private static readonly XNamespace NsNfe = "http://www.portalfiscal.inf.br/nfe";

    public CompraService(ICompraRepository repo, IContaPagarRepository contaPagarRepo,
        ICondicaoPagamentoRepository condicaoRepo, IMovimentacaoEstoqueRepository movRepo,
        IFornecedorRepository fornecedorRepo, ITransportadoraRepository transportadoraRepo, DbConnectionFactory factory)
    {
        _repo = repo;
        _contaPagarRepo = contaPagarRepo;
        _condicaoRepo = condicaoRepo;
        _movRepo = movRepo;
        _fornecedorRepo = fornecedorRepo;
        _transportadoraRepo = transportadoraRepo;
        _factory = factory;
    }

    protected override string NomeEntidade => "Compra";

    public Task<PaginacaoDto<CompraListDto>> ObterTodosAsync(FiltroConsultaDto filtro)
        => _repo.ObterTodosAsync(filtro);

    public async Task<CompraDto?> ObterPorIdAsync(int id)
    {
        var c = await _repo.ObterPorIdAsync(id);
        if (c == null) return null;
        var itens = await _repo.ObterItensPorCompraAsync(id);
        return new CompraDto
        {
            Id = c.Id,
            IdOriginal = c.Id,
            FornecedorId = c.FornecedorId,
            NomeFornecedor = c.NomeFornecedor,
            TransportadoraId = c.TransportadoraId,
            NomeTransportadora = c.NomeTransportadora,
            ModeloNf = c.ModeloNf ?? string.Empty,
            SerieNf = c.SerieNf,
            NumeroNf = c.NumeroNf,
            ChaveAcesso = c.ChaveAcesso,
            DataEmissao = c.DataEmissao,
            DataChegada = c.DataChegada,
            CondicaoPagamentoId = c.CondicaoPagamentoId,
            NomeCondicaoPagamento = c.NomeCondicaoPagamento ?? string.Empty,
            ValorSubtotal = c.ValorSubtotal,
            ValorFrete = c.ValorFrete,
            ValorSeguro = c.ValorSeguro,
            ValorOutrosAcrescimos = c.ValorOutrosAcrescimos,
            ValorDesconto = c.ValorDesconto,
            ValorTotal = c.ValorTotal,
            StatusCompra = c.StatusCompra,
            MotivoCancelamento = c.MotivoCancelamento,
            CriadoEm = c.CriadoEm,
            AtualizadoEm = c.AtualizadoEm,
            Itens = itens.Select(i => new CompraItemDto
            {
                Id = i.Id,
                CompraId = i.CompraId,
                ProdutoVariacaoId = i.ProdutoVariacaoId,
                NomeProduto = i.NomeProduto,
                Cor = i.Cor,
                Tamanho = i.Tamanho,
                Quantidade = i.Quantidade,
                ValorUnitario = i.ValorUnitario,
                ValorDesconto = i.ValorDesconto,
                CustoUnitarioEfetivo = i.CustoUnitarioEfetivo
            }).ToList()
        };
    }

    public Task<List<CompraItemListDto>> ObterItensAsync(int compraId)
        => _repo.ObterItensPorCompraAsync(compraId);

    public Task<List<ContaPagarListDto>> ObterContasPagarAsync(int compraId)
        => _contaPagarRepo.ObterPorCompraAsync(compraId);

    public async Task<List<CompraParcelaDto>> SimularParcelasAsync(int condicaoPagamentoId,
        DateTime dataEmissao, decimal valorTotal, DateTime? primeiroVencimento = null)
    {
        var parcelas = new List<CompraParcelaDto>();
        var condicao = await _condicaoRepo.ObterPorIdAsync(condicaoPagamentoId);
        if (condicao == null || condicao.NumeroParcelas <= 0) return parcelas;

        var n = condicao.NumeroParcelas;
        var config = await _condicaoRepo.ObterParcelasAsync(condicaoPagamentoId);
        var dias1 = config.FirstOrDefault(x => x.NumeroParcela == 1)?.DiasVencimento ?? 30;

        var primeiro = (primeiroVencimento ?? dataEmissao.AddDays(dias1)).Date;
        var valorParcela = Math.Round(valorTotal / n, 2);
        var diferenca = valorTotal - valorParcela * n;

        for (int p = 1; p <= n; p++)
        {
            parcelas.Add(new CompraParcelaDto
            {
                Numero = p,
                DataVencimento = primeiro.AddMonths(p - 1),
                Valor = p == n ? valorParcela + diferenca : valorParcela
            });
        }
        return parcelas;
    }

    public async Task<(bool sucesso, string mensagem, int id)> SalvarAsync(CompraDto dto)
    {
        try
        {
            dto.ModeloNf = (dto.ModeloNf ?? "").Trim();
            dto.NumeroNf = string.IsNullOrWhiteSpace(dto.NumeroNf) ? null : dto.NumeroNf.Trim();
            dto.ChaveAcesso = string.IsNullOrWhiteSpace(dto.ChaveAcesso) ? null : ChaveNfeHelper.SomenteDigitos(dto.ChaveAcesso);

            if (!dto.FornecedorId.HasValue)
                return (false, "Selecione o fornecedor.", 0);

            var fornecedor = await _fornecedorRepo.ObterPorIdAsync(dto.FornecedorId.Value);
            if (fornecedor == null)
                return (false, "Fornecedor não encontrado.", 0);
            if (!fornecedor.CondicaoPagamentoId.HasValue)
                return (false, "O fornecedor não possui condição de pagamento cadastrada. Atualize o cadastro do fornecedor.", 0);
            dto.CondicaoPagamentoId = fornecedor.CondicaoPagamentoId;

            if (!dto.TransportadoraId.HasValue) return (false, "Selecione a transportadora.", 0);

            var itensValidos = dto.Itens.Where(i => !i.Removido).ToList();
            if (itensValidos.Count == 0)
                return (false, "Adicione pelo menos um item à compra.", 0);

            if (!dto.DataEmissao.HasValue)
                return (false, "Informe a data de emissão.", 0);
            if (dto.DataEmissao.Value.Date > DateTime.Today)
                return (false, "A data de emissão não pode ser maior que a data atual.", 0);

            if (!dto.DataChegada.HasValue)
                return (false, "Informe a data de chegada.", 0);
            if (dto.DataChegada.Value.Date < dto.DataEmissao.Value.Date)
                return (false, "A data de chegada não pode ser anterior à data de emissão.", 0);
            if (dto.DataChegada.Value.Date > DateTime.Today)
                return (false, "A data de chegada não pode ser maior que a data atual.", 0);

            if (!ModelosNf.Contains(dto.ModeloNf))
                return (false, "Selecione o modelo da nota fiscal.", 0);
            if (!dto.SerieNf.HasValue || dto.SerieNf < 0 || dto.SerieNf > 999)
                return (false, "Informe a série da nota fiscal (0 a 999).", 0);
            if (dto.NumeroNf == null || dto.NumeroNf.Length > 9 || !dto.NumeroNf.All(char.IsDigit))
                return (false, "Informe o número da nota fiscal (somente números, até 9 dígitos).", 0);
            dto.NumeroNf = dto.NumeroNf.TrimStart('0');
            if (dto.NumeroNf.Length == 0) dto.NumeroNf = "0";

            if (dto.ChaveAcesso != null)
            {
                if (!ChaveNfeHelper.Validar(dto.ChaveAcesso))
                    return (false, "Chave de acesso inválida.", 0);

                var (cnpj, modelo, serie, numero, ano, mes) = ChaveNfeHelper.Decompor(dto.ChaveAcesso);
                var divergencias = new List<string>();
                if (modelo != dto.ModeloNf) divergencias.Add("modelo");
                if (serie != dto.SerieNf) divergencias.Add("série");
                if (numero != dto.NumeroNf) divergencias.Add("número");
                if (ano != dto.DataEmissao.Value.Year || mes != dto.DataEmissao.Value.Month) divergencias.Add("data de emissão");
                if (cnpj != ChaveNfeHelper.SomenteDigitos(fornecedor.CpfCnpj)) divergencias.Add("CNPJ do fornecedor");
                if (divergencias.Count > 0)
                    return (false, $"A chave de acesso não confere com: {string.Join(", ", divergencias)}.", 0);
            }

            var compraExistente = await _repo.ObterCompraComMesmaNotaAsync(dto.FornecedorId.Value, dto.ModeloNf, dto.SerieNf.Value, dto.NumeroNf);
            if (compraExistente.HasValue)
                return (false, $"Esta nota já foi lançada na Compra #{compraExistente}.", 0);

            if (dto.ValorFrete < 0)
                return (false, "O frete não pode ser negativo.", 0);
            if (dto.ValorSeguro < 0) return (false, "O seguro não pode ser negativo.", 0);
            if (dto.ValorOutrosAcrescimos < 0)
                return (false, "Outros acréscimos não podem ser negativos.", 0);

            foreach (var item in itensValidos)
            {
                if (item.ProdutoVariacaoId == 0)
                    return (false, "Selecione a variação de todos os itens.", 0);
                if (item.Quantidade <= 0)
                    return (false, $"{Desc(item)}: quantidade deve ser maior que zero.", 0);
                if (item.ValorUnitario <= 0)
                    return (false, $"{Desc(item)}: custo unitário deve ser maior que zero.", 0);
                if (item.ValorDesconto < 0 || item.ValorDesconto > item.ValorUnitario * item.Quantidade)
                    return (false, $"{Desc(item)}: desconto não pode ser maior que o valor do item.", 0);
            }

            var duplicada = itensValidos.GroupBy(i => i.ProdutoVariacaoId).FirstOrDefault(g => g.Count() > 1);
            if (duplicada != null)
                return (false, $"{Desc(duplicada.First())} foi adicionado mais de uma vez. " +
                               "Junte as quantidades em um único item.", 0);

            dto.ValorSubtotal = itensValidos.Sum(i => i.ValorUnitario * i.Quantidade);
            dto.ValorDesconto = itensValidos.Sum(i => i.ValorDesconto);
            dto.ValorTotal = dto.ValorSubtotal - dto.ValorDesconto + dto.ValorFrete + dto.ValorSeguro + dto.ValorOutrosAcrescimos;

            RatearCusto(itensValidos, dto.ValorFrete + dto.ValorSeguro + dto.ValorOutrosAcrescimos);

            var parcelasGerar = new List<CompraParcelaDto>();

            if (dto.CondicaoPagamentoId.HasValue && dto.ValorTotal > 0)
            {
                if (dto.Parcelas.Count == 0)
                {
                    parcelasGerar = await SimularParcelasAsync(dto.CondicaoPagamentoId.Value,
                        dto.DataEmissao.Value, dto.ValorTotal);
                }
                else
                {
                    var condicao = await _condicaoRepo.ObterPorIdAsync(dto.CondicaoPagamentoId.Value);
                    if (condicao == null)
                        return (false, "Condição de pagamento não encontrada.", 0);
                    if (dto.Parcelas.Count != condicao.NumeroParcelas)
                        return (false, $"A condição de pagamento exige {condicao.NumeroParcelas} parcela(s), " +
                                       $"mas foram informadas {dto.Parcelas.Count}.", 0);

                    DateTime? anterior = null;
                    foreach (var p in dto.Parcelas)
                    {
                        if (!p.DataVencimento.HasValue)
                            return (false, $"Parcela {p.Numero}: informe a data de vencimento.", 0);
                        if (p.Valor <= 0)
                            return (false, $"Parcela {p.Numero}: o valor deve ser maior que zero.", 0);
                        if (p.DataVencimento.Value.Date < dto.DataEmissao.Value.Date)
                            return (false, $"Parcela {p.Numero}: o vencimento não pode ser anterior à data de emissão.", 0);
                        if (anterior.HasValue && p.DataVencimento.Value.Date < anterior.Value)
                            return (false, $"Parcela {p.Numero}: os vencimentos devem estar em ordem crescente.", 0);
                        anterior = p.DataVencimento.Value.Date;
                    }

                    var soma = Math.Round(dto.Parcelas.Sum(p => p.Valor), 2);
                    var total = Math.Round(dto.ValorTotal, 2);
                    if (soma != total)
                        return (false, $"A soma das parcelas (R$ {soma:N2}) é diferente do total da nota (R$ {total:N2}).", 0);

                    parcelasGerar = dto.Parcelas;
                }
            }
            var totalParcelas = parcelasGerar.Count;

            using var conn = _factory.CreateConnection();
            if (conn.State != ConnectionState.Open) conn.Open();
            using var tx = conn.BeginTransaction();

            var novoId = await _repo.InserirAsync(dto, tx);

            var obsEntrada = $"COMPRA #{novoId}" +
                (string.IsNullOrWhiteSpace(dto.NumeroNf) ? "" : $" — NF {dto.NumeroNf}");
            var movId = await _movRepo.InserirAsync("ENTRADA", obsEntrada, novoId, tx);

            foreach (var item in itensValidos)
            {
                await _repo.InserirItemAsync(item, novoId, tx);

                if (!await _repo.AtualizarEstoqueAsync(item.ProdutoVariacaoId, +item.Quantidade, tx))
                    return (false, $"{Desc(item)}: variação sem registro de estoque. Compra não foi gravada.", 0);

                await _movRepo.InserirItemAsync(movId, item.ProdutoVariacaoId, item.Quantidade,
                    item.CustoUnitarioEfetivo, tx);

                await _repo.RecalcularCustoVariacaoAsync(item.ProdutoVariacaoId, tx);
            }

            foreach (var parcela in parcelasGerar)
            {
                var numero = parcela.Numero;
                var descricao = totalParcelas == 1
                    ? $"COMPRA #{novoId}" + (string.IsNullOrWhiteSpace(dto.NumeroNf) ? "" : $" — NF {dto.NumeroNf}")
                    : $"COMPRA #{novoId} — PARCELA {numero}/{totalParcelas}";

                await _contaPagarRepo.InserirAutomaticaAsync(dto.FornecedorId, novoId, descricao,
                    parcela.DataVencimento!.Value.Date, parcela.Valor, tx);
            }

            tx.Commit();

            return (true, "Compra lançada com sucesso! Estoque e custo atualizados" +
                (parcelasGerar.Count > 0 ? " e contas a pagar geradas." : "."), novoId);
        }
        catch (Exception ex) { return (false, Erro(ex).mensagem, 0); }
    }

    public async Task<(bool sucesso, string mensagem)> CancelarAsync(int compraId, string motivo)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(motivo))
                return (false, "Informe o motivo do cancelamento.");
            motivo = motivo.Trim().ToUpperInvariant();
            if (motivo.Length > 255)
                return (false, "O motivo do cancelamento deve ter no máximo 255 caracteres.");

            var compra = await _repo.ObterPorIdAsync(compraId);
            if (compra == null) return (false, "Compra não encontrada.");
            if (compra.StatusCompra == "CANCELADO") return (false, "Compra já está cancelada.");

            if (await _contaPagarRepo.ExisteParcelaPagaAsync(compraId))
                return (false, "Não é possível cancelar: já existe parcela paga para esta compra.");

            var itens = await _repo.ObterItensPorCompraAsync(compraId);

            using var conn = _factory.CreateConnection();
            if (conn.State != ConnectionState.Open) conn.Open();
            using var tx = conn.BeginTransaction();

            var obsEstorno = $"ESTORNO — CANCELAMENTO DA COMPRA #{compraId}: {motivo}";
            if (obsEstorno.Length > 200) obsEstorno = obsEstorno[..200];
            var movId = await _movRepo.InserirAsync("SAIDA", obsEstorno, compraId, tx);

            foreach (var item in itens)
            {
                if (!await _repo.AtualizarEstoqueAsync(item.ProdutoVariacaoId, -item.Quantidade, tx))
                    return (false, $"Não é possível cancelar: estoque insuficiente de " +
                                   $"{item.NomeProduto} {item.Cor}/{item.Tamanho} para estornar " +
                                   $"{item.Quantidade} un. (parte já foi vendida).");

                await _movRepo.InserirItemAsync(movId, item.ProdutoVariacaoId, item.Quantidade,
                    item.CustoUnitarioEfetivo, tx);
            }

            await _contaPagarRepo.CancelarPorCompraAsync(compraId, tx);
            await _repo.AtualizarStatusAsync(compraId, "CANCELADO", tx, motivoCancelamento: motivo);

            foreach (var variacaoId in itens.Select(i => i.ProdutoVariacaoId).Distinct())
                await _repo.RecalcularCustoVariacaoAsync(variacaoId, tx);

            tx.Commit();
            return (true, "Compra cancelada com sucesso! Estoque estornado, custo recalculado e parcelas em aberto canceladas.");
        }
        catch (Exception ex) { return (false, Erro(ex).mensagem); }
    }

    public async Task<(bool sucesso, string mensagem, CompraXmlDto? dados)> LerXmlNfeAsync(Stream xml)
    {
        const string invalido = "Arquivo XML não é uma NF-e válida.";
        try
        {
            using var ms = new MemoryStream();
            var buffer = new byte[81920];
            int lidos;
            while ((lidos = await xml.ReadAsync(buffer)) > 0)
            {
                ms.Write(buffer, 0, lidos);
                if (ms.Length > TamanhoMaximoXml)
                    return (false, "O arquivo XML deve ter no máximo 2 MB.", null);
            }
            ms.Position = 0;

            XDocument doc;
            try
            {
                var settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null };
                using var reader = XmlReader.Create(ms, settings);
                doc = XDocument.Load(reader);
            }
            catch (XmlException) { return (false, invalido, null); }

            var raiz = doc.Root;
            var nfe = raiz?.Name == NsNfe + "nfeProc" ? raiz.Element(NsNfe + "NFe")
                    : raiz?.Name == NsNfe + "NFe" ? raiz
                    : null;
            var inf = nfe?.Element(NsNfe + "infNFe");
            if (inf == null) return (false, invalido, null);

            var ide = inf.Element(NsNfe + "ide");
            var emit = inf.Element(NsNfe + "emit");
            var transporta = inf.Element(NsNfe + "transp")?.Element(NsNfe + "transporta");
            var tot = inf.Element(NsNfe + "total")?.Element(NsNfe + "ICMSTot");

            var chave = ((string?)inf.Attribute("Id") ?? "").Trim();
            if (chave.StartsWith("NFe")) chave = chave[3..];
            if (!ChaveNfeHelper.Validar(chave))
                return (false, "Chave de acesso do XML inválida.", null);

            var modelo = ide?.Element(NsNfe + "mod")?.Value.Trim();
            var serieTexto = ide?.Element(NsNfe + "serie")?.Value.Trim();
            var numeroTexto = ide?.Element(NsNfe + "nNF")?.Value.Trim();
            var emissaoTexto = (ide?.Element(NsNfe + "dhEmi") ?? ide?.Element(NsNfe + "dEmi"))?.Value.Trim();
            if (string.IsNullOrEmpty(modelo) || !int.TryParse(serieTexto, out var serie)
                || string.IsNullOrEmpty(numeroTexto) || !numeroTexto.All(char.IsDigit)
                || emissaoTexto == null || emissaoTexto.Length < 10
                || !DateTime.TryParseExact(emissaoTexto[..10], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var emissao))
                return (false, invalido, null);

            var numero = numeroTexto.TrimStart('0');
            var dados = new CompraXmlDto
            {
                ChaveAcesso = chave,
                ModeloNf = modelo,
                SerieNf = serie,
                NumeroNf = numero.Length == 0 ? "0" : numero,
                DataEmissao = emissao,
                CnpjEmitente = ChaveNfeHelper.SomenteDigitos((emit?.Element(NsNfe + "CNPJ") ?? emit?.Element(NsNfe + "CPF"))?.Value),
                NomeEmitente = emit?.Element(NsNfe + "xNome")?.Value.Trim() ?? string.Empty,
                CnpjTransportadora = transporta == null ? null : ChaveNfeHelper.SomenteDigitos((transporta.Element(NsNfe + "CNPJ") ?? transporta.Element(NsNfe + "CPF"))?.Value),
                NomeTransportadora = transporta?.Element(NsNfe + "xNome")?.Value.Trim(),
                ValorFrete = Valor(tot, "vFrete"),
                ValorSeguro = Valor(tot, "vSeg"),
                ValorOutros = Valor(tot, "vOutro"),
                ValorTotalNota = Valor(tot, "vNF")
            };

            if (dados.CnpjEmitente.Length > 0)
                dados.FornecedorId = (await _fornecedorRepo.ObterTodosAtivosAsync())
                    .FirstOrDefault(f => ChaveNfeHelper.SomenteDigitos(f.CpfCnpj) == dados.CnpjEmitente)?.Id;
            if (!string.IsNullOrEmpty(dados.CnpjTransportadora))
                dados.TransportadoraId = (await _transportadoraRepo.ObterTodosAtivosAsync())
                    .FirstOrDefault(t => ChaveNfeHelper.SomenteDigitos(t.Cnpj) == dados.CnpjTransportadora)?.Id;

            return (true, "XML lido com sucesso.", dados);
        }
        catch (Exception ex) { return (false, Erro(ex).mensagem, null); }
    }

    private static decimal Valor(XElement? pai, string nome)
    {
        var texto = pai?.Element(NsNfe + nome)?.Value;
        return decimal.TryParse(texto, NumberStyles.Number, CultureInfo.InvariantCulture, out var v) ? v : 0;
    }

    private static void RatearCusto(List<CompraItemDto> itens, decimal acrescimos)
    {
        var baseValor = itens.Sum(i => i.ValorTotal);
        var baseQtd = itens.Sum(i => i.Quantidade);

        foreach (var i in itens)
        {
            var proporcao = baseValor > 0
                ? i.ValorTotal / baseValor
                : (decimal)i.Quantidade / baseQtd;

            var custoTotalItem = i.ValorTotal + acrescimos * proporcao;
            i.CustoUnitarioEfetivo = Math.Round(custoTotalItem / i.Quantidade, 4);
        }
    }

    private static string Desc(CompraItemDto i) => $"{i.NomeProduto} {i.Cor}/{i.Tamanho}";
}