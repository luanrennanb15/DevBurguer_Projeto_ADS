package com.example.devburguergarcom;

import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ArrayAdapter;
import android.widget.LinearLayout;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.core.content.ContextCompat;

import com.example.devburguergarcom.modelo.ItemPedido;
import com.example.devburguergarcom.modelo.Rodada;

import java.util.List;

// Liga a lista de Rodada ao ListView da Comanda. Cada card mostra o número da
// rodada, um selo de status e a lista de itens daquela rodada.
public class AdaptadorRodada extends ArrayAdapter<Rodada>
{
    public AdaptadorRodada(Context context, List<Rodada> rodadas)
    {
        super(context, 0, rodadas);
    }

    @NonNull
    @Override
    public View getView(int position, View viewConvertido, @NonNull ViewGroup grupoPai)
    {
        View view = LayoutInflater.from(getContext()).inflate(R.layout.item_rodada, grupoPai, false);

        Rodada rodada = getItem(position);

        LinearLayout painel = view.findViewById(R.id.painelItemRodada);
        TextView txvNumero = view.findViewById(R.id.txvItemRodadaNumero);
        TextView txvStatus = view.findViewById(R.id.txvItemRodadaStatus);
        LinearLayout lytItens = view.findViewById(R.id.lytItensRodada);
        TextView txvSubtotal = view.findViewById(R.id.txvItemRodadaSubtotal);

        txvNumero.setText(getContext().getString(R.string.rotuloRodada, rodada.getNumero()));

        boolean entregue = rodada.getStatus().equals(Rodada.STATUS_ENTREGUE);

        if (entregue)
        {
            txvStatus.setText(R.string.statusRodadaEntregue);
            txvStatus.setBackgroundResource(R.drawable.fundo_selo_verde);
            painel.setBackgroundResource(R.drawable.fundo_cartao);
        }
        else
        {
            txvStatus.setText(R.string.statusRodadaEmPreparo);
            txvStatus.setBackgroundResource(R.drawable.fundo_selo_dourado);
            painel.setBackgroundResource(R.drawable.fundo_cartao_destaque);
        }

        lytItens.removeAllViews();
        for (ItemPedido item : rodada.getItens())
        {
            lytItens.addView(criarLinhaItem(item));
        }

        txvSubtotal.setText(getContext().getString(R.string.rotuloPreco, Dinheiro.formatar(rodada.getSubtotal())));

        return view;
    }

    private View criarLinhaItem(ItemPedido item)
    {
        LinearLayout linha = new LinearLayout(getContext());
        linha.setOrientation(LinearLayout.HORIZONTAL);
        linha.setLayoutParams(new LinearLayout.LayoutParams(
                LinearLayout.LayoutParams.MATCH_PARENT, LinearLayout.LayoutParams.WRAP_CONTENT));

        TextView txvDescricao = new TextView(getContext());
        txvDescricao.setText(getContext().getString(R.string.rotuloItemRodada, item.getQuantidade(), item.getProduto().getNome()));
        txvDescricao.setTextColor(ContextCompat.getColor(getContext(), R.color.corBranca));
        txvDescricao.setTextSize(15);
        LinearLayout.LayoutParams parametrosDescricao = new LinearLayout.LayoutParams(
                0, LinearLayout.LayoutParams.WRAP_CONTENT, 1);
        txvDescricao.setLayoutParams(parametrosDescricao);

        TextView txvValor = new TextView(getContext());
        txvValor.setText(getContext().getString(R.string.rotuloPreco, Dinheiro.formatar(item.getSubtotal())));
        txvValor.setTextColor(ContextCompat.getColor(getContext(), R.color.corBranca));
        txvValor.setTextSize(15);
        txvValor.setTypeface(null, android.graphics.Typeface.BOLD);

        linha.addView(txvDescricao);
        linha.addView(txvValor);

        return linha;
    }
}
