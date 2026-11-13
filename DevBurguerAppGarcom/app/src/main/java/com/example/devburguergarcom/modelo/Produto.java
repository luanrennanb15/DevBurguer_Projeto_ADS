package com.example.devburguergarcom.modelo;

import java.io.Serializable;

// Um item do cardápio, como vem de GET /api/produtos.
// Implementa Serializable para poder viajar de uma Activity para outra por Intent.
public class Produto implements Serializable
{
    private int id;
    private String nome;
    private double preco;
    private String categoria;
    private String descricao;

    public Produto(int id, String nome, double preco, String categoria, String descricao)
    {
        this.id = id;
        this.nome = nome;
        this.preco = preco;
        this.categoria = categoria;
        this.descricao = descricao;
    }

    public int getId()
    {
        return id;
    }

    public String getNome()
    {
        return nome;
    }

    public double getPreco()
    {
        return preco;
    }

    public String getCategoria()
    {
        return categoria;
    }

    public String getDescricao()
    {
        return descricao;
    }
}
