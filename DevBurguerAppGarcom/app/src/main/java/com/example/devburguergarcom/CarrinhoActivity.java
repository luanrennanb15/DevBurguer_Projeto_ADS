package com.example.devburguergarcom;

import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import android.widget.ListView;
import android.widget.TextView;
import android.widget.Toast;

import androidx.appcompat.app.AppCompatActivity;

import com.example.devburguergarcom.modelo.ItemPedido;

import java.util.ArrayList;

public class CarrinhoActivity extends AppCompatActivity
{
    public static final String EXTRA_NUMERO_MESA = "numeroMesa";
    public static final String EXTRA_ITENS_PEDIDO = "itensPedido";

    // Mock: ainda não existe uma Comanda real pra saber qual é o número da rodada.
    private static final int NUMERO_RODADA_MOCK = 1;

    private TextView txvTituloCarrinho;
    private ListView lstItensCarrinho;
    private TextView txvCarrinhoTotalItens;
    private TextView txvCarrinhoTotalValor;
    private Button btnEnviarRodada;

    private ArrayList<ItemPedido> itens;
    private AdaptadorCarrinho adaptador;

    @Override
    protected void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_carrinho);

        configuracao();
        eventos();
    }

    @SuppressWarnings("unchecked")
    private void configuracao()
    {
        txvTituloCarrinho = findViewById(R.id.txvTituloCarrinho);
        lstItensCarrinho = findViewById(R.id.lstItensCarrinho);
        txvCarrinhoTotalItens = findViewById(R.id.txvCarrinhoTotalItens);
        txvCarrinhoTotalValor = findViewById(R.id.txvCarrinhoTotalValor);
        btnEnviarRodada = findViewById(R.id.btnEnviarRodada);

        int numeroMesa = getIntent().getIntExtra(EXTRA_NUMERO_MESA, 0);
        txvTituloCarrinho.setText(getString(R.string.tituloTelaCarrinho, numeroMesa));

        itens = (ArrayList<ItemPedido>) getIntent().getSerializableExtra(EXTRA_ITENS_PEDIDO);
        if (itens == null)
        {
            itens = new ArrayList<>();
        }

        adaptador = new AdaptadorCarrinho(this, itens, new AdaptadorCarrinho.OuvinteCarrinho()
        {
            @Override
            public void aoAlterarCarrinho()
            {
                atualizarRodape();
            }
        });
        lstItensCarrinho.setAdapter(adaptador);

        btnEnviarRodada.setText(getString(R.string.botaoEnviarRodada, NUMERO_RODADA_MOCK));

        atualizarRodape();
    }

    private void eventos()
    {
        findViewById(R.id.btnVoltarCardapio).setOnClickListener(new View.OnClickListener()
        {
            @Override
            public void onClick(View v)
            {
                finish();
            }
        });

        btnEnviarRodada.setOnClickListener(new View.OnClickListener()
        {
            @Override
            public void onClick(View v)
            {
                if (itens.isEmpty())
                {
                    Toast.makeText(CarrinhoActivity.this, getString(R.string.mensagemPedidoVazio), Toast.LENGTH_SHORT).show();
                    return;
                }

                // A tela de Comanda ainda não existe (próximo passo) e a API de
                // rodadas também não — por enquanto é só um aviso mock.
                Toast.makeText(CarrinhoActivity.this, getString(R.string.mensagemRodadaEnviada), Toast.LENGTH_SHORT).show();
            }
        });
    }

    private void atualizarRodape()
    {
        int totalItens = 0;
        double totalValor = 0;

        for (ItemPedido item : itens)
        {
            totalItens += item.getQuantidade();
            totalValor += item.getSubtotal();
        }

        txvCarrinhoTotalItens.setText(getString(R.string.rotuloTotalItens, totalItens));
        txvCarrinhoTotalValor.setText(getString(R.string.rotuloPreco, Dinheiro.formatar(totalValor)));
    }
}
