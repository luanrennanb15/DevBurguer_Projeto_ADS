package com.example.devburguergarcom;

import android.content.Intent;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import android.widget.ListView;
import android.widget.TextView;

import androidx.appcompat.app.AppCompatActivity;

import com.example.devburguergarcom.modelo.ItemPedido;
import com.example.devburguergarcom.modelo.Produto;
import com.example.devburguergarcom.modelo.Rodada;

import java.util.ArrayList;
import java.util.List;

public class ComandaActivity extends AppCompatActivity
{
    public static final String EXTRA_NUMERO_MESA = "numeroMesa";
    public static final String EXTRA_NOVA_RODADA = "novaRodada";

    private TextView txvTituloComanda;
    private TextView txvSubtituloComanda;
    private ListView lstRodadas;
    private TextView txvTotalComanda;
    private Button btnAdicionarItens;

    private int numeroMesa;

    @Override
    protected void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_comanda);

        configuracao();
        eventos();
    }

    @SuppressWarnings("unchecked")
    private void configuracao()
    {
        txvTituloComanda = findViewById(R.id.txvTituloComanda);
        txvSubtituloComanda = findViewById(R.id.txvSubtituloComanda);
        lstRodadas = findViewById(R.id.lstRodadas);
        txvTotalComanda = findViewById(R.id.txvTotalComanda);
        btnAdicionarItens = findViewById(R.id.btnAdicionarItens);

        numeroMesa = getIntent().getIntExtra(EXTRA_NUMERO_MESA, 0);
        txvTituloComanda.setText(getString(R.string.tituloTelaComanda, numeroMesa));

        ArrayList<ItemPedido> novaRodada = (ArrayList<ItemPedido>) getIntent().getSerializableExtra(EXTRA_NOVA_RODADA);

        // MOCK: a API de comandas/rodadas ainda está em desenvolvimento pelo Luan.
        // A "Rodada 1" aqui é só um histórico fictício pra mostrar como a tela fica
        // com mais de uma rodada; quando a API existir, isso vem do backend de verdade.
        List<Rodada> rodadas = new ArrayList<>();
        rodadas.add(criarRodadaHistoricoMock());

        if (novaRodada != null && !novaRodada.isEmpty())
        {
            rodadas.add(new Rodada(2, Rodada.STATUS_EM_PREPARO, novaRodada));
        }
        else
        {
            rodadas.add(criarRodadaEmPreparoMock());
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

    private Rodada criarRodadaHistoricoMock()
    {
        List<ItemPedido> itens = new ArrayList<>();
        itens.add(new ItemPedido(new Produto(101, "Refrigerante Lata", 7.00, "bebidas", ""), 2));
        itens.add(new ItemPedido(new Produto(102, "Porção de Batata", 18.00, "tradicionais", ""), 1));
        return new Rodada(1, Rodada.STATUS_ENTREGUE, itens);
    }

    private Rodada criarRodadaEmPreparoMock()
    {
        List<ItemPedido> itens = new ArrayList<>();
        itens.add(new ItemPedido(new Produto(103, "X-Burguer", 22.00, "tradicionais", ""), 2));
        itens.add(new ItemPedido(new Produto(104, "X-Bacon", 26.00, "tradicionais", ""), 1));
        return new Rodada(2, Rodada.STATUS_EM_PREPARO, itens);
    }
}
