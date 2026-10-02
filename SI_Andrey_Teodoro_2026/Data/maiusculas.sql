USE si_andrey_teodoro_2026;

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
       bairro       = UPPER(TRIM(bairro));

UPDATE categorias
   SET categoria = UPPER(TRIM(categoria));

UPDATE marcas
   SET marca = UPPER(TRIM(marca));

UPDATE unidades_medida
   SET unidade_medida = UPPER(TRIM(unidade_medida)),
       descricao      = UPPER(TRIM(descricao));

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
       inscricao_estadual    = UPPER(TRIM(inscricao_estadual)),
       inscricao_municipal   = UPPER(TRIM(inscricao_municipal));

UPDATE emitentes
   SET nome_razaosocial     = UPPER(TRIM(nome_razaosocial)),
       apelido_nomefantasia = UPPER(TRIM(apelido_nomefantasia)),
       endereco             = UPPER(TRIM(endereco)),
       numero               = UPPER(TRIM(numero)),
       complemento          = UPPER(TRIM(complemento)),
       bairro               = UPPER(TRIM(bairro)),
       inscricao_estadual   = UPPER(TRIM(inscricao_estadual)),
       inscricao_municipal  = UPPER(TRIM(inscricao_municipal));

UPDATE transportadoras
   SET razaosocial        = UPPER(TRIM(razaosocial)),
       nomefantasia       = UPPER(TRIM(nomefantasia)),
       inscricao_estadual = UPPER(TRIM(inscricao_estadual)),
       endereco           = UPPER(TRIM(endereco)),
       numero             = UPPER(TRIM(numero)),
       complemento        = UPPER(TRIM(complemento)),
       bairro             = UPPER(TRIM(bairro));

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
   SET tamanho = UPPER(TRIM(tamanho));

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
