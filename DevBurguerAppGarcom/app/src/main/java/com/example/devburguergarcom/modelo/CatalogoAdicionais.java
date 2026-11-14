package com.example.devburguergarcom.modelo;

import java.util.Arrays;
import java.util.HashSet;
import java.util.List;
import java.util.Set;

// Lista fixa dos adicionais (nome e preço), igual à que o site usa em
// docs/js/cart.js e à tabela "adicionais" do banco (database/seeds/seed_adicionais.sql).
// A API não tem endpoint pra listar adicionais (o site também não usa um),
// então o app guarda essa mesma lista fixa. Se o preço de algum adicional
// mudar no banco, é preciso atualizar aqui também.
public class CatalogoAdicionais
{
    private static final List<Adicional> TODOS = Arrays.asList(
            new Adicional("Bacon", 5.00),
            new Adicional("Cheddar", 3.00),
            new Adicional("Ovo", 2.00),
            new Adicional("Hamburguer", 7.00),
            new Adicional("Mussarela", 3.00),
            new Adicional("Presunto", 3.00),
            new Adicional("Alface", 2.00),
            new Adicional("Milho", 3.00),
            new Adicional("Ervilha", 3.00),
            new Adicional("Frango Desfiado", 5.00),
            new Adicional("Catupiry", 3.00),
            new Adicional("Calabresa", 5.00),
            new Adicional("Contra Filé", 5.00),
            new Adicional("Molho da Casa", 2.00),
            new Adicional("Blend 180g", 9.00),
            new Adicional("Onion Rings", 5.00),
            new Adicional("Costela Desfiada", 8.00),
            new Adicional("Cebola Caramelizada", 8.00),
            new Adicional("Barbecue", 1.00)
    );

    // Categorias (mesmos slugs de GET /api/produtos) que não aceitam adicionais
    // de lanche — igual à regra do site (CATEGORIAS_SEM_ADICIONAIS em cart.js).
    private static final Set<String> CATEGORIAS_SEM_ADICIONAIS =
            new HashSet<>(Arrays.asList("bebidas", "sucos", "alcoolicas", "milkshakes"));

    public static boolean aceitaAdicionais(String categoria)
    {
        return categoria != null && !CATEGORIAS_SEM_ADICIONAIS.contains(categoria.toLowerCase());
    }

    public static List<Adicional> listarTodos()
    {
        return TODOS;
    }

    // Soma o preço dos adicionais escolhidos (pelos nomes), pra somar no preço
    // unitário do item — igual à conta que a API faz no servidor.
    public static double calcularValor(List<String> nomesEscolhidos)
    {
        double total = 0;
        for (Adicional adicional : TODOS)
        {
            if (nomesEscolhidos.contains(adicional.getNome()))
            {
                total += adicional.getPreco();
            }
        }
        return total;
    }
}
