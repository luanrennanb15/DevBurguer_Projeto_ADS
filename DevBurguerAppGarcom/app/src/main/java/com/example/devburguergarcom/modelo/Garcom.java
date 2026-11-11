package com.example.devburguergarcom.modelo;

import java.io.Serializable;

// O garçom autenticado, como vem de POST /api/garcom/login (nunca traz a senha).
public class Garcom implements Serializable
{
    private int id;
    private String nome;
    private String usuario;

    public Garcom(int id, String nome, String usuario)
    {
        this.id = id;
        this.nome = nome;
        this.usuario = usuario;
    }

    public int getId()
    {
        return id;
    }

    public String getNome()
    {
        return nome;
    }

    public String getUsuario()
    {
        return usuario;
    }
}
