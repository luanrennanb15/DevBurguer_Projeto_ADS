package com.example.devburguergarcom.modelo;

// Representa uma mesa do salão. O número total de mesas é fixo (12, decidido
// no app); só o status (livre/ocupada) vem de GET /api/mesas/ocupadas.
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
