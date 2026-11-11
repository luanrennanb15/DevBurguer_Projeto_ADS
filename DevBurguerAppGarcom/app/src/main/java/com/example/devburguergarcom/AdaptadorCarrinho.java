package com.example.devburguergarcom;

import android.content.Context;
import android.text.Editable;
import android.text.TextWatcher;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.EditText;
import android.widget.TextView;

import androidx.annotation.NonNull;

import com.example.devburguergarcom.modelo.ItemPedido;

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
        EditText edtObservacao = view.findViewById(R.id.edtItemCarrinhoObservacao);
        TextView txvQuantidade = view.findViewById(R.id.txvItemCarrinhoQuantidade);
        Button btnRemover = view.findViewById(R.id.btnItemCarrinhoRemover);
        Button btnAdicionar = view.findViewById(R.id.btnItemCarrinhoAdicionar);

        txvNome.setText(item.getProduto().getNome());
        txvQuantidade.setText(String.valueOf(item.getQuantidade()));
        atualizarPreco(txvPreco, item);
        edtObservacao.setText(item.getObservacao());

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
                    remove(item);
                }
                else
                {
                    item.setQuantidade(item.getQuantidade() - 1);
                    txvQuantidade.setText(String.valueOf(item.getQuantidade()));
                    atualizarPreco(txvPreco, item);
                }

                ouvinte.aoAlterarCarrinho();
            }
        });

        return view;
    }

    private void atualizarPreco(TextView txvPreco, ItemPedido item)
    {
        txvPreco.setText(getContext().getString(R.string.rotuloPreco, Dinheiro.formatar(item.getSubtotal())));
    }
}
