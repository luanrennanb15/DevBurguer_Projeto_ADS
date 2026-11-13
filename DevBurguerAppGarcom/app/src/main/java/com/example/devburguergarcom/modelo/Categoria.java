package com.example.devburguergarcom.modelo;

// Uma categoria do cardápio, como vem de GET /api/categorias.
// "slug" é o valor curto usado para filtrar produtos (ex.: "tradicionais"),
// "label" é o nome para mostrar na tela (ex.: "Lanche Tradicional").
public class Categoria
{
    private String slug;
    private String label;

    public Categoria(String slug, String label)
    {
        this.slug = slug;
        this.label = label;
    }

    public String getSlug()
    {
        return slug;
    }

    public String getLabel()
    {
        return label;
    }
}
