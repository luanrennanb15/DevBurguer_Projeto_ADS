package com.example.devburguergarcom;

import android.content.Intent;
import android.os.Bundle;
import android.util.TypedValue;
import android.view.View;
import android.widget.Button;
import android.widget.LinearLayout;
import android.widget.ListView;
import android.widget.TextView;
import android.widget.Toast;

import androidx.appcompat.app.AppCompatActivity;
import androidx.core.content.ContextCompat;

import com.example.devburguergarcom.modelo.Categoria;
import com.example.devburguergarcom.modelo.ItemPedido;
import com.example.devburguergarcom.modelo.Produto;
import com.example.devburguergarcom.rede.ApiDevBurguer;
import com.example.devburguergarcom.rede.OuvinteApi;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

public class CardapioActivity extends AppCompatActivity
{
    public static final String EXTRA_NUMERO_MESA = "numeroMesa";

    private TextView txvTituloCardapio;
    private LinearLayout lytCategorias;
    private LinearLayout lytCarregando;
    private LinearLayout lytErro;
    private TextView txvMensagemErro;
    private Button btnTentarNovamente;
    private LinearLayout lytConteudo;
    private ListView lstProdutos;
    private TextView txvRodapeItens;
    private TextView txvRodapeTotal;
    private LinearLayout painelRodapeCarrinho;

    private ApiDevBurguer api;
    private int numeroMesa;

    private List<Categoria> categorias = new ArrayList<>();
    private List<Produto> todosProdutos = new ArrayList<>();
    private final Map<Integer, Integer> quantidades = new HashMap<>();
    private String categoriaSelecionada;

    @Override
    protected void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_cardapio);

        configuracao();
        eventos();

        carregarCardapio();
    }

    private void configuracao()
    {
        txvTituloCardapio = findViewById(R.id.txvTituloCardapio);
        lytCategorias = findViewById(R.id.lytCategorias);
        lytCarregando = findViewById(R.id.lytCarregando);
        lytErro = findViewById(R.id.lytErro);
        txvMensagemErro = findViewById(R.id.txvMensagemErro);
        btnTentarNovamente = findViewById(R.id.btnTentarNovamente);
        lytConteudo = findViewById(R.id.lytConteudo);
        lstProdutos = findViewById(R.id.lstProdutos);
        txvRodapeItens = findViewById(R.id.txvRodapeItens);
        txvRodapeTotal = findViewById(R.id.txvRodapeTotal);
        painelRodapeCarrinho = findViewById(R.id.painelRodapeCarrinho);

        api = new ApiDevBurguer(this);
        numeroMesa = getIntent().getIntExtra(EXTRA_NUMERO_MESA, 0);
        txvTituloCardapio.setText(getString(R.string.tituloTelaCardapio, numeroMesa));

        atualizarRodape();
    }

    private void eventos()
    {
        findViewById(R.id.btnVoltarMesas).setOnClickListener(new View.OnClickListener()
        {
            @Override
            public void onClick(View v)
            {
                finish();
            }
        });

        btnTentarNovamente.setOnClickListener(new View.OnClickListener()
        {
            @Override
            public void onClick(View v)
            {
                carregarCardapio();
            }
        });

        painelRodapeCarrinho.setOnClickListener(new View.OnClickListener()
        {
            @Override
            public void onClick(View v)
            {
                abrirCarrinho();
            }
        });
    }

    private void carregarCardapio()
    {
        mostrarCarregando();

        api.buscarCategorias(new OuvinteApi<List<Categoria>>()
        {
            @Override
            public void aoSucesso(List<Categoria> resultado)
            {
                categorias = resultado;
                carregarProdutos();
            }

            @Override
            public void aoFalhar(String mensagemErro)
            {
                mostrarErro(mensagemErro);
            }
        });
    }

    private void carregarProdutos()
    {
        api.buscarProdutos(new OuvinteApi<List<Produto>>()
        {
            @Override
            public void aoSucesso(List<Produto> resultado)
            {
                todosProdutos = resultado;
                montarCategorias();
                mostrarConteudo();
            }

            @Override
            public void aoFalhar(String mensagemErro)
            {
                mostrarErro(mensagemErro);
            }
        });
    }

    private void montarCategorias()
    {
        if (categoriaSelecionada == null && !categorias.isEmpty())
        {
            categoriaSelecionada = categorias.get(0).getSlug();
        }

        lytCategorias.removeAllViews();

        for (Categoria categoria : categorias)
        {
            boolean selecionada = categoria.getSlug().equals(categoriaSelecionada);

            Button chip = new Button(this);
            chip.setText(categoria.getLabel());
            chip.setAllCaps(false);
            chip.setTextSize(TypedValue.COMPLEX_UNIT_SP, 15);
            chip.setMinHeight(0);
            chip.setMinimumHeight(0);
            chip.setPadding(dpParaPx(18), 0, dpParaPx(18), 0);

            if (selecionada)
            {
                chip.setBackgroundResource(R.drawable.fundo_chip_selecionado);
                chip.setTextColor(ContextCompat.getColor(this, R.color.corVermelhoPrincipal));
            }
            else
            {
                chip.setBackgroundResource(R.drawable.fundo_chip_normal);
                chip.setTextColor(ContextCompat.getColor(this, R.color.corBranca));
            }

            LinearLayout.LayoutParams parametros = new LinearLayout.LayoutParams(
                    LinearLayout.LayoutParams.WRAP_CONTENT, dpParaPx(44));
            parametros.setMarginEnd(dpParaPx(8));
            chip.setLayoutParams(parametros);

            chip.setOnClickListener(new View.OnClickListener()
            {
                @Override
                public void onClick(View v)
                {
                    categoriaSelecionada = categoria.getSlug();
                    montarCategorias();
                    atualizarListaProdutos();
                }
            });

            lytCategorias.addView(chip);
        }

        atualizarListaProdutos();
    }

    private void atualizarListaProdutos()
    {
        List<Produto> produtosDaCategoria = new ArrayList<>();

        for (Produto produto : todosProdutos)
        {
            if (produto.getCategoria().equals(categoriaSelecionada))
            {
                produtosDaCategoria.add(produto);
            }
        }

        lstProdutos.setAdapter(new AdaptadorProduto(this, produtosDaCategoria, quantidades, new AdaptadorProduto.OuvinteQuantidade()
        {
            @Override
            public void aoAlterarQuantidade(Produto produto, int novaQuantidade)
            {
                atualizarRodape();
            }
        }));
    }

    private void abrirCarrinho()
    {
        ArrayList<ItemPedido> itensPedido = new ArrayList<>();

        for (Produto produto : todosProdutos)
        {
            Integer quantidade = quantidades.get(produto.getId());
            if (quantidade != null && quantidade > 0)
            {
                itensPedido.add(new ItemPedido(produto, quantidade));
            }
        }

        if (itensPedido.isEmpty())
        {
            Toast.makeText(this, getString(R.string.mensagemPedidoVazio), Toast.LENGTH_SHORT).show();
            return;
        }

        Intent intent = new Intent(this, CarrinhoActivity.class);
        intent.putExtra(CarrinhoActivity.EXTRA_NUMERO_MESA, numeroMesa);
        intent.putExtra(CarrinhoActivity.EXTRA_ITENS_PEDIDO, itensPedido);
        startActivity(intent);
    }

    private void atualizarRodape()
    {
        int totalItens = 0;
        double totalValor = 0;

        for (Produto produto : todosProdutos)
        {
            Integer quantidade = quantidades.get(produto.getId());
            if (quantidade != null)
            {
                totalItens += quantidade;
                totalValor += quantidade * produto.getPreco();
            }
        }

        txvRodapeItens.setText(getString(R.string.rotuloVerPedido, totalItens));
        txvRodapeTotal.setText(getString(R.string.rotuloPreco, Dinheiro.formatar(totalValor)));
    }

    private void mostrarCarregando()
    {
        lytCarregando.setVisibility(View.VISIBLE);
        lytErro.setVisibility(View.GONE);
        lytConteudo.setVisibility(View.GONE);
    }

    private void mostrarErro(String mensagem)
    {
        txvMensagemErro.setText(mensagem);
        lytCarregando.setVisibility(View.GONE);
        lytErro.setVisibility(View.VISIBLE);
        lytConteudo.setVisibility(View.GONE);
    }

    private void mostrarConteudo()
    {
        lytCarregando.setVisibility(View.GONE);
        lytErro.setVisibility(View.GONE);
        lytConteudo.setVisibility(View.VISIBLE);
    }

    private int dpParaPx(int dp)
    {
        return (int) TypedValue.applyDimension(TypedValue.COMPLEX_UNIT_DIP, dp, getResources().getDisplayMetrics());
    }
}
