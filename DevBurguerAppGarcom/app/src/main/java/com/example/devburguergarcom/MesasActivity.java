package com.example.devburguergarcom;

import android.content.Intent;
import android.os.Bundle;
import android.view.View;
import android.widget.AdapterView;
import android.widget.Button;
import android.widget.GridView;
import android.widget.TextView;
import android.widget.Toast;

import androidx.appcompat.app.AppCompatActivity;

import com.example.devburguergarcom.modelo.Mesa;
import com.example.devburguergarcom.rede.ApiDevBurguer;
import com.example.devburguergarcom.rede.OuvinteApi;

import java.util.ArrayList;
import java.util.List;

public class MesasActivity extends AppCompatActivity
{
    public static final String EXTRA_NOME_GARCOM = "nomeGarcom";

    // O salão tem 12 mesas fixas (não vem da API). A API só diz quais estão
    // ocupadas; "Pronto" ainda não existe aí, então por enquanto é só livre/ocupada.
    private static final int TOTAL_MESAS = 12;

    private TextView txvSaudacaoGarcom;
    private GridView grdMesas;
    private Button btnSair;

    private ApiDevBurguer api;
    private List<Mesa> mesas = new ArrayList<>();

    @Override
    protected void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_mesas);

        configuracao();
        eventos();
        carregarMesas();
    }

    private void configuracao()
    {
        txvSaudacaoGarcom = findViewById(R.id.txvSaudacaoGarcom);
        grdMesas = findViewById(R.id.grdMesas);
        btnSair = findViewById(R.id.btnSair);

        api = new ApiDevBurguer(this);

        String nomeGarcom = getIntent().getStringExtra(EXTRA_NOME_GARCOM);
        txvSaudacaoGarcom.setText(getString(R.string.saudacaoGarcom, nomeGarcom));

        for (int numero = 1; numero <= TOTAL_MESAS; numero++)
        {
            mesas.add(new Mesa(numero, getString(R.string.statusMesaLivre)));
        }
        grdMesas.setAdapter(new AdaptadorMesa(this, mesas));
    }

    // Busca GET /api/mesas/ocupadas e marca as mesas correspondentes. Se a API
    // falhar, mantém todas livres (igual ao app do Luan) em vez de travar a tela.
    private void carregarMesas()
    {
        api.buscarMesasOcupadas(new OuvinteApi<List<Integer>>()
        {
            @Override
            public void aoSucesso(List<Integer> ocupadas)
            {
                mesas.clear();
                for (int numero = 1; numero <= TOTAL_MESAS; numero++)
                {
                    String status = ocupadas.contains(numero)
                            ? getString(R.string.statusMesaOcupada)
                            : getString(R.string.statusMesaLivre);
                    mesas.add(new Mesa(numero, status));
                }
                grdMesas.setAdapter(new AdaptadorMesa(MesasActivity.this, mesas));
            }

            @Override
            public void aoFalhar(String mensagemErro)
            {
                Toast.makeText(MesasActivity.this, mensagemErro, Toast.LENGTH_LONG).show();
            }
        });
    }

    private void eventos()
    {
        grdMesas.setOnItemClickListener(new AdapterView.OnItemClickListener()
        {
            @Override
            public void onItemClick(AdapterView<?> listaPai, View itemClicado, int posicao, long id)
            {
                Mesa mesa = (Mesa) grdMesas.getItemAtPosition(posicao);

                if (mesa.getStatus().equals(getString(R.string.statusMesaLivre)))
                {
                    Intent intent = new Intent(MesasActivity.this, CardapioActivity.class);
                    intent.putExtra(CardapioActivity.EXTRA_NUMERO_MESA, mesa.getNumero());
                    startActivity(intent);
                }
                else
                {
                    Intent intent = new Intent(MesasActivity.this, ComandaActivity.class);
                    intent.putExtra(ComandaActivity.EXTRA_NUMERO_MESA, mesa.getNumero());
                    startActivity(intent);
                }
            }
        });

        btnSair.setOnClickListener(new View.OnClickListener()
        {
            @Override
            public void onClick(View v)
            {
                startActivity(new Intent(MesasActivity.this, LoginActivity.class));
                finish();
            }
        });
    }
}
