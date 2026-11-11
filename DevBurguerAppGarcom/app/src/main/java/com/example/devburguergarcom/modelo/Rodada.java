package com.example.devburguergarcom.modelo;

import java.io.Serializable;
import java.util.List;

// Uma rodada de itens de uma mesa: cada rodada é um idPedido que o GET
// /api/mesas/:mesa/comanda devolve. O status é o texto exato do banco
// (ex.: "Em Producao"), sem tradução — só decide a cor do selo na tela.
public class Rodada implements Serializable
{
    private int numero;
    private String status;
    private List<ItemPedido> itens;

    public Rodada(int numero, String status, List<ItemPedido> itens)
    {
        this.numero = numero;
        this.status = status;
        this.itens = itens;
    }

    public int getNumero()
    {
        return numero;
    }

    public String getStatus()
    {
        return status;
    }

    public List<ItemPedido> getItens()
    {
        return itens;
    }

    public boolean isEntregue()
    {
        return "Entregue".equalsIgnoreCase(status) || "Finalizado".equalsIgnoreCase(status);
    }

    public double getSubtotal()
    {
        double subtotal = 0;
        for (ItemPedido item : itens)
        {
            subtotal += item.getSubtotal();
        }
        return subtotal;
    }

    public int getTotalItens()
    {
        int total = 0;
        for (ItemPedido item : itens)
        {
            total += item.getQuantidade();
        }
        return total;
    }
}
