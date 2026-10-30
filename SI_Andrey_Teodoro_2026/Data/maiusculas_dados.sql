USE si_andrey_teodoro_2026;

SELECT c.TABLE_NAME, c.COLUMN_NAME, c.COLLATION_NAME, s.INDEX_NAME
  FROM information_schema.COLUMNS c
  JOIN information_schema.STATISTICS s
    ON s.TABLE_SCHEMA = c.TABLE_SCHEMA AND s.TABLE_NAME = c.TABLE_NAME AND s.COLUMN_NAME = c.COLUMN_NAME
 WHERE c.TABLE_SCHEMA = DATABASE()
   AND s.NON_UNIQUE = 0
   AND c.DATA_TYPE IN ('char', 'varchar', 'text')
 ORDER BY c.TABLE_NAME, s.INDEX_NAME, c.COLUMN_NAME;

SELECT 'paises.sigla' AS chave, UPPER(TRIM(sigla)) AS valor, COUNT(*) AS qtd FROM paises GROUP BY UPPER(TRIM(sigla)) HAVING COUNT(*) > 1
UNION ALL SELECT 'paises.pais', UPPER(TRIM(pais)), COUNT(*) FROM paises GROUP BY UPPER(TRIM(pais)) HAVING COUNT(*) > 1
UNION ALL SELECT 'estados.pais_id+uf', CONCAT(pais_id, '/', UPPER(TRIM(uf))), COUNT(*) FROM estados GROUP BY CONCAT(pais_id, '/', UPPER(TRIM(uf))) HAVING COUNT(*) > 1
UNION ALL SELECT 'cidades.cidade+estado_id', CONCAT(UPPER(TRIM(cidade)), '/', estado_id), COUNT(*) FROM cidades GROUP BY CONCAT(UPPER(TRIM(cidade)), '/', estado_id) HAVING COUNT(*) > 1
UNION ALL SELECT 'fornecedores.razaosocial', UPPER(TRIM(razaosocial)), COUNT(*) FROM fornecedores GROUP BY UPPER(TRIM(razaosocial)) HAVING COUNT(*) > 1
UNION ALL SELECT 'categorias.categoria', UPPER(TRIM(categoria)), COUNT(*) FROM categorias GROUP BY UPPER(TRIM(categoria)) HAVING COUNT(*) > 1
UNION ALL SELECT 'marcas.marca', UPPER(TRIM(marca)), COUNT(*) FROM marcas GROUP BY UPPER(TRIM(marca)) HAVING COUNT(*) > 1
UNION ALL SELECT 'unidades_medida.unidade_medida', UPPER(TRIM(unidade_medida)), COUNT(*) FROM unidades_medida GROUP BY UPPER(TRIM(unidade_medida)) HAVING COUNT(*) > 1
UNION ALL SELECT 'cores.nome', UPPER(TRIM(nome)), COUNT(*) FROM cores GROUP BY UPPER(TRIM(nome)) HAVING COUNT(*) > 1
UNION ALL SELECT 'tamanhos.nome', UPPER(TRIM(nome)), COUNT(*) FROM tamanhos GROUP BY UPPER(TRIM(nome)) HAVING COUNT(*) > 1
UNION ALL SELECT 'transportadoras.razaosocial', UPPER(TRIM(razaosocial)), COUNT(*) FROM transportadoras GROUP BY UPPER(TRIM(razaosocial)) HAVING COUNT(*) > 1
UNION ALL SELECT 'veiculos.placa', UPPER(TRIM(placa)), COUNT(*) FROM veiculos GROUP BY UPPER(TRIM(placa)) HAVING COUNT(*) > 1
UNION ALL SELECT 'metodos_pagamento.codigo', UPPER(TRIM(codigo)), COUNT(*) FROM metodos_pagamento GROUP BY UPPER(TRIM(codigo)) HAVING COUNT(*) > 1
UNION ALL SELECT 'metodos_pagamento.metodo_pagamento', UPPER(TRIM(metodo_pagamento)), COUNT(*) FROM metodos_pagamento GROUP BY UPPER(TRIM(metodo_pagamento)) HAVING COUNT(*) > 1
UNION ALL SELECT 'condicoes_pagamentos.condicao_pagamento', UPPER(TRIM(condicao_pagamento)), COUNT(*) FROM condicoes_pagamentos GROUP BY UPPER(TRIM(condicao_pagamento)) HAVING COUNT(*) > 1
UNION ALL SELECT 'produto_variacoes.produto+cor+tamanho', CONCAT(produto_id, '/', UPPER(TRIM(cor)), '/', UPPER(TRIM(tamanho))), COUNT(*) FROM produto_variacoes GROUP BY CONCAT(produto_id, '/', UPPER(TRIM(cor)), '/', UPPER(TRIM(tamanho))) HAVING COUNT(*) > 1;

SET SQL_SAFE_UPDATES = 0;
START TRANSACTION;

UPDATE paises
   SET pais          = UPPER(TRIM(pais)),
       sigla         = UPPER(TRIM(sigla)),
       moeda         = UPPER(TRIM(moeda)),
       simbolo_moeda = UPPER(TRIM(simbolo_moeda));

UPDATE estados
   SET estado = UPPER(TRIM(estado)),
       uf     = UPPER(TRIM(uf));

UPDATE cidades
   SET cidade = UPPER(TRIM(cidade));

UPDATE fornecedores
   SET razaosocial  = UPPER(TRIM(razaosocial)),
       nomefantasia = UPPER(TRIM(nomefantasia)),
       endereco     = UPPER(TRIM(endereco)),
       numero       = UPPER(TRIM(numero)),
       complemento  = UPPER(TRIM(complemento)),
       bairro       = UPPER(TRIM(bairro)),
       email        = UPPER(TRIM(email));

UPDATE categorias
   SET categoria = UPPER(TRIM(categoria));

UPDATE marcas
   SET marca = UPPER(TRIM(marca));

UPDATE unidades_medida
   SET unidade_medida = UPPER(TRIM(unidade_medida)),
       descricao      = UPPER(TRIM(descricao));

UPDATE cores
   SET nome = UPPER(TRIM(nome));

UPDATE tamanhos
   SET nome = UPPER(TRIM(nome));

UPDATE clientes
   SET nome_razaosocial      = UPPER(TRIM(nome_razaosocial)),
       apelido_nomefantasia  = UPPER(TRIM(apelido_nomefantasia)),
       documento_estrangeiro = UPPER(TRIM(documento_estrangeiro)),
       endereco              = UPPER(TRIM(endereco)),
       numero                = UPPER(TRIM(numero)),
       complemento           = UPPER(TRIM(complemento)),
       bairro                = UPPER(TRIM(bairro)),
       email                 = UPPER(TRIM(email)),
       inscricao_estadual    = UPPER(TRIM(inscricao_estadual)),
       inscricao_municipal   = UPPER(TRIM(inscricao_municipal));

UPDATE emitentes
   SET nome_razaosocial     = UPPER(TRIM(nome_razaosocial)),
       apelido_nomefantasia = UPPER(TRIM(apelido_nomefantasia)),
       endereco             = UPPER(TRIM(endereco)),
       numero               = UPPER(TRIM(numero)),
       complemento          = UPPER(TRIM(complemento)),
       bairro               = UPPER(TRIM(bairro)),
       email                = UPPER(TRIM(email)),
       inscricao_estadual   = UPPER(TRIM(inscricao_estadual)),
       inscricao_municipal  = UPPER(TRIM(inscricao_municipal));

UPDATE transportadoras
   SET razaosocial        = UPPER(TRIM(razaosocial)),
       nomefantasia       = UPPER(TRIM(nomefantasia)),
       inscricao_estadual = UPPER(TRIM(inscricao_estadual)),
       endereco           = UPPER(TRIM(endereco)),
       numero             = UPPER(TRIM(numero)),
       complemento        = UPPER(TRIM(complemento)),
       bairro             = UPPER(TRIM(bairro)),
       email              = UPPER(TRIM(email));

UPDATE veiculos
   SET placa = UPPER(TRIM(placa)),
       uf    = UPPER(TRIM(uf));

UPDATE metodos_pagamento
   SET codigo           = UPPER(TRIM(codigo)),
       metodo_pagamento = UPPER(TRIM(metodo_pagamento));

UPDATE condicoes_pagamentos
   SET condicao_pagamento = UPPER(TRIM(condicao_pagamento));

UPDATE produtos
   SET produto   = UPPER(TRIM(produto)),
       descricao = UPPER(TRIM(descricao));

UPDATE produto_variacoes
   SET cor     = UPPER(TRIM(cor)),
       tamanho = UPPER(TRIM(tamanho));

UPDATE movimentacoes_estoque
   SET observacao = UPPER(TRIM(observacao));

UPDATE vendas
   SET motivo_cancelamento = UPPER(TRIM(motivo_cancelamento));

UPDATE compras
   SET numero_nf           = UPPER(TRIM(numero_nf)),
       motivo_cancelamento = UPPER(TRIM(motivo_cancelamento));

UPDATE nfes_produtos
   SET descricao_item = UPPER(TRIM(descricao_item));

UPDATE contas_pagar
   SET descricao = UPPER(TRIM(descricao));

UPDATE contas_receber
   SET descricao = UPPER(TRIM(descricao));

UPDATE contas_receber_baixas
   SET observacao = UPPER(TRIM(observacao));

COMMIT;
SET SQL_SAFE_UPDATES = 1;

SELECT tabela, coluna, pendentes FROM (
          SELECT 'paises' AS tabela, 'pais' AS coluna, SUM(BINARY pais <> BINARY UPPER(pais)) AS pendentes FROM paises
UNION ALL SELECT 'paises', 'sigla', SUM(BINARY sigla <> BINARY UPPER(sigla)) FROM paises
UNION ALL SELECT 'paises', 'moeda', SUM(BINARY moeda <> BINARY UPPER(moeda)) FROM paises
UNION ALL SELECT 'paises', 'simbolo_moeda', SUM(BINARY simbolo_moeda <> BINARY UPPER(simbolo_moeda)) FROM paises
UNION ALL SELECT 'estados', 'estado', SUM(BINARY estado <> BINARY UPPER(estado)) FROM estados
UNION ALL SELECT 'estados', 'uf', SUM(BINARY uf <> BINARY UPPER(uf)) FROM estados
UNION ALL SELECT 'cidades', 'cidade', SUM(BINARY cidade <> BINARY UPPER(cidade)) FROM cidades
UNION ALL SELECT 'fornecedores', 'razaosocial', SUM(BINARY razaosocial <> BINARY UPPER(razaosocial)) FROM fornecedores
UNION ALL SELECT 'fornecedores', 'nomefantasia', SUM(BINARY nomefantasia <> BINARY UPPER(nomefantasia)) FROM fornecedores
UNION ALL SELECT 'fornecedores', 'endereco', SUM(BINARY endereco <> BINARY UPPER(endereco)) FROM fornecedores
UNION ALL SELECT 'fornecedores', 'numero', SUM(BINARY numero <> BINARY UPPER(numero)) FROM fornecedores
UNION ALL SELECT 'fornecedores', 'complemento', SUM(BINARY complemento <> BINARY UPPER(complemento)) FROM fornecedores
UNION ALL SELECT 'fornecedores', 'bairro', SUM(BINARY bairro <> BINARY UPPER(bairro)) FROM fornecedores
UNION ALL SELECT 'fornecedores', 'email', SUM(BINARY email <> BINARY UPPER(email)) FROM fornecedores
UNION ALL SELECT 'categorias', 'categoria', SUM(BINARY categoria <> BINARY UPPER(categoria)) FROM categorias
UNION ALL SELECT 'marcas', 'marca', SUM(BINARY marca <> BINARY UPPER(marca)) FROM marcas
UNION ALL SELECT 'unidades_medida', 'unidade_medida', SUM(BINARY unidade_medida <> BINARY UPPER(unidade_medida)) FROM unidades_medida
UNION ALL SELECT 'unidades_medida', 'descricao', SUM(BINARY descricao <> BINARY UPPER(descricao)) FROM unidades_medida
UNION ALL SELECT 'cores', 'nome', SUM(BINARY nome <> BINARY UPPER(nome)) FROM cores
UNION ALL SELECT 'tamanhos', 'nome', SUM(BINARY nome <> BINARY UPPER(nome)) FROM tamanhos
UNION ALL SELECT 'clientes', 'nome_razaosocial', SUM(BINARY nome_razaosocial <> BINARY UPPER(nome_razaosocial)) FROM clientes
UNION ALL SELECT 'clientes', 'apelido_nomefantasia', SUM(BINARY apelido_nomefantasia <> BINARY UPPER(apelido_nomefantasia)) FROM clientes
UNION ALL SELECT 'clientes', 'documento_estrangeiro', SUM(BINARY documento_estrangeiro <> BINARY UPPER(documento_estrangeiro)) FROM clientes
UNION ALL SELECT 'clientes', 'endereco', SUM(BINARY endereco <> BINARY UPPER(endereco)) FROM clientes
UNION ALL SELECT 'clientes', 'numero', SUM(BINARY numero <> BINARY UPPER(numero)) FROM clientes
UNION ALL SELECT 'clientes', 'complemento', SUM(BINARY complemento <> BINARY UPPER(complemento)) FROM clientes
UNION ALL SELECT 'clientes', 'bairro', SUM(BINARY bairro <> BINARY UPPER(bairro)) FROM clientes
UNION ALL SELECT 'clientes', 'email', SUM(BINARY email <> BINARY UPPER(email)) FROM clientes
UNION ALL SELECT 'clientes', 'inscricao_estadual', SUM(BINARY inscricao_estadual <> BINARY UPPER(inscricao_estadual)) FROM clientes
UNION ALL SELECT 'clientes', 'inscricao_municipal', SUM(BINARY inscricao_municipal <> BINARY UPPER(inscricao_municipal)) FROM clientes
UNION ALL SELECT 'emitentes', 'nome_razaosocial', SUM(BINARY nome_razaosocial <> BINARY UPPER(nome_razaosocial)) FROM emitentes
UNION ALL SELECT 'emitentes', 'apelido_nomefantasia', SUM(BINARY apelido_nomefantasia <> BINARY UPPER(apelido_nomefantasia)) FROM emitentes
UNION ALL SELECT 'emitentes', 'endereco', SUM(BINARY endereco <> BINARY UPPER(endereco)) FROM emitentes
UNION ALL SELECT 'emitentes', 'numero', SUM(BINARY numero <> BINARY UPPER(numero)) FROM emitentes
UNION ALL SELECT 'emitentes', 'complemento', SUM(BINARY complemento <> BINARY UPPER(complemento)) FROM emitentes
UNION ALL SELECT 'emitentes', 'bairro', SUM(BINARY bairro <> BINARY UPPER(bairro)) FROM emitentes
UNION ALL SELECT 'emitentes', 'email', SUM(BINARY email <> BINARY UPPER(email)) FROM emitentes
UNION ALL SELECT 'emitentes', 'inscricao_estadual', SUM(BINARY inscricao_estadual <> BINARY UPPER(inscricao_estadual)) FROM emitentes
UNION ALL SELECT 'emitentes', 'inscricao_municipal', SUM(BINARY inscricao_municipal <> BINARY UPPER(inscricao_municipal)) FROM emitentes
UNION ALL SELECT 'transportadoras', 'razaosocial', SUM(BINARY razaosocial <> BINARY UPPER(razaosocial)) FROM transportadoras
UNION ALL SELECT 'transportadoras', 'nomefantasia', SUM(BINARY nomefantasia <> BINARY UPPER(nomefantasia)) FROM transportadoras
UNION ALL SELECT 'transportadoras', 'inscricao_estadual', SUM(BINARY inscricao_estadual <> BINARY UPPER(inscricao_estadual)) FROM transportadoras
UNION ALL SELECT 'transportadoras', 'endereco', SUM(BINARY endereco <> BINARY UPPER(endereco)) FROM transportadoras
UNION ALL SELECT 'transportadoras', 'numero', SUM(BINARY numero <> BINARY UPPER(numero)) FROM transportadoras
UNION ALL SELECT 'transportadoras', 'complemento', SUM(BINARY complemento <> BINARY UPPER(complemento)) FROM transportadoras
UNION ALL SELECT 'transportadoras', 'bairro', SUM(BINARY bairro <> BINARY UPPER(bairro)) FROM transportadoras
UNION ALL SELECT 'transportadoras', 'email', SUM(BINARY email <> BINARY UPPER(email)) FROM transportadoras
UNION ALL SELECT 'veiculos', 'placa', SUM(BINARY placa <> BINARY UPPER(placa)) FROM veiculos
UNION ALL SELECT 'veiculos', 'uf', SUM(BINARY uf <> BINARY UPPER(uf)) FROM veiculos
UNION ALL SELECT 'metodos_pagamento', 'codigo', SUM(BINARY codigo <> BINARY UPPER(codigo)) FROM metodos_pagamento
UNION ALL SELECT 'metodos_pagamento', 'metodo_pagamento', SUM(BINARY metodo_pagamento <> BINARY UPPER(metodo_pagamento)) FROM metodos_pagamento
UNION ALL SELECT 'condicoes_pagamentos', 'condicao_pagamento', SUM(BINARY condicao_pagamento <> BINARY UPPER(condicao_pagamento)) FROM condicoes_pagamentos
UNION ALL SELECT 'produtos', 'produto', SUM(BINARY produto <> BINARY UPPER(produto)) FROM produtos
UNION ALL SELECT 'produtos', 'descricao', SUM(BINARY descricao <> BINARY UPPER(descricao)) FROM produtos
UNION ALL SELECT 'produto_variacoes', 'cor', SUM(BINARY cor <> BINARY UPPER(cor)) FROM produto_variacoes
UNION ALL SELECT 'produto_variacoes', 'tamanho', SUM(BINARY tamanho <> BINARY UPPER(tamanho)) FROM produto_variacoes
UNION ALL SELECT 'movimentacoes_estoque', 'observacao', SUM(BINARY observacao <> BINARY UPPER(observacao)) FROM movimentacoes_estoque
UNION ALL SELECT 'vendas', 'motivo_cancelamento', SUM(BINARY motivo_cancelamento <> BINARY UPPER(motivo_cancelamento)) FROM vendas
UNION ALL SELECT 'compras', 'numero_nf', SUM(BINARY numero_nf <> BINARY UPPER(numero_nf)) FROM compras
UNION ALL SELECT 'compras', 'motivo_cancelamento', SUM(BINARY motivo_cancelamento <> BINARY UPPER(motivo_cancelamento)) FROM compras
UNION ALL SELECT 'nfes_produtos', 'descricao_item', SUM(BINARY descricao_item <> BINARY UPPER(descricao_item)) FROM nfes_produtos
UNION ALL SELECT 'contas_pagar', 'descricao', SUM(BINARY descricao <> BINARY UPPER(descricao)) FROM contas_pagar
UNION ALL SELECT 'contas_receber', 'descricao', SUM(BINARY descricao <> BINARY UPPER(descricao)) FROM contas_receber
UNION ALL SELECT 'contas_receber_baixas', 'observacao', SUM(BINARY observacao <> BINARY UPPER(observacao)) FROM contas_receber_baixas
) t
WHERE COALESCE(pendentes, 0) > 0;

SELECT 'condicoes_pagamentos' AS tabela, id, condicao_pagamento AS valor FROM condicoes_pagamentos ORDER BY id;
