package com.example.devburguergarcom.modelo;

import java.io.Serializable;

// Um produto dentro do pedido que está sendo montado: além do Produto em si,
// guarda a quantidade escolhida e uma observação (ex.: "sem cebola").
public class ItemPedido implements Serializable
{
    private Produto produto;
    private int quantidade;
    private String observacao;

    public ItemPedido(Produto produto, int quantidade)
    {
        this.produto = produto;
        this.quantidade = quantidade;
        this.observacao = "";
    }

    public Produto getProduto()
    {
        return produto;
    }

    public int getQuantidade()
    {
        return quantidade;
    }

    public void setQuantidade(int quantidade)
    {
        this.quantidade = quantidade;
    }

    public String getObservacao()
    {
        return observacao;
    }

    public void setObservacao(String observacao)
    {
        this.observacao = observacao;
    }

    public double getSubtotal()
    {
        return produto.getPreco() * quantidade;
    }
}
