package com.example.devburguergarcom.rede;

import android.content.Context;
import android.os.Handler;
import android.os.Looper;

import com.example.devburguergarcom.R;
import com.example.devburguergarcom.modelo.Categoria;
import com.example.devburguergarcom.modelo.Produto;

import org.json.JSONArray;
import org.json.JSONObject;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.net.HttpURLConnection;
import java.net.SocketTimeoutException;
import java.net.URL;
import java.nio.charset.StandardCharsets;
import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;

// Classe única para falar com a API do DevBurguer (GET /api/produtos e /api/categorias).
// O plano gratuito do Render pode demorar até ~50s pra "acordar" na primeira chamada,
// por isso o timeout é generoso (60s). Toda chamada roda em outra thread e o resultado
// volta pra thread principal, porque a tela não pode travar esperando a rede.
public class ApiDevBurguer
{
    // MODO DE TESTE: a API real está fora do ar (erro 500). Com isso em "true",
    // as buscas abaixo devolvem dados fictícios (mock) em vez de chamar a internet,
    // só pra testar as telas. Quando a API do Luan voltar a funcionar, troque para
    // "false" (ou apague este bloco) pra voltar a usar a API de verdade.
    private static final boolean MODO_TESTE_OFFLINE = true;

    private static final String URL_BASE = "https://devburguer-api-9gds.onrender.com/api";
    private static final int TEMPO_LIMITE_MS = 60000;

    private final Context contexto;
    private final ExecutorService executor = Executors.newSingleThreadExecutor();
    private final Handler handlerTelaPrincipal = new Handler(Looper.getMainLooper());

    public ApiDevBurguer(Context contexto)
    {
        this.contexto = contexto.getApplicationContext();
    }

    public void buscarProdutos(OuvinteApi<List<Produto>> ouvinte)
    {
        if (MODO_TESTE_OFFLINE)
        {
            avisarSucesso(ouvinte, criarProdutosMock());
            return;
        }

        executor.execute(new Runnable()
        {
            @Override
            public void run()
            {
                try
                {
                    String json = fazerRequisicao("/produtos");
                    List<Produto> produtos = lerProdutos(json);
                    avisarSucesso(ouvinte, produtos);
                }
                catch (ErroApi erro)
                {
                    avisarFalha(ouvinte, erro.mensagem);
                }
            }
        });
    }

    public void buscarCategorias(OuvinteApi<List<Categoria>> ouvinte)
    {
        if (MODO_TESTE_OFFLINE)
        {
            avisarSucesso(ouvinte, criarCategoriasMock());
            return;
        }

        executor.execute(new Runnable()
        {
            @Override
            public void run()
            {
                try
                {
                    String json = fazerRequisicao("/categorias");
                    List<Categoria> categorias = lerCategorias(json);
                    avisarSucesso(ouvinte, categorias);
                }
                catch (ErroApi erro)
                {
                    avisarFalha(ouvinte, erro.mensagem);
                }
            }
        });
    }

    // Dados fictícios só pro MODO_TESTE_OFFLINE, com os mesmos campos da API real.
    private List<Categoria> criarCategoriasMock()
    {
        List<Categoria> categorias = new ArrayList<>();
        categorias.add(new Categoria("tradicionais", "Lanche Tradicional"));
        categorias.add(new Categoria("combos", "Combo"));
        categorias.add(new Categoria("bebidas", "Bebidas"));
        return categorias;
    }

    private List<Produto> criarProdutosMock()
    {
        List<Produto> produtos = new ArrayList<>();
        produtos.add(new Produto(1, "X-Burguer", 22.00, "tradicionais", "Pão, hambúrguer, queijo e salada"));
        produtos.add(new Produto(2, "X-Bacon", 26.00, "tradicionais", "Hambúrguer, queijo e bacon crocante"));
        produtos.add(new Produto(3, "Frango Crispy", 28.00, "tradicionais", "Frango empanado, alface e maionese"));
        produtos.add(new Produto(4, "Burguer Duplo", 32.00, "combos", "Dois hambúrgueres, queijo e molho da casa"));
        produtos.add(new Produto(5, "Combo X-Bacon + Fritas", 36.00, "combos", "X-Bacon, porção de batata e refrigerante"));
        produtos.add(new Produto(6, "Refrigerante Lata", 7.00, "bebidas", "350ml"));
        produtos.add(new Produto(7, "Suco Natural", 9.00, "bebidas", "500ml, sabor da casa"));
        return produtos;
    }

    private String fazerRequisicao(String caminho) throws ErroApi
    {
        HttpURLConnection conexao = null;

        try
        {
            URL url = new URL(URL_BASE + caminho);
            conexao = (HttpURLConnection) url.openConnection();
            conexao.setRequestMethod("GET");
            conexao.setConnectTimeout(TEMPO_LIMITE_MS);
            conexao.setReadTimeout(TEMPO_LIMITE_MS);

            int codigo = conexao.getResponseCode();

            if (codigo != 200)
            {
                throw new ErroApi(contexto.getString(R.string.erroCardapioServidor));
            }

            InputStream entrada = conexao.getInputStream();
            return lerConteudo(entrada);
        }
        catch (SocketTimeoutException e)
        {
            throw new ErroApi(contexto.getString(R.string.erroCardapioTimeout));
        }
        catch (IOException e)
        {
            throw new ErroApi(contexto.getString(R.string.erroCardapioConexao));
        }
        catch (ErroApi e)
        {
            throw e;
        }
        catch (Exception e)
        {
            throw new ErroApi(contexto.getString(R.string.erroCardapioInesperado));
        }
        finally
        {
            if (conexao != null)
            {
                conexao.disconnect();
            }
        }
    }

    private String lerConteudo(InputStream entrada) throws IOException
    {
        StringBuilder texto = new StringBuilder();
        BufferedReader leitor = new BufferedReader(new InputStreamReader(entrada, StandardCharsets.UTF_8));
        String linha;

        while ((linha = leitor.readLine()) != null)
        {
            texto.append(linha);
        }

        leitor.close();
        return texto.toString();
    }

    private List<Produto> lerProdutos(String json) throws ErroApi
    {
        try
        {
            JSONArray array = new JSONArray(json);
            List<Produto> produtos = new ArrayList<>();

            for (int i = 0; i < array.length(); i++)
            {
                JSONObject item = array.getJSONObject(i);
                produtos.add(new Produto(
                        item.getInt("id"),
                        item.getString("nome"),
                        item.getDouble("preco"),
                        item.getString("categoria"),
                        item.optString("descricao", "")
                ));
            }

            return produtos;
        }
        catch (Exception e)
        {
            throw new ErroApi(contexto.getString(R.string.erroCardapioInesperado));
        }
    }

    private List<Categoria> lerCategorias(String json) throws ErroApi
    {
        try
        {
            JSONArray array = new JSONArray(json);
            List<Categoria> categorias = new ArrayList<>();

            for (int i = 0; i < array.length(); i++)
            {
                JSONObject item = array.getJSONObject(i);
                categorias.add(new Categoria(item.getString("slug"), item.getString("label")));
            }

            return categorias;
        }
        catch (Exception e)
        {
            throw new ErroApi(contexto.getString(R.string.erroCardapioInesperado));
        }
    }

    private <T> void avisarSucesso(OuvinteApi<T> ouvinte, T resultado)
    {
        handlerTelaPrincipal.post(new Runnable()
        {
            @Override
            public void run()
            {
                ouvinte.aoSucesso(resultado);
            }
        });
    }

    private void avisarFalha(OuvinteApi<?> ouvinte, String mensagem)
    {
        handlerTelaPrincipal.post(new Runnable()
        {
            @Override
            public void run()
            {
                ouvinte.aoFalhar(mensagem);
            }
        });
    }

    // Erro interno só pra carregar a mensagem já traduzida até o ponto de aviso.
    private static class ErroApi extends Exception
    {
        final String mensagem;

        ErroApi(String mensagem)
        {
            this.mensagem = mensagem;
        }
    }
}
