-- ================================================================
-- Migração: casas decimais da Compra
-- Rodar ANTES de publicar o código novo. É compatível com o código antigo
-- (desconto_unitario tem DEFAULT 0 e o código antigo não lê nem grava essa coluna).
-- Definições copiadas do banco.txt; só a escala muda. Confira antes com:
--   SHOW CREATE TABLE compras_itens;
--   SHOW CREATE TABLE produto_variacoes;
-- ================================================================

USE si_andrey_teodoro_2026;

-- 1. Preço unitário com 4 casas (era DECIMAL(14,2) NOT NULL, sem DEFAULT)
ALTER TABLE compras_itens
  MODIFY valor_unitario DECIMAL(14,4) NOT NULL;

-- 2. Desconto unitário (fonte da verdade do desconto da linha)
ALTER TABLE compras_itens
  ADD COLUMN desconto_unitario DECIMAL(14,4) NOT NULL DEFAULT 0 AFTER valor_unitario;

-- 3. Preenche as linhas antigas a partir do desconto total da linha
--    (o MySQL Workbench bloqueia UPDATE sem chave no WHERE com safe updates ligado)
SET @safe_updates_antes = @@SQL_SAFE_UPDATES;
SET SQL_SAFE_UPDATES = 0;
UPDATE compras_itens
   SET desconto_unitario = ROUND(valor_desconto / quantidade, 4)
 WHERE quantidade > 0 AND valor_desconto > 0;
SET SQL_SAFE_UPDATES = @safe_updates_antes;

-- 4. Custo da variação com 4 casas (era DECIMAL(14,2) NOT NULL DEFAULT 0.00)
--    DESATIVADO de propósito: Vendas (VendaService.cs:141) e Produtos
--    (ProdutoService.cs:116, ModalCadastroProduto.razor:203) comparam
--    preco < preco_custo e mostram o custo em N2. Com 4 casas, um custo de
--    10,0049 bloquearia a venda a R$ 10,00 com a mensagem "abaixo do custo
--    (R$ 10,00)". Só descomentar depois de ajustar essas comparações.
-- ALTER TABLE produto_variacoes
--   MODIFY preco_custo DECIMAL(14,4) NOT NULL DEFAULT 0.0000;
