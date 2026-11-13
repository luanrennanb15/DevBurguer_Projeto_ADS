package com.example.devburguergarcom;

import android.content.Intent;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import android.widget.ListView;
import android.widget.TextView;
import android.widget.Toast;

import androidx.appcompat.app.AppCompatActivity;

import com.example.devburguergarcom.modelo.Rodada;
import com.example.devburguergarcom.rede.ApiDevBurguer;
import com.example.devburguergarcom.rede.OuvinteApi;

import java.util.List;

public class ComandaActivity extends AppCompatActivity
{
    public static final String EXTRA_NUMERO_MESA = "numeroMesa";

    private TextView txvTituloComanda;
    private TextView txvSubtituloComanda;
    private ListView lstRodadas;
    private TextView txvTotalComanda;
    private Button btnAdicionarItens;

    private ApiDevBurguer api;
    private int numeroMesa;

    @Override
    protected void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_comanda);

        configuracao();
        eventos();
        carregarComanda();
    }

    private void configuracao()
    {
        txvTituloComanda = findViewById(R.id.txvTituloComanda);
        txvSubtituloComanda = findViewById(R.id.txvSubtituloComanda);
        lstRodadas = findViewById(R.id.lstRodadas);
        txvTotalComanda = findViewById(R.id.txvTotalComanda);
        btnAdicionarItens = findViewById(R.id.btnAdicionarItens);

        api = new ApiDevBurguer(this);
        numeroMesa = getIntent().getIntExtra(EXTRA_NUMERO_MESA, 0);
        txvTituloComanda.setText(getString(R.string.tituloTelaComanda, numeroMesa));
    }

    // Busca a comanda de verdade em GET /api/mesas/:mesa/comanda.
    private void carregarComanda()
    {
        txvSubtituloComanda.setText(getString(R.string.carregandoComanda));

        api.buscarComanda(numeroMesa, new OuvinteApi<List<Rodada>>()
        {
            @Override
            public void aoSucesso(List<Rodada> rodadas)
            {
                mostrarRodadas(rodadas);
            }

            @Override
            public void aoFalhar(String mensagemErro)
            {
                txvSubtituloComanda.setText(mensagemErro);
                Toast.makeText(ComandaActivity.this, mensagemErro, Toast.LENGTH_LONG).show();
            }
        });
    }

    // Se a mesa foi liberada (pagou e fechou no caixa) entre o garçom tocar
    // nela e a comanda carregar, não tem rodada nenhuma pra mostrar. Sem esse
    // aviso, a tela ficava zerada do mesmo jeito que num bug de carregamento.
    private void mostrarRodadas(List<Rodada> rodadas)
    {
        if (rodadas.isEmpty())
        {
            txvSubtituloComanda.setText(R.string.avisoMesaLiberada);
            txvTotalComanda.setText(getString(R.string.rotuloPreco, Dinheiro.formatar(0)));
            lstRodadas.setAdapter(new AdaptadorRodada(this, rodadas));
            return;
        }

        int totalItens = 0;
        double totalValor = 0;
        for (Rodada rodada : rodadas)
        {
            totalItens += rodada.getTotalItens();
            totalValor += rodada.getSubtotal();
        }

        txvSubtituloComanda.setText(getString(R.string.subtituloComanda, rodadas.size(), totalItens));
        txvTotalComanda.setText(getString(R.string.rotuloPreco, Dinheiro.formatar(totalValor)));

        lstRodadas.setAdapter(new AdaptadorRodada(this, rodadas));
    }

    private void eventos()
    {
        findViewById(R.id.btnVoltarMesasComanda).setOnClickListener(new View.OnClickListener()
        {
            @Override
            public void onClick(View v)
            {
                // Vai direto pras Mesas e limpa Cardápio/Carrinho que tenham
                // ficado atrás na pilha (senão a seta de voltar cairia numa
                // tela antiga, ainda com os itens da rodada que já foi enviada).
                Intent intent = new Intent(ComandaActivity.this, MesasActivity.class);
                intent.addFlags(Intent.FLAG_ACTIVITY_CLEAR_TOP);
                startActivity(intent);
                finish();
            }
        });

        btnAdicionarItens.setOnClickListener(new View.OnClickListener()
        {
            @Override
            public void onClick(View v)
            {
                Intent intent = new Intent(ComandaActivity.this, CardapioActivity.class);
                intent.putExtra(CardapioActivity.EXTRA_NUMERO_MESA, numeroMesa);
                startActivity(intent);
            }
        });
    }
}
