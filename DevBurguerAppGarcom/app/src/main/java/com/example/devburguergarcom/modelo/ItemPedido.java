package com.example.devburguergarcom.modelo;

import java.io.Serializable;
import java.util.ArrayList;
import java.util.List;

// Um produto dentro do pedido que está sendo montado: além do Produto em si,
// guarda a quantidade escolhida, uma observação (ex.: "sem cebola") e os
// adicionais escolhidos (ex.: "Bacon", "Cheddar").
public class ItemPedido implements Serializable
{
    private Produto produto;
    private int quantidade;
    private String observacao;
    private List<String> adicionais;
    // Só usado quando o item vem da Comanda (GET /mesas/:mesa/comanda), pra
    // exibir os adicionais que já foram escolhidos. Não usa o campo
    // "adicionais" de cima porque o preço que vem da comanda já inclui o
    // valor dos adicionais, e getSubtotal() somaria esse valor de novo.
    private String adicionaisTexto;

    public ItemPedido(Produto produto, int quantidade)
    {
        this.produto = produto;
        this.quantidade = quantidade;
        this.observacao = "";
        this.adicionais = new ArrayList<>();
        this.adicionaisTexto = "";
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

    public List<String> getAdicionais()
    {
        return adicionais;
    }

    public void setAdicionais(List<String> adicionais)
    {
        this.adicionais = adicionais;
    }

    public String getAdicionaisTexto()
    {
        return adicionaisTexto;
    }

    public void setAdicionaisTexto(String adicionaisTexto)
    {
        this.adicionaisTexto = adicionaisTexto;
    }

    public double getSubtotal()
    {
        return (produto.getPreco() + CatalogoAdicionais.calcularValor(adicionais)) * quantidade;
    }
}
