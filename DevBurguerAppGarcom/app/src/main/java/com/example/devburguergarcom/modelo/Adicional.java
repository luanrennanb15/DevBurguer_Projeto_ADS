package com.example.devburguergarcom.modelo;

import java.io.Serializable;

// Um adicional de lanche (ex.: Bacon, Cheddar). Ver CatalogoAdicionais pra
// entender por que a lista de adicionais é fixa aqui no app.
public class Adicional implements Serializable
{
    private final String nome;
    private final double preco;

    public Adicional(String nome, double preco)
    {
        this.nome = nome;
        this.preco = preco;
    }

    public String getNome()
    {
        return nome;
    }

    public double getPreco()
    {
        return preco;
    }
}
