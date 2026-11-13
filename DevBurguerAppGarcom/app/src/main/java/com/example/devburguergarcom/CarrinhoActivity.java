package com.example.devburguergarcom;

import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import android.widget.ListView;
import android.widget.TextView;
import android.widget.Toast;

import androidx.appcompat.app.AppCompatActivity;

import com.example.devburguergarcom.modelo.ItemPedido;
import com.example.devburguergarcom.rede.ApiDevBurguer;
import com.example.devburguergarcom.rede.OuvinteApi;

import java.util.ArrayList;

public class CarrinhoActivity extends AppCompatActivity
{
    public static final String EXTRA_NUMERO_MESA = "numeroMesa";
    public static final String EXTRA_ITENS_PEDIDO = "itensPedido";

    private TextView txvTituloCarrinho;
    private ListView lstItensCarrinho;
    private TextView txvCarrinhoVazio;
    private TextView txvCarrinhoTotalItens;
    private TextView txvCarrinhoTotalValor;
    private Button btnEnviarRodada;

    private ArrayList<ItemPedido> itens;
    private AdaptadorCarrinho adaptador;
    private ApiDevBurguer api;
    private int numeroMesa;

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
        txvCarrinhoVazio = findViewById(R.id.txvCarrinhoVazio);
        txvCarrinhoTotalItens = findViewById(R.id.txvCarrinhoTotalItens);
        txvCarrinhoTotalValor = findViewById(R.id.txvCarrinhoTotalValor);
        btnEnviarRodada = findViewById(R.id.btnEnviarRodada);

        api = new ApiDevBurguer(this);
        numeroMesa = getIntent().getIntExtra(EXTRA_NUMERO_MESA, 0);
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

        btnEnviarRodada.setText(getString(R.string.botaoEnviarRodada));

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

                btnEnviarRodada.setEnabled(false);
                btnEnviarRodada.setText(getString(R.string.botaoEnviando));

                api.enviarRodadaGarcom(numeroMesa, itens, new OuvinteApi<Void>()
                {
                    @Override
                    public void aoSucesso(Void resultado)
                    {
                        // Quem abriu o Carrinho (Cardápio) decide pra onde ir: ele se
                        // fecha em cadeia até voltar pra tela de Mesas, que então abre
                        // a Comanda. Assim não fica nenhuma tela antiga pra trás.
                        setResult(RESULT_OK);
                        finish();
                    }

                    @Override
                    public void aoFalhar(String mensagemErro)
                    {
                        btnEnviarRodada.setEnabled(true);
                        btnEnviarRodada.setText(getString(R.string.botaoEnviarRodada));
                        Toast.makeText(CarrinhoActivity.this, mensagemErro, Toast.LENGTH_LONG).show();
                    }
                });
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

        boolean carrinhoVazio = itens.isEmpty();
        txvCarrinhoVazio.setVisibility(carrinhoVazio ? View.VISIBLE : View.GONE);
        lstItensCarrinho.setVisibility(carrinhoVazio ? View.GONE : View.VISIBLE);
        btnEnviarRodada.setEnabled(!carrinhoVazio);
    }
}
