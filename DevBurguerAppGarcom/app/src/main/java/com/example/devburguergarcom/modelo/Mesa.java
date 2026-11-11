package com.example.devburguergarcom.modelo;

// Representa uma mesa do salão. Por enquanto só com dados mock (a API de mesas
// ainda está em desenvolvimento pelo Luan); quando ela existir, só precisamos
// trocar quem cria os objetos Mesa, essa classe continua igual.
public class Mesa
{
    private int numero;
    private String status;

    public Mesa(int numero, String status)
    {
        this.numero = numero;
        this.status = status;
    }

    public int getNumero()
    {
        return numero;
    }

    public String getStatus()
    {
        return status;
    }
}
