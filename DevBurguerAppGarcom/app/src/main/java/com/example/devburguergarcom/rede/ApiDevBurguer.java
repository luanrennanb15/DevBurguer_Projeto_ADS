package com.example.devburguergarcom.rede;

import android.content.Context;
import android.os.Handler;
import android.os.Looper;

import com.example.devburguergarcom.R;
import com.example.devburguergarcom.modelo.Categoria;
import com.example.devburguergarcom.modelo.Garcom;
import com.example.devburguergarcom.modelo.ItemPedido;
import com.example.devburguergarcom.modelo.Produto;
import com.example.devburguergarcom.modelo.Rodada;

import org.json.JSONArray;
import org.json.JSONObject;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.io.OutputStream;
import java.net.HttpURLConnection;
import java.net.SocketTimeoutException;
import java.net.URL;
import java.nio.charset.StandardCharsets;
import java.util.ArrayList;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;

// Classe única para falar com a API do DevBurguer (GET /api/produtos e /api/categorias).
// O plano gratuito do Render pode demorar até ~50s pra "acordar" na primeira chamada,
// por isso o timeout é generoso (60s). Toda chamada roda em outra thread e o resultado
// volta pra thread principal, porque a tela não pode travar esperando a rede.
public class ApiDevBurguer
{
    // MODO DE TESTE: deixe "true" só se quiser testar as telas sem depender da
    // internet. Com "false", chama a API de verdade (GET /produtos e /categorias).
    // Aviso: em 11/11 o /api/produtos ainda estava devolvendo erro 500 no Render;
    // se isso ainda não foi corrigido, o Cardápio vai mostrar a tela de erro.
    private static final boolean MODO_TESTE_OFFLINE = false;

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

    // POST /api/garcom/login -> autentica o garçom (usuario/senha em texto puro;
    // o hash SHA-256 é feito pelo servidor, do nosso lado não precisa).
    public void loginGarcom(String usuario, String senha, OuvinteApi<Garcom> ouvinte)
    {
        executor.execute(new Runnable()
        {
            @Override
            public void run()
            {
                try
                {
                    JSONObject corpoEnvio = new JSONObject();
                    corpoEnvio.put("usuario", usuario);
                    corpoEnvio.put("senha", senha);

                    RespostaHttp resposta = chamar("POST", "/garcom/login", corpoEnvio.toString());
                    JSONObject corpoResposta = new JSONObject(resposta.corpo.isEmpty() ? "{}" : resposta.corpo);

                    if (resposta.codigo == 200)
                    {
                        Garcom garcom = new Garcom(
                                corpoResposta.getInt("id"),
                                corpoResposta.getString("nome"),
                                corpoResposta.getString("usuario"));
                        avisarSucesso(ouvinte, garcom);
                    }
                    else
                    {
                        avisarFalha(ouvinte, extrairMensagemErro(corpoResposta));
                    }
                }
                catch (ErroApi erro)
                {
                    avisarFalha(ouvinte, erro.mensagem);
                }
                catch (Exception e)
                {
                    avisarFalha(ouvinte, contexto.getString(R.string.erroInesperado));
                }
            }
        });
    }

    // POST /api/pedidos/garcom -> registra a rodada da mesa (sem cliente nem
    // pagamento: isso é sempre no caixa). Adicionais ainda não têm tela própria,
    // então vai sempre vazio por enquanto.
    public void enviarRodadaGarcom(int mesa, List<ItemPedido> itens, OuvinteApi<Void> ouvinte)
    {
        executor.execute(new Runnable()
        {
            @Override
            public void run()
            {
                try
                {
                    JSONArray itensJson = new JSONArray();
                    for (ItemPedido item : itens)
                    {
                        JSONObject itemJson = new JSONObject();
                        itemJson.put("idProduto", item.getProduto().getId());
                        itemJson.put("quantidade", item.getQuantidade());
                        itemJson.put("observacao", item.getObservacao());
                        itemJson.put("adicionais", "");
                        itensJson.put(itemJson);
                    }

                    JSONObject corpoEnvio = new JSONObject();
                    corpoEnvio.put("mesa", mesa);
                    corpoEnvio.put("itens", itensJson);

                    RespostaHttp resposta = chamar("POST", "/pedidos/garcom", corpoEnvio.toString());

                    if (resposta.codigo == 201)
                    {
                        avisarSucesso(ouvinte, null);
                    }
                    else
                    {
                        JSONObject corpoResposta = new JSONObject(resposta.corpo.isEmpty() ? "{}" : resposta.corpo);
                        avisarFalha(ouvinte, extrairMensagemErro(corpoResposta));
                    }
                }
                catch (ErroApi erro)
                {
                    avisarFalha(ouvinte, erro.mensagem);
                }
                catch (Exception e)
                {
                    avisarFalha(ouvinte, contexto.getString(R.string.erroInesperado));
                }
            }
        });
    }

    // GET /api/mesas/ocupadas -> números das mesas ocupadas (comanda em aberto).
    // A API ainda não distingue "pronto": por enquanto é só livre/ocupada.
    public void buscarMesasOcupadas(OuvinteApi<List<Integer>> ouvinte)
    {
        executor.execute(new Runnable()
        {
            @Override
            public void run()
            {
                try
                {
                    RespostaHttp resposta = chamar("GET", "/mesas/ocupadas", null);
                    JSONObject corpoResposta = new JSONObject(resposta.corpo.isEmpty() ? "{}" : resposta.corpo);

                    if (resposta.codigo == 200)
                    {
                        JSONArray ocupadasJson = corpoResposta.optJSONArray("ocupadas");
                        List<Integer> ocupadas = new ArrayList<>();
                        if (ocupadasJson != null)
                        {
                            for (int i = 0; i < ocupadasJson.length(); i++)
                            {
                                ocupadas.add(ocupadasJson.getInt(i));
                            }
                        }
                        avisarSucesso(ouvinte, ocupadas);
                    }
                    else
                    {
                        avisarFalha(ouvinte, extrairMensagemErro(corpoResposta));
                    }
                }
                catch (ErroApi erro)
                {
                    avisarFalha(ouvinte, erro.mensagem);
                }
                catch (Exception e)
                {
                    avisarFalha(ouvinte, contexto.getString(R.string.erroInesperado));
                }
            }
        });
    }

    // GET /api/mesas/:mesa/comanda -> itens em aberto da mesa, agrupados por
    // rodada (cada rodada é um idPedido). O status mostrado é o texto exato do
    // banco (ex.: "Em Producao"), sem tradução.
    public void buscarComanda(int mesa, OuvinteApi<List<Rodada>> ouvinte)
    {
        executor.execute(new Runnable()
        {
            @Override
            public void run()
            {
                try
                {
                    RespostaHttp resposta = chamar("GET", "/mesas/" + mesa + "/comanda", null);
                    JSONObject corpoResposta = new JSONObject(resposta.corpo.isEmpty() ? "{}" : resposta.corpo);

                    if (resposta.codigo == 200)
                    {
                        avisarSucesso(ouvinte, lerComanda(corpoResposta));
                    }
                    else
                    {
                        avisarFalha(ouvinte, extrairMensagemErro(corpoResposta));
                    }
                }
                catch (ErroApi erro)
                {
                    avisarFalha(ouvinte, erro.mensagem);
                }
                catch (Exception e)
                {
                    avisarFalha(ouvinte, contexto.getString(R.string.erroInesperado));
                }
            }
        });
    }

    // Agrupa a lista plana de itens da comanda pelo idPedido: cada idPedido
    // virou uma rodada quando o garçom apertou "Enviar rodada".
    private List<Rodada> lerComanda(JSONObject corpoResposta) throws Exception
    {
        List<Rodada> rodadas = new ArrayList<>();
        JSONArray itensJson = corpoResposta.optJSONArray("itens");
        if (itensJson == null)
        {
            return rodadas;
        }

        Map<Integer, List<ItemPedido>> itensPorPedido = new LinkedHashMap<>();
        Map<Integer, String> statusPorPedido = new LinkedHashMap<>();

        for (int i = 0; i < itensJson.length(); i++)
        {
            JSONObject item = itensJson.getJSONObject(i);
            int idPedido = item.getInt("idPedido");

            // A comanda não devolve o id do produto, só o nome: não precisamos
            // dele aqui, é só pra exibir a linha da rodada.
            Produto produto = new Produto(0, item.getString("produto"), item.getDouble("preco"), "", "");

            ItemPedido itemPedido = new ItemPedido(produto, item.getInt("quantidade"));
            itemPedido.setObservacao(item.optString("observacao", ""));

            List<ItemPedido> itensDaRodada = itensPorPedido.get(idPedido);
            if (itensDaRodada == null)
            {
                itensDaRodada = new ArrayList<>();
                itensPorPedido.put(idPedido, itensDaRodada);
            }
            itensDaRodada.add(itemPedido);

            statusPorPedido.put(idPedido, item.getString("status"));
        }

        for (Integer idPedido : itensPorPedido.keySet())
        {
            rodadas.add(new Rodada(idPedido, statusPorPedido.get(idPedido), itensPorPedido.get(idPedido)));
        }

        return rodadas;
    }

    // Lê o campo "erro" que a API sempre devolve quando dá 400/401/403/500.
    private String extrairMensagemErro(JSONObject corpoResposta)
    {
        String erro = corpoResposta.optString("erro", "");
        return erro.isEmpty() ? contexto.getString(R.string.erroInesperado) : erro;
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
        RespostaHttp resposta = chamar("GET", caminho, null);

        if (resposta.codigo != 200)
        {
            throw new ErroApi(contexto.getString(R.string.erroCardapioServidor));
        }

        return resposta.corpo;
    }

    // Chamada HTTP genérica (GET ou POST). Devolve o código e o corpo mesmo
    // quando a API responde erro (400/401/403/500) — quem chamou decide o que
    // fazer com isso. Só lança ErroApi pra falha de rede/timeout de verdade.
    private RespostaHttp chamar(String metodo, String caminho, String corpoJson) throws ErroApi
    {
        HttpURLConnection conexao = null;

        try
        {
            URL url = new URL(URL_BASE + caminho);
            conexao = (HttpURLConnection) url.openConnection();
            conexao.setRequestMethod(metodo);
            conexao.setConnectTimeout(TEMPO_LIMITE_MS);
            conexao.setReadTimeout(TEMPO_LIMITE_MS);

            if (corpoJson != null)
            {
                conexao.setDoOutput(true);
                conexao.setRequestProperty("Content-Type", "application/json; charset=utf-8");
                byte[] bytes = corpoJson.getBytes(StandardCharsets.UTF_8);
                OutputStream saida = conexao.getOutputStream();
                saida.write(bytes);
                saida.close();
            }

            int codigo = conexao.getResponseCode();
            InputStream entrada = (codigo >= 200 && codigo < 300) ? conexao.getInputStream() : conexao.getErrorStream();
            String corpo = entrada != null ? lerConteudo(entrada) : "";

            return new RespostaHttp(codigo, corpo);
        }
        catch (SocketTimeoutException e)
        {
            throw new ErroApi(contexto.getString(R.string.erroCardapioTimeout));
        }
        catch (IOException e)
        {
            throw new ErroApi(contexto.getString(R.string.erroCardapioConexao));
        }
        finally
        {
            if (conexao != null)
            {
                conexao.disconnect();
            }
        }
    }

    // Par simples (código HTTP + corpo da resposta) devolvido por chamar().
    private static class RespostaHttp
    {
        final int codigo;
        final String corpo;

        RespostaHttp(int codigo, String corpo)
        {
            this.codigo = codigo;
            this.corpo = corpo;
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
