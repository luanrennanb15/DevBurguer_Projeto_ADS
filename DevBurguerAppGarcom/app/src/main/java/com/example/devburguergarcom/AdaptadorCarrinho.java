package com.example.devburguergarcom;

import android.content.Context;
import android.content.DialogInterface;
import android.text.Editable;
import android.text.TextWatcher;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.EditText;
import android.widget.LinearLayout;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.appcompat.app.AlertDialog;

import com.example.devburguergarcom.modelo.Adicional;
import com.example.devburguergarcom.modelo.CatalogoAdicionais;
import com.example.devburguergarcom.modelo.ItemPedido;

import java.util.ArrayList;
import java.util.List;

// Liga a lista de ItemPedido ao ListView do carrinho. Cada card tem um campo de
// observação e um contador -/+. Como o carrinho costuma ter poucos itens, essa
// classe sempre infla uma view nova (não reaproveita), pra não complicar o
// TextWatcher do campo de observação com a reciclagem do ListView.
public class AdaptadorCarrinho extends ArrayAdapter<ItemPedido>
{
    public interface OuvinteCarrinho
    {
        void aoAlterarCarrinho();
    }

    private final OuvinteCarrinho ouvinte;

    public AdaptadorCarrinho(Context context, List<ItemPedido> itens, OuvinteCarrinho ouvinte)
    {
        super(context, 0, itens);
        this.ouvinte = ouvinte;
    }

    @NonNull
    @Override
    public View getView(int position, View viewConvertido, @NonNull ViewGroup grupoPai)
    {
        View view = LayoutInflater.from(getContext()).inflate(R.layout.item_carrinho, grupoPai, false);

        ItemPedido item = getItem(position);

        TextView txvNome = view.findViewById(R.id.txvItemCarrinhoNome);
        TextView txvPreco = view.findViewById(R.id.txvItemCarrinhoPreco);
        LinearLayout lytAdicionais = view.findViewById(R.id.lytItemCarrinhoAdicionais);
        TextView txvAdicionais = view.findViewById(R.id.txvItemCarrinhoAdicionais);
        Button btnAdicionais = view.findViewById(R.id.btnItemCarrinhoAdicionais);
        EditText edtObservacao = view.findViewById(R.id.edtItemCarrinhoObservacao);
        TextView txvQuantidade = view.findViewById(R.id.txvItemCarrinhoQuantidade);
        Button btnRemover = view.findViewById(R.id.btnItemCarrinhoRemover);
        Button btnAdicionar = view.findViewById(R.id.btnItemCarrinhoAdicionar);

        txvNome.setText(item.getProduto().getNome());
        txvQuantidade.setText(String.valueOf(item.getQuantidade()));
        atualizarPreco(txvPreco, item);
        edtObservacao.setText(item.getObservacao());

        // Bebidas, sucos, alcoólicas e milkshakes não têm adicional de lanche
        // (mesma regra do site), então nem mostra o botão nesses casos.
        boolean aceitaAdicionais = CatalogoAdicionais.aceitaAdicionais(item.getProduto().getCategoria());
        lytAdicionais.setVisibility(aceitaAdicionais ? View.VISIBLE : View.GONE);
        atualizarTextoAdicionais(txvAdicionais, item);

        btnAdicionais.setOnClickListener(new View.OnClickListener()
        {
            @Override
            public void onClick(View v)
            {
                abrirDialogoAdicionais(item, txvAdicionais, txvPreco);
            }
        });

        edtObservacao.addTextChangedListener(new TextWatcher()
        {
            @Override
            public void beforeTextChanged(CharSequence s, int start, int count, int after)
            {
            }

            @Override
            public void onTextChanged(CharSequence s, int start, int before, int count)
            {
            }

            @Override
            public void afterTextChanged(Editable s)
            {
                item.setObservacao(s.toString());
            }
        });

        btnAdicionar.setOnClickListener(new View.OnClickListener()
        {
            @Override
            public void onClick(View v)
            {
                item.setQuantidade(item.getQuantidade() + 1);
                txvQuantidade.setText(String.valueOf(item.getQuantidade()));
                atualizarPreco(txvPreco, item);
                ouvinte.aoAlterarCarrinho();
            }
        });

        btnRemover.setOnClickListener(new View.OnClickListener()
        {
            @Override
            public void onClick(View v)
            {
                if (item.getQuantidade() <= 1)
                {
                    new AlertDialog.Builder(getContext())
                            .setTitle(R.string.tituloRemoverItem)
                            .setMessage(getContext().getString(R.string.mensagemRemoverItem, item.getProduto().getNome()))
                            .setNegativeButton(R.string.botaoCancelar, null)
                            .setPositiveButton(R.string.botaoRemover, new DialogInterface.OnClickListener()
                            {
                                @Override
                                public void onClick(DialogInterface dialog, int which)
                                {
                                    remove(item);
                                    ouvinte.aoAlterarCarrinho();
                                }
                            })
                            .show();
                }
                else
                {
                    item.setQuantidade(item.getQuantidade() - 1);
                    txvQuantidade.setText(String.valueOf(item.getQuantidade()));
                    atualizarPreco(txvPreco, item);
                    ouvinte.aoAlterarCarrinho();
                }
            }
        });

        return view;
    }

    private void atualizarPreco(TextView txvPreco, ItemPedido item)
    {
        txvPreco.setText(getContext().getString(R.string.rotuloPreco, Dinheiro.formatar(item.getSubtotal())));
    }

    private void atualizarTextoAdicionais(TextView txvAdicionais, ItemPedido item)
    {
        if (item.getAdicionais().isEmpty())
        {
            txvAdicionais.setText(R.string.rotuloSemAdicionais);
        }
        else
        {
            txvAdicionais.setText(getContext().getString(
                    R.string.rotuloAdicionaisEscolhidos, String.join(", ", item.getAdicionais())));
        }
    }

    // Diálogo de múltipla escolha com os adicionais fixos do CatalogoAdicionais.
    // A escolha só vira definitiva se o garçom confirmar (botão "Confirmar");
    // cancelando, o item.getAdicionais() nem é lido.
    private void abrirDialogoAdicionais(ItemPedido item, TextView txvAdicionais, TextView txvPreco)
    {
        List<Adicional> todos = CatalogoAdicionais.listarTodos();
        CharSequence[] opcoes = new CharSequence[todos.size()];
        boolean[] marcados = new boolean[todos.size()];
        List<String> escolhidos = new ArrayList<>(item.getAdicionais());

        for (int i = 0; i < todos.size(); i++)
        {
            Adicional adicional = todos.get(i);
            opcoes[i] = getContext().getString(
                    R.string.rotuloOpcaoAdicional, adicional.getNome(), Dinheiro.formatar(adicional.getPreco()));
            marcados[i] = escolhidos.contains(adicional.getNome());
        }

        new AlertDialog.Builder(getContext())
                .setTitle(R.string.tituloEscolherAdicionais)
                .setMultiChoiceItems(opcoes, marcados, new DialogInterface.OnMultiChoiceClickListener()
                {
                    @Override
                    public void onClick(DialogInterface dialog, int posicao, boolean marcado)
                    {
                        String nomeAdicional = todos.get(posicao).getNome();
                        if (marcado)
                        {
                            escolhidos.add(nomeAdicional);
                        }
                        else
                        {
                            escolhidos.remove(nomeAdicional);
                        }
                    }
                })
                .setNegativeButton(R.string.botaoCancelar, null)
                .setPositiveButton(R.string.botaoConfirmar, new DialogInterface.OnClickListener()
                {
                    @Override
                    public void onClick(DialogInterface dialog, int which)
                    {
                        item.setAdicionais(escolhidos);
                        atualizarTextoAdicionais(txvAdicionais, item);
                        atualizarPreco(txvPreco, item);
                        ouvinte.aoAlterarCarrinho();
                    }
                })
                .show();
    }
}
