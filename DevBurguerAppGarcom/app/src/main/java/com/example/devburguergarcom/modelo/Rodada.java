package com.example.devburguergarcom.modelo;

import java.io.Serializable;
import java.util.List;

// Uma rodada de itens enviada pro mesmo pedido da mesa. A API de comandas/rodadas
// ainda não existe (Luan está desenvolvendo), então por enquanto o status aqui é mock.
public class Rodada implements Serializable
{
    // Códigos internos de status (não são texto de tela — isso fica em strings.xml).
    public static final String STATUS_ENTREGUE = "ENTREGUE";
    public static final String STATUS_EM_PREPARO = "EM_PREPARO";

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
