SELECT 
    p.Nome AS Produto,
    COALESCE(SUM(i.Quantidade), 0) AS Quantidade
FROM ItensPedido i
INNER JOIN Produtos p ON p.Id = i.IdProduto
INNER JOIN Pedidos ped ON ped.Id = i.IdPedido
WHERE ped.DataPedido BETWEEN @DataInicio AND @DataFim
GROUP BY p.Nome
ORDER BY Quantidade DESC;
