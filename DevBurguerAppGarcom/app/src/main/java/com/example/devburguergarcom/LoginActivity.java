package com.example.devburguergarcom;

import android.content.Intent;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.Toast;

import androidx.appcompat.app.AppCompatActivity;

public class LoginActivity extends AppCompatActivity
{
    private EditText edtUsuario;
    private EditText edtSenha;
    private Button btnEntrar;

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
    }

    private void eventos()
    {
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

                // MOCK: login real do garçom ainda não existe na API (Luan está desenvolvendo).
                // Por enquanto só confirma que "entrou", sem validar usuário/senha de verdade.
                Toast.makeText(LoginActivity.this, getString(R.string.mensagemLoginMock, usuario), Toast.LENGTH_SHORT).show();

                Intent intent = new Intent(LoginActivity.this, MesasActivity.class);
                intent.putExtra(MesasActivity.EXTRA_NOME_GARCOM, usuario);
                startActivity(intent);
                finish();
            }
        });
    }
}
