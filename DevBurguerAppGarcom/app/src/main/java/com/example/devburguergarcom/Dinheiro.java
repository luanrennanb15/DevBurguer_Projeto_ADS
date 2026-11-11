package com.example.devburguergarcom;

import java.util.Locale;

// Formata valores em reais com vírgula decimal (padrão brasileiro), ex.: 22,00.
public class Dinheiro
{
    public static String formatar(double valor)
    {
        return String.format(new Locale("pt", "BR"), "%.2f", valor);
    }
}
