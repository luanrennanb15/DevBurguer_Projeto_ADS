SELECT 
    p.Id,
    p.IdMotoboy AS IdMotoboy,
    m.Nome AS Motoboy,
    p.QuantidadeEntregas,
    p.ValorTotalEntregas,
    p.ValorChegada,
    p.TotalPagar,
    p.DataPagamento
FROM PagamentoMotoboy p
LEFT JOIN Motoboys m ON m.Id = p.IdMotoboy
ORDER BY p.Id DESC;
