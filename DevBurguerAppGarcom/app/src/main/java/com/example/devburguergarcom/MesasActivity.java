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

import java.util.ArrayList;
import java.util.List;

public class MesasActivity extends AppCompatActivity
{
    public static final String EXTRA_NOME_GARCOM = "nomeGarcom";

    private TextView txvSaudacaoGarcom;
    private GridView grdMesas;
    private Button btnSair;

    @Override
    protected void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_mesas);

        configuracao();
        eventos();
    }

    private void configuracao()
    {
        txvSaudacaoGarcom = findViewById(R.id.txvSaudacaoGarcom);
        grdMesas = findViewById(R.id.grdMesas);
        btnSair = findViewById(R.id.btnSair);

        String nomeGarcom = getIntent().getStringExtra(EXTRA_NOME_GARCOM);
        txvSaudacaoGarcom.setText(getString(R.string.saudacaoGarcom, nomeGarcom));

        // MOCK: a API de mesas ainda está em desenvolvimento pelo Luan.
        // Essa lista fixa sai daqui assim que o contrato real da API existir.
        List<Mesa> mesas = new ArrayList<>();
        mesas.add(new Mesa(1, getString(R.string.statusMesaOcupada)));
        mesas.add(new Mesa(2, getString(R.string.statusMesaLivre)));
        mesas.add(new Mesa(3, getString(R.string.statusMesaLivre)));
        mesas.add(new Mesa(4, getString(R.string.statusMesaPronto)));
        mesas.add(new Mesa(5, getString(R.string.statusMesaOcupada)));
        mesas.add(new Mesa(6, getString(R.string.statusMesaOcupada)));
        mesas.add(new Mesa(7, getString(R.string.statusMesaLivre)));
        mesas.add(new Mesa(8, getString(R.string.statusMesaLivre)));
        mesas.add(new Mesa(9, getString(R.string.statusMesaOcupada)));
        mesas.add(new Mesa(10, getString(R.string.statusMesaLivre)));
        mesas.add(new Mesa(11, getString(R.string.statusMesaLivre)));
        mesas.add(new Mesa(12, getString(R.string.statusMesaLivre)));

        grdMesas.setAdapter(new AdaptadorMesa(this, mesas));
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
                    // A tela de Comanda ainda não existe (próximo passo).
                    Toast.makeText(MesasActivity.this, getString(R.string.mensagemMesaOcupadaSelecionada, mesa.getNumero()), Toast.LENGTH_SHORT).show();
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
