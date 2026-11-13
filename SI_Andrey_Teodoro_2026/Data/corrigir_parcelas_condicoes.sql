USE si_andrey_teodoro_2026;

SELECT c.id,
       c.condicao_pagamento,
       c.numero_parcelas,
       COUNT(p.id) AS linhas_parcelas,
       COALESCE(GROUP_CONCAT(CONCAT(p.numero_parcela, ': ', p.dias_vencimento, ' dias')
                ORDER BY p.numero_parcela, p.id SEPARATOR ' | '), '') AS parcelas_atuais
FROM condicoes_pagamentos c
LEFT JOIN condicoes_pagamentos_parcelas p ON p.condicao_pagamento_id = c.id
GROUP BY c.id, c.condicao_pagamento, c.numero_parcelas
HAVING COUNT(p.id) <> c.numero_parcelas
    OR COUNT(DISTINCT p.numero_parcela) <> COUNT(p.id)
    OR COALESCE(MIN(p.numero_parcela), 1) <> 1
    OR COALESCE(MAX(p.numero_parcela), 0) <> c.numero_parcelas
ORDER BY c.id;

DROP TEMPORARY TABLE IF EXISTS tmp_condicoes_inconsistentes;
CREATE TEMPORARY TABLE tmp_condicoes_inconsistentes AS
SELECT c.id, c.numero_parcelas
FROM condicoes_pagamentos c
LEFT JOIN condicoes_pagamentos_parcelas p ON p.condicao_pagamento_id = c.id
GROUP BY c.id, c.numero_parcelas
HAVING COUNT(p.id) <> c.numero_parcelas
    OR COUNT(DISTINCT p.numero_parcela) <> COUNT(p.id)
    OR COALESCE(MIN(p.numero_parcela), 1) <> 1
    OR COALESCE(MAX(p.numero_parcela), 0) <> c.numero_parcelas;

DROP TEMPORARY TABLE IF EXISTS tmp_prazos;
CREATE TEMPORARY TABLE tmp_prazos AS
SELECT d.condicao_pagamento_id,
       d.dias_vencimento,
       ROW_NUMBER() OVER (PARTITION BY d.condicao_pagamento_id ORDER BY d.dias_vencimento) AS numero_parcela,
       COUNT(*) OVER (PARTITION BY d.condicao_pagamento_id) AS total
FROM (SELECT DISTINCT p.condicao_pagamento_id, p.dias_vencimento
      FROM condicoes_pagamentos_parcelas p
      INNER JOIN tmp_condicoes_inconsistentes t ON t.id = p.condicao_pagamento_id
      WHERE t.numero_parcelas > 1) d;

START TRANSACTION;

DELETE p
FROM condicoes_pagamentos_parcelas p
INNER JOIN tmp_condicoes_inconsistentes t ON t.id = p.condicao_pagamento_id;

INSERT INTO condicoes_pagamentos_parcelas (condicao_pagamento_id, numero_parcela, dias_vencimento)
SELECT t.id, 1, 0
FROM tmp_condicoes_inconsistentes t
WHERE t.numero_parcelas = 1;

INSERT INTO condicoes_pagamentos_parcelas (condicao_pagamento_id, numero_parcela, dias_vencimento)
SELECT pz.condicao_pagamento_id, pz.numero_parcela, pz.dias_vencimento
FROM tmp_prazos pz
INNER JOIN tmp_condicoes_inconsistentes t ON t.id = pz.condicao_pagamento_id
WHERE pz.total = t.numero_parcelas
ORDER BY pz.condicao_pagamento_id, pz.numero_parcela;

INSERT INTO condicoes_pagamentos_parcelas (condicao_pagamento_id, numero_parcela, dias_vencimento)
WITH RECURSIVE seq (n) AS (
    SELECT 1
    UNION ALL
    SELECT n + 1 FROM seq WHERE n < 999
)
SELECT t.id, seq.n, 30 * seq.n
FROM tmp_condicoes_inconsistentes t
INNER JOIN seq ON seq.n <= t.numero_parcelas
LEFT JOIN (SELECT condicao_pagamento_id, MAX(total) AS total
           FROM tmp_prazos
           GROUP BY condicao_pagamento_id) pt ON pt.condicao_pagamento_id = t.id
WHERE t.numero_parcelas > 1
  AND COALESCE(pt.total, 0) <> t.numero_parcelas
ORDER BY t.id, seq.n;

COMMIT;

DROP TEMPORARY TABLE IF EXISTS tmp_prazos;
DROP TEMPORARY TABLE IF EXISTS tmp_condicoes_inconsistentes;

SELECT c.id,
       c.condicao_pagamento,
       c.numero_parcelas,
       GROUP_CONCAT(CONCAT(p.numero_parcela, ': ', p.dias_vencimento, ' dias')
                    ORDER BY p.numero_parcela SEPARATOR ' | ') AS parcelas
FROM condicoes_pagamentos c
LEFT JOIN condicoes_pagamentos_parcelas p ON p.condicao_pagamento_id = c.id
GROUP BY c.id, c.condicao_pagamento, c.numero_parcelas
ORDER BY c.id;
