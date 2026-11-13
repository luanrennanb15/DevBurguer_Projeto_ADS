package com.example.devburguergarcom.rede;

// Callback chamado quando uma chamada à API termina (sempre na thread principal,
// já que quem recebe vai mexer na tela).
public interface OuvinteApi<T>
{
    void aoSucesso(T resultado);

    void aoFalhar(String mensagemErro);
}
