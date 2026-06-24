/* ============================================================================
   DEVBURGUER — PAGAMENTOS DE MOTOBOY (DADOS DE DEMONSTRAÇÃO)
   ----------------------------------------------------------------------------
   Cria os pagamentos de motoboy para a tela "Faturamento de Motoboys".

   - Bate com as ENTREGAS REAIS já criadas (agrupa por motoboy e por semana).
   - O pagamento da semana atual fica datado de HOJE, então a tela aparece
     preenchida mesmo no filtro padrão "Hoje" (sem precisar clicar em "Mês").
   - Se não houver entregas finalizadas, gera pagamentos aleatórios mesmo assim.

   COMO USAR: abra no SSMS, confira o "USE" abaixo e aperte F5.
   Rode este script DEPOIS do popular_banco_demo.sql (não precisa rodar o
   principal de novo).
   ============================================================================ */

USE DevBurguerDB;   -- << troque se o seu banco tiver outro nome
SET NOCOUNT ON;

/* Limpa pagamentos antigos de demonstração e recria certo.
   (São dados de teste — se você tiver pagamentos reais que queira manter,
    comente a linha abaixo.) */
DELETE FROM PagamentoMotoboy;

/* ---------------------------------------------------------------------------
   1) PAGAMENTOS SEMANAIS baseados nas entregas reais já cadastradas
      ValorTotalEntregas = nº de entregas x R$ 6,00
      ValorChegada       = R$ 70,00 (valor padrão de chegada do sistema)
      TotalPagar         = entregas + chegada
   --------------------------------------------------------------------------- */
INSERT INTO PagamentoMotoboy
    (IdMotoboy, QuantidadeEntregas, ValorTotalEntregas, ValorChegada, TotalPagar, DataPagamento, Comentario)
SELECT
    p.IdMotoboy,
    COUNT(*)                        AS QuantidadeEntregas,
    COUNT(*) * 6.00                 AS ValorTotalEntregas,
    70.00                           AS ValorChegada,
    COUNT(*) * 6.00 + 70.00         AS TotalPagar,
    CASE WHEN DATEPART(WEEK, MAX(p.Data)) = DATEPART(WEEK, GETDATE())
              AND YEAR(MAX(p.Data))       = YEAR(GETDATE())
         THEN GETDATE()                 -- semana atual -> datado de HOJE
         ELSE MAX(p.Data) END       AS DataPagamento,
    'Pagamento semanal (demo)'      AS Comentario
FROM Pedidos p
WHERE p.TipoEntrega = 'Entrega'
  AND p.IdMotoboy IS NOT NULL
  AND p.Status = 'Finalizado'
GROUP BY p.IdMotoboy, DATEPART(WEEK, p.Data);

/* ---------------------------------------------------------------------------
   2) FALLBACK: se não havia entregas finalizadas para basear, gera ~4 semanas
      de pagamentos aleatórios para cada motoboy (uma delas datada de hoje).
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM PagamentoMotoboy)
BEGIN
    INSERT INTO PagamentoMotoboy
        (IdMotoboy, QuantidadeEntregas, ValorTotalEntregas, ValorChegada, TotalPagar, DataPagamento, Comentario)
    SELECT
        m.Id,
        q.qtd,
        q.qtd * 6.00,
        70.00,
        q.qtd * 6.00 + 70.00,
        CASE WHEN w.sem = 0 THEN GETDATE()
             ELSE DATEADD(DAY, -(w.sem * 7), CAST(GETDATE() AS DATE)) END,
        'Pagamento semanal (demo)'
    FROM Motoboys m
    CROSS JOIN (VALUES (0),(1),(2),(3)) AS w(sem)
    CROSS APPLY (SELECT 15 + ABS(CHECKSUM(NEWID())) % 26 AS qtd) AS q;  -- 15 a 40 entregas
END

/* ---------------------------------------------------------------------------
   3) RESUMO
   --------------------------------------------------------------------------- */
PRINT '====================================================';
PRINT ' PAGAMENTOS DE MOTOBOY CRIADOS';
PRINT '====================================================';

SELECT
    m.Nome                       AS Motoboy,
    COUNT(*)                     AS Pagamentos,
    SUM(pm.QuantidadeEntregas)   AS TotalEntregas,
    SUM(pm.TotalPagar)           AS TotalRecebido
FROM PagamentoMotoboy pm
JOIN Motoboys m ON m.Id = pm.IdMotoboy
GROUP BY m.Nome
ORDER BY TotalRecebido DESC;
