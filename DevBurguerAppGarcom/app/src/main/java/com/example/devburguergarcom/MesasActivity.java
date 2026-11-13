package com.example.devburguergarcom;

import android.content.DialogInterface;
import android.content.Intent;
import android.os.Bundle;
import android.view.View;
import android.widget.AdapterView;
import android.widget.Button;
import android.widget.GridView;
import android.widget.LinearLayout;
import android.widget.TextView;

import androidx.appcompat.app.AlertDialog;
import androidx.appcompat.app.AppCompatActivity;

import com.example.devburguergarcom.modelo.Mesa;
import com.example.devburguergarcom.rede.ApiDevBurguer;
import com.example.devburguergarcom.rede.OuvinteApi;

import java.util.ArrayList;
import java.util.List;

public class MesasActivity extends AppCompatActivity
{
    // O salão tem 12 mesas fixas (não vem da API). A API só diz quais estão
    // ocupadas, então o status de cada mesa é só livre/ocupada.
    private static final int TOTAL_MESAS = 12;

    private TextView txvSaudacaoGarcom;
    private GridView grdMesas;
    private Button btnSair;
    private LinearLayout lytCarregando;
    private LinearLayout lytErro;
    private TextView txvMensagemErro;
    private Button btnTentarNovamente;
    private LinearLayout lytConteudo;

    private ApiDevBurguer api;
    private List<Mesa> mesas = new ArrayList<>();

    @Override
    protected void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_mesas);

        configuracao();
        eventos();
    }

    // Recarrega sempre que a tela volta a ficar visível (ex.: voltando da
    // Comanda), pra mostrar o status atualizado das mesas.
    @Override
    protected void onResume()
    {
        super.onResume();
        carregarMesas();
    }

    private void configuracao()
    {
        txvSaudacaoGarcom = findViewById(R.id.txvSaudacaoGarcom);
        grdMesas = findViewById(R.id.grdMesas);
        btnSair = findViewById(R.id.btnSair);
        lytCarregando = findViewById(R.id.lytCarregando);
        lytErro = findViewById(R.id.lytErro);
        txvMensagemErro = findViewById(R.id.txvMensagemErro);
        btnTentarNovamente = findViewById(R.id.btnTentarNovamente);
        lytConteudo = findViewById(R.id.lytConteudo);

        api = new ApiDevBurguer(this);

        // Lê do SharedPreferences (gravado no login), não do Intent: essa tela
        // pode ser recriada do zero por outra Activity (ex.: o botão de voltar
        // da Comanda usa FLAG_ACTIVITY_CLEAR_TOP), então um Intent novo não
        // teria esse dado se ele só viesse de lá.
        String nomeGarcom = getSharedPreferences(LoginActivity.PREFS_SESSAO, MODE_PRIVATE)
                .getString(LoginActivity.PREF_NOME_GARCOM, "");
        txvSaudacaoGarcom.setText(getString(R.string.saudacaoGarcom, nomeGarcom));

        for (int numero = 1; numero <= TOTAL_MESAS; numero++)
        {
            mesas.add(new Mesa(numero, getString(R.string.statusMesaLivre)));
        }
        grdMesas.setAdapter(new AdaptadorMesa(this, mesas));
    }

    @Override
    protected void onDestroy()
    {
        super.onDestroy();
        api.encerrar();
    }

    // Busca GET /api/mesas/ocupadas e marca as mesas correspondentes. Se a API
    // falhar, mantém todas livres (igual ao app do Luan) em vez de travar a tela.
    private void carregarMesas()
    {
        mostrarCarregando();

        api.buscarMesasOcupadas(new OuvinteApi<List<Integer>>()
        {
            @Override
            public void aoSucesso(List<Integer> ocupadas)
            {
                if (isFinishing() || isDestroyed())
                {
                    return;
                }

                mesas.clear();
                for (int numero = 1; numero <= TOTAL_MESAS; numero++)
                {
                    String status = ocupadas.contains(numero)
                            ? getString(R.string.statusMesaOcupada)
                            : getString(R.string.statusMesaLivre);
                    mesas.add(new Mesa(numero, status));
                }
                grdMesas.setAdapter(new AdaptadorMesa(MesasActivity.this, mesas));
                mostrarConteudo();
            }

            @Override
            public void aoFalhar(String mensagemErro)
            {
                if (isFinishing() || isDestroyed())
                {
                    return;
                }

                mostrarErro(mensagemErro);
            }
        });
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

    private void eventos()
    {
        btnTentarNovamente.setOnClickListener(new View.OnClickListener()
        {
            @Override
            public void onClick(View v)
            {
                carregarMesas();
            }
        });

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
                new AlertDialog.Builder(MesasActivity.this)
                        .setTitle(R.string.tituloConfirmarSaida)
                        .setMessage(R.string.mensagemConfirmarSaida)
                        .setNegativeButton(R.string.botaoCancelar, null)
                        .setPositiveButton(R.string.botaoSair, new DialogInterface.OnClickListener()
                        {
                            @Override
                            public void onClick(DialogInterface dialog, int which)
                            {
                                getSharedPreferences(LoginActivity.PREFS_SESSAO, MODE_PRIVATE)
                                        .edit()
                                        .clear()
                                        .apply();

                                startActivity(new Intent(MesasActivity.this, LoginActivity.class));
                                finish();
                            }
                        })
                        .show();
            }
        });
    }
}
