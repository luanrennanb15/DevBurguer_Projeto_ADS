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

import com.example.devburguergarcom.modelo.Mesa;

import java.util.List;

// Liga a lista de objetos Mesa ao layout item_mesa.xml, um card por mesa na grade.
public class AdaptadorMesa extends ArrayAdapter<Mesa>
{
    public AdaptadorMesa(Context context, List<Mesa> mesas)
    {
        super(context, 0, mesas);
    }

    @NonNull
    @Override
    public View getView(int position, View viewConvertido, @NonNull ViewGroup grupoPai)
    {
        if (viewConvertido == null)
        {
            viewConvertido = LayoutInflater.from(getContext()).inflate(R.layout.item_mesa, grupoPai, false);
        }

        Mesa mesa = getItem(position);

        LinearLayout painel = viewConvertido.findViewById(R.id.painelItemMesa);
        TextView txvNumero = viewConvertido.findViewById(R.id.txvItemMesaNumero);
        TextView txvStatus = viewConvertido.findViewById(R.id.txvItemMesaStatus);

        txvNumero.setText(getContext().getString(R.string.rotuloMesaNumero, mesa.getNumero()));
        txvStatus.setText(mesa.getStatus());

        if (mesa.getStatus().equals(getContext().getString(R.string.statusMesaLivre)))
        {
            painel.setBackgroundResource(R.drawable.fundo_mesa_livre);
            txvStatus.setTextColor(ContextCompat.getColor(getContext(), R.color.corVerde));
        }
        else
        {
            painel.setBackgroundResource(R.drawable.fundo_mesa_ocupada);
            txvStatus.setTextColor(ContextCompat.getColor(getContext(), R.color.corVermelhoPrincipal));
        }

        return viewConvertido;
    }
}
