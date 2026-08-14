UPDATE PagamentoMotoboy SET
    QuantidadeEntregas=@q,
    ValorTotalEntregas=@v,
    ValorChegada=@c,
    TotalPagar=@t,
    DataPagamento=@d
WHERE Id=@id
