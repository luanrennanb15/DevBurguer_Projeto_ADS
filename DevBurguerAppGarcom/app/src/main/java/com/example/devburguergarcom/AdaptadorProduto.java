package com.example.devburguergarcom;

import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.LinearLayout;
import android.widget.TextView;

import androidx.annotation.NonNull;

import com.example.devburguergarcom.modelo.Produto;

import java.util.List;
import java.util.Map;

// Liga a lista de Produto ao ListView do cardápio. Cada linha mostra um botão
// "Adicionar" (produto ainda não está no pedido) ou um contador -/+ (já está).
public class AdaptadorProduto extends ArrayAdapter<Produto>
{
    // Avisa a Activity sempre que a quantidade de um produto muda, pra ela
    // atualizar o rodapé (total de itens e valor).
    public interface OuvinteQuantidade
    {
        void aoAlterarQuantidade(Produto produto, int novaQuantidade);
    }

    private final Map<Integer, Integer> quantidades;
    private final OuvinteQuantidade ouvinte;

    public AdaptadorProduto(Context context, List<Produto> produtos, Map<Integer, Integer> quantidades, OuvinteQuantidade ouvinte)
    {
        super(context, 0, produtos);
        this.quantidades = quantidades;
        this.ouvinte = ouvinte;
    }

    @NonNull
    @Override
    public View getView(int position, View viewConvertido, @NonNull ViewGroup grupoPai)
    {
        if (viewConvertido == null)
        {
            viewConvertido = LayoutInflater.from(getContext()).inflate(R.layout.item_produto, grupoPai, false);
        }

        Produto produto = getItem(position);

        TextView txvNome = viewConvertido.findViewById(R.id.txvItemProdutoNome);
        TextView txvDescricao = viewConvertido.findViewById(R.id.txvItemProdutoDescricao);
        TextView txvPreco = viewConvertido.findViewById(R.id.txvItemProdutoPreco);
        LinearLayout grpContador = viewConvertido.findViewById(R.id.grpContadorProduto);
        TextView txvQuantidade = viewConvertido.findViewById(R.id.txvItemProdutoQuantidade);
        Button btnAdicionar = viewConvertido.findViewById(R.id.btnItemProdutoAdicionar);
        Button btnRemover = viewConvertido.findViewById(R.id.btnItemProdutoRemover);
        Button btnAdicionarMais = viewConvertido.findViewById(R.id.btnItemProdutoAdicionarMais);

        txvNome.setText(produto.getNome());
        txvDescricao.setText(produto.getDescricao());
        txvDescricao.setVisibility(produto.getDescricao().isEmpty() ? View.GONE : View.VISIBLE);
        txvPreco.setText(getContext().getString(R.string.rotuloPreco, Dinheiro.formatar(produto.getPreco())));

        int quantidadeAtual = quantidades.containsKey(produto.getId()) ? quantidades.get(produto.getId()) : 0;
        atualizarVisual(grpContador, btnAdicionar, txvQuantidade, quantidadeAtual);

        btnAdicionar.setOnClickListener(new View.OnClickListener()
        {
            @Override
            public void onClick(View v)
            {
                alterarQuantidade(produto, 1, grpContador, btnAdicionar, txvQuantidade);
            }
        });

        btnAdicionarMais.setOnClickListener(new View.OnClickListener()
        {
            @Override
            public void onClick(View v)
            {
                int quantidade = (quantidades.containsKey(produto.getId()) ? quantidades.get(produto.getId()) : 0) + 1;
                alterarQuantidade(produto, quantidade, grpContador, btnAdicionar, txvQuantidade);
            }
        });

        btnRemover.setOnClickListener(new View.OnClickListener()
        {
            @Override
            public void onClick(View v)
            {
                int quantidade = (quantidades.containsKey(produto.getId()) ? quantidades.get(produto.getId()) : 0) - 1;
                alterarQuantidade(produto, quantidade, grpContador, btnAdicionar, txvQuantidade);
            }
        });

        return viewConvertido;
    }

    private void alterarQuantidade(Produto produto, int novaQuantidade, LinearLayout grpContador, Button btnAdicionar, TextView txvQuantidade)
    {
        if (novaQuantidade <= 0)
        {
            quantidades.remove(produto.getId());
        }
        else
        {
            quantidades.put(produto.getId(), novaQuantidade);
        }

        atualizarVisual(grpContador, btnAdicionar, txvQuantidade, novaQuantidade);
        ouvinte.aoAlterarQuantidade(produto, Math.max(novaQuantidade, 0));
    }

    private void atualizarVisual(LinearLayout grpContador, Button btnAdicionar, TextView txvQuantidade, int quantidade)
    {
        if (quantidade > 0)
        {
            grpContador.setVisibility(View.VISIBLE);
            btnAdicionar.setVisibility(View.GONE);
            txvQuantidade.setText(String.valueOf(quantidade));
        }
        else
        {
            grpContador.setVisibility(View.GONE);
            btnAdicionar.setVisibility(View.VISIBLE);
        }
    }
}
