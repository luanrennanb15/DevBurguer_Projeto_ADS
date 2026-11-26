package com.example.devburguergarcom;

import android.content.Intent;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import android.text.method.HideReturnsTransformationMethod;
import android.text.method.PasswordTransformationMethod;
import android.widget.EditText;
import android.widget.ImageButton;
import android.widget.Toast;

import androidx.appcompat.app.AppCompatActivity;

import com.example.devburguergarcom.modelo.Garcom;
import com.example.devburguergarcom.rede.ApiDevBurguer;
import com.example.devburguergarcom.rede.OuvinteApi;

public class LoginActivity extends AppCompatActivity
{
    // Guarda o nome do garçom logado num SharedPreferences (um "cofrinho" local
    // simples do Android) em vez de só passar pelo Intent da tela de Mesas.
    // Assim, mesmo quando a Mesas é recriada do zero por outra tela (ex.: ao
    // voltar da Comanda, que usa FLAG_ACTIVITY_CLEAR_TOP), o nome continua
    // disponível pra mostrar na saudação.
    public static final String PREFS_SESSAO = "sessaoGarcom";
    public static final String PREF_NOME_GARCOM = "nomeGarcom";

    private EditText edtUsuario;
    private EditText edtSenha;
    private Button btnEntrar;
    private ImageButton btnVerSenha;

    // true = a senha está aparecendo em texto; false = aparece como bolinhas.
    private boolean senhaVisivel = false;

    private ApiDevBurguer api;

    @Override
    protected void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_login);

        configuracao();
        eventos();
    }

    private void configuracao()
    {
        edtUsuario = findViewById(R.id.edtUsuario);
        edtSenha = findViewById(R.id.edtSenha);
        btnEntrar = findViewById(R.id.btnEntrar);
        btnVerSenha = findViewById(R.id.btnVerSenha);

        api = new ApiDevBurguer(this);
    }

    // Troca entre mostrar a senha em texto e escondê-la em bolinhas. Não muda o
    // inputType do campo, só o "método de transformação" do texto, que é quem
    // decide como as letras são desenhadas na tela. O texto digitado é o mesmo.
    private void alternarVisibilidadeSenha()
    {
        // Guarda onde o cursor estava, porque trocar o método de transformação
        // joga o cursor pro começo do campo.
        int posicaoCursor = edtSenha.getSelectionEnd();

        senhaVisivel = !senhaVisivel;

        if (senhaVisivel)
        {
            edtSenha.setTransformationMethod(HideReturnsTransformationMethod.getInstance());
            btnVerSenha.setImageResource(R.drawable.ic_olho_cortado);
            btnVerSenha.setContentDescription(getString(R.string.botaoOcultarSenha));
        }
        else
        {
            edtSenha.setTransformationMethod(PasswordTransformationMethod.getInstance());
            btnVerSenha.setImageResource(R.drawable.ic_olho);
            btnVerSenha.setContentDescription(getString(R.string.botaoMostrarSenha));
        }

        if (posicaoCursor >= 0)
        {
            edtSenha.setSelection(posicaoCursor);
        }
    }

    @Override
    protected void onDestroy()
    {
        super.onDestroy();
        api.encerrar();
    }

    private void eventos()
    {
        btnVerSenha.setOnClickListener(new View.OnClickListener()
        {
            @Override
            public void onClick(View v)
            {
                alternarVisibilidadeSenha();
            }
        });

        btnEntrar.setOnClickListener(new View.OnClickListener()
        {
            @Override
            public void onClick(View v)
            {
                String usuario = edtUsuario.getText().toString();
                String senha = edtSenha.getText().toString();

                if (usuario.isEmpty() || senha.isEmpty())
                {
                    Toast.makeText(LoginActivity.this, getString(R.string.erroCamposVazios), Toast.LENGTH_SHORT).show();
                    return;
                }

                btnEntrar.setEnabled(false);
                btnEntrar.setText(getString(R.string.botaoEntrando));

                api.loginGarcom(usuario, senha, new OuvinteApi<Garcom>()
                {
                    @Override
                    public void aoSucesso(Garcom garcom)
                    {
                        // A resposta da API chega em outra thread e pode demorar; se o
                        // garçom já saiu da tela antes dela voltar, não navega de novo.
                        if (isFinishing() || isDestroyed())
                        {
                            return;
                        }

                        getSharedPreferences(PREFS_SESSAO, MODE_PRIVATE)
                                .edit()
                                .putString(PREF_NOME_GARCOM, garcom.getNome())
                                .apply();

                        Intent intent = new Intent(LoginActivity.this, MesasActivity.class);
                        startActivity(intent);
                        finish();
                    }

                    @Override
                    public void aoFalhar(String mensagemErro)
                    {
                        if (isFinishing() || isDestroyed())
                        {
                            return;
                        }

                        btnEntrar.setEnabled(true);
                        btnEntrar.setText(getString(R.string.botaoEntrar));
                        Toast.makeText(LoginActivity.this, mensagemErro, Toast.LENGTH_LONG).show();
                    }
                });
            }
        });
    }
}
