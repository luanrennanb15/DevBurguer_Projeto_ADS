// ============================================================================
// DEVBURGUER MOBILE — App principal (React Native + Expo)
// ----------------------------------------------------------------------------
// Aplicativo do cliente: consulta o cardapio, monta o carrinho e envia o
// pedido, consumindo a MESMA API REST do site (hospedada no Render).
// Feito em um unico arquivo, usando apenas componentes do React Native,
// para ser simples de rodar (nao precisa de bibliotecas extras).
// ============================================================================

import { StatusBar } from 'expo-status-bar';
import { useEffect, useMemo, useState } from 'react';
import {
  ActivityIndicator, Alert, FlatList, SafeAreaView, ScrollView,
  StyleSheet, Text, TextInput, TouchableOpacity, View,
} from 'react-native';

// ─── Configuracao ───────────────────────────────────────────────────────────
const API_BASE = 'https://devburguer-api-9gds.onrender.com/api';
const TAXA_ENTREGA = 6.0;

// Cores do app (identidade DevBurguer)
const C = {
  fundo: '#14141c', cartao: '#1e1e2a', texto: '#ececf5', suave: '#9a9ab5',
  roxo: '#823cdc', roxoClaro: '#b478ff', laranja: '#e08a1e', verde: '#2fae5a',
  borda: '#2b2b3c', erro: '#e05a5a',
};

// Rotulos e ordem das categorias (os mesmos slugs que a API devolve)
const CATEGORIAS = [
  { slug: 'tradicionais', label: 'Tradicionais' },
  { slug: 'gourmet', label: 'Gourmet' },
  { slug: 'combos', label: 'Combos' },
  { slug: 'bebidas', label: 'Bebidas' },
  { slug: 'sucos', label: 'Sucos' },
  { slug: 'alcoolicas', label: 'Cervejas' },
  { slug: 'milkshakes', label: 'Milkshakes' },
];

const money = (v) => 'R$ ' + Number(v).toFixed(2).replace('.', ',');

export default function App() {
  const [tela, setTela] = useState('cardapio');      // cardapio | carrinho | checkout | sucesso
  const [produtos, setProdutos] = useState([]);
  const [carregando, setCarregando] = useState(true);
  const [erro, setErro] = useState(null);
  const [carrinho, setCarrinho] = useState([]);        // [{ produto, qtd }]
  const [pedido, setPedido] = useState(null);          // resposta da API ao finalizar

  // ── Carrega o cardapio da API ──────────────────────────────────────────
  async function carregarCardapio() {
    setCarregando(true);
    setErro(null);
    try {
      const r = await fetch(`${API_BASE}/produtos`);
      if (!r.ok) throw new Error('status ' + r.status);
      const dados = await r.json();
      setProdutos(dados);
    } catch (e) {
      setErro('Nao foi possivel carregar o cardapio. Verifique sua internet e tente de novo. (A API gratuita pode levar ate 1 minuto para "acordar" no primeiro acesso.)');
    } finally {
      setCarregando(false);
    }
  }
  useEffect(() => { carregarCardapio(); }, []);

  // ── Carrinho ───────────────────────────────────────────────────────────
  const qtdNoCarrinho = carrinho.reduce((s, i) => s + i.qtd, 0);
  const subtotal = carrinho.reduce((s, i) => s + i.produto.preco * i.qtd, 0);

  function adicionar(produto) {
    setCarrinho((prev) => {
      const ex = prev.find((i) => i.produto.id === produto.id);
      if (ex) return prev.map((i) => i.produto.id === produto.id ? { ...i, qtd: i.qtd + 1 } : i);
      return [...prev, { produto, qtd: 1 }];
    });
  }
  function mudarQtd(id, delta) {
    setCarrinho((prev) => prev
      .map((i) => i.produto.id === id ? { ...i, qtd: i.qtd + delta } : i)
      .filter((i) => i.qtd > 0));
  }

  // ── Telas ────────────────────────────────────────────────────────────────
  return (
    <SafeAreaView style={s.safe}>
      <StatusBar style="light" />
      <Cabecalho />

      {tela === 'cardapio' && (
        <Cardapio
          produtos={produtos} carregando={carregando} erro={erro}
          onRecarregar={carregarCardapio} onAdicionar={adicionar}
        />
      )}
      {tela === 'carrinho' && (
        <Carrinho
          carrinho={carrinho} subtotal={subtotal} onMudarQtd={mudarQtd}
          irCardapio={() => setTela('cardapio')}
          irCheckout={() => setTela('checkout')}
        />
      )}
      {tela === 'checkout' && (
        <Checkout
          carrinho={carrinho} subtotal={subtotal}
          onVoltar={() => setTela('carrinho')}
          onSucesso={(resp) => { setPedido(resp); setCarrinho([]); setTela('sucesso'); }}
        />
      )}
      {tela === 'sucesso' && (
        <Sucesso pedido={pedido} onVoltar={() => setTela('cardapio')} />
      )}

      {/* Barra inferior */}
      {tela !== 'sucesso' && (
        <View style={s.tabbar}>
          <TabBtn ativo={tela === 'cardapio'} label="Cardapio" onPress={() => setTela('cardapio')} />
          <TabBtn ativo={tela === 'carrinho' || tela === 'checkout'}
            label={`Carrinho${qtdNoCarrinho ? ' (' + qtdNoCarrinho + ')' : ''}`}
            onPress={() => setTela('carrinho')} />
        </View>
      )}
    </SafeAreaView>
  );
}

// ─── Componentes ─────────────────────────────────────────────────────────────
function Cabecalho() {
  return (
    <View style={s.header}>
      <Text style={s.logo}>Dev<Text style={{ color: C.laranja }}>Burguer</Text></Text>
      <Text style={s.sub}>Peca pelo app</Text>
    </View>
  );
}

function TabBtn({ ativo, label, onPress }) {
  return (
    <TouchableOpacity style={[s.tabBtn, ativo && s.tabBtnAtivo]} onPress={onPress}>
      <Text style={[s.tabTxt, ativo && s.tabTxtAtivo]}>{label}</Text>
    </TouchableOpacity>
  );
}

function Cardapio({ produtos, carregando, erro, onRecarregar, onAdicionar }) {
  const porCategoria = useMemo(() => {
    const mapa = {};
    for (const p of produtos) (mapa[p.categoria] = mapa[p.categoria] || []).push(p);
    return mapa;
  }, [produtos]);

  if (carregando) {
    return (
      <View style={s.centro}>
        <ActivityIndicator size="large" color={C.roxoClaro} />
        <Text style={s.carregandoTxt}>Carregando o cardapio...</Text>
      </View>
    );
  }
  if (erro) {
    return (
      <View style={s.centro}>
        <Text style={s.erroTxt}>{erro}</Text>
        <TouchableOpacity style={s.btnPrimario} onPress={onRecarregar}>
          <Text style={s.btnPrimarioTxt}>Tentar de novo</Text>
        </TouchableOpacity>
      </View>
    );
  }

  return (
    <ScrollView contentContainerStyle={{ padding: 14, paddingBottom: 90 }}>
      {CATEGORIAS.filter((c) => porCategoria[c.slug]?.length).map((cat) => (
        <View key={cat.slug} style={{ marginBottom: 18 }}>
          <Text style={s.catTitulo}>{cat.label}</Text>
          {porCategoria[cat.slug].map((p) => (
            <View key={p.id} style={s.card}>
              <View style={{ flex: 1, paddingRight: 10 }}>
                <Text style={s.nome}>{p.nome}</Text>
                {!!p.descricao && <Text style={s.desc} numberOfLines={2}>{p.descricao}</Text>}
                <Text style={s.preco}>{money(p.preco)}</Text>
              </View>
              <TouchableOpacity style={s.btnAdd} onPress={() => onAdicionar(p)}>
                <Text style={s.btnAddTxt}>+</Text>
              </TouchableOpacity>
            </View>
          ))}
        </View>
      ))}
    </ScrollView>
  );
}

function Carrinho({ carrinho, subtotal, onMudarQtd, irCardapio, irCheckout }) {
  if (!carrinho.length) {
    return (
      <View style={s.centro}>
        <Text style={s.vazioTxt}>Seu carrinho esta vazio.</Text>
        <TouchableOpacity style={s.btnPrimario} onPress={irCardapio}>
          <Text style={s.btnPrimarioTxt}>Ver o cardapio</Text>
        </TouchableOpacity>
      </View>
    );
  }
  return (
    <View style={{ flex: 1 }}>
      <ScrollView contentContainerStyle={{ padding: 14, paddingBottom: 90 }}>
        {carrinho.map((i) => (
          <View key={i.produto.id} style={s.card}>
            <View style={{ flex: 1, paddingRight: 10 }}>
              <Text style={s.nome}>{i.produto.nome}</Text>
              <Text style={s.preco}>{money(i.produto.preco * i.qtd)}</Text>
            </View>
            <View style={s.qtdBox}>
              <TouchableOpacity style={s.qtdBtn} onPress={() => onMudarQtd(i.produto.id, -1)}>
                <Text style={s.qtdBtnTxt}>-</Text>
              </TouchableOpacity>
              <Text style={s.qtdNum}>{i.qtd}</Text>
              <TouchableOpacity style={s.qtdBtn} onPress={() => onMudarQtd(i.produto.id, +1)}>
                <Text style={s.qtdBtnTxt}>+</Text>
              </TouchableOpacity>
            </View>
          </View>
        ))}
        <Text style={s.obsTaxa}>* A taxa de entrega de {money(TAXA_ENTREGA)} e somada quando o pedido for para entrega.</Text>
      </ScrollView>
      <View style={s.rodape}>
        <Text style={s.totalTxt}>Subtotal: <Text style={{ color: C.texto, fontWeight: 'bold' }}>{money(subtotal)}</Text></Text>
        <TouchableOpacity style={s.btnPrimario} onPress={irCheckout}>
          <Text style={s.btnPrimarioTxt}>Finalizar pedido</Text>
        </TouchableOpacity>
      </View>
    </View>
  );
}

function Checkout({ carrinho, subtotal, onVoltar, onSucesso }) {
  const [nome, setNome] = useState('');
  const [telefone, setTelefone] = useState('');
  const [tipoEntrega, setTipoEntrega] = useState('Entrega');
  const [endereco, setEndereco] = useState('');
  const [numero, setNumero] = useState('');
  const [bairro, setBairro] = useState('');
  const [pagamento, setPagamento] = useState('Pix');
  const [enviando, setEnviando] = useState(false);

  const total = subtotal + (tipoEntrega === 'Entrega' ? TAXA_ENTREGA : 0);

  async function enviar() {
    if (!nome.trim() || !telefone.trim())
      return Alert.alert('Atencao', 'Preencha nome e telefone.');
    if (tipoEntrega === 'Entrega' && (!endereco.trim() || !bairro.trim()))
      return Alert.alert('Atencao', 'Para entrega, preencha endereco e bairro.');

    const corpo = {
      cliente: { nome: nome.trim(), telefone: telefone.trim() },
      tipoEntrega,
      endereco, numero, bairro,
      formaPagamento: pagamento,
      itens: carrinho.map((i) => ({ idProduto: i.produto.id, quantidade: i.qtd, observacao: '' })),
    };
    setEnviando(true);
    try {
      const r = await fetch(`${API_BASE}/pedidos`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(corpo),
      });
      const resp = await r.json();
      if (!r.ok) throw new Error(resp.erro || 'erro');
      onSucesso(resp);
    } catch (e) {
      Alert.alert('Erro', 'Nao foi possivel enviar o pedido. Tente novamente.');
    } finally {
      setEnviando(false);
    }
  }

  return (
    <ScrollView contentContainerStyle={{ padding: 14, paddingBottom: 40 }}>
      <Text style={s.catTitulo}>Seus dados</Text>
      <Campo label="Nome" valor={nome} onChange={setNome} placeholder="Seu nome" />
      <Campo label="Telefone" valor={telefone} onChange={setTelefone} placeholder="(15) 90000-0000" teclado="phone-pad" />

      <Text style={[s.catTitulo, { marginTop: 14 }]}>Tipo</Text>
      <View style={{ flexDirection: 'row', gap: 10 }}>
        {['Entrega', 'Retirada'].map((t) => (
          <TouchableOpacity key={t} onPress={() => setTipoEntrega(t)}
            style={[s.opc, tipoEntrega === t && s.opcAtiva]}>
            <Text style={[s.opcTxt, tipoEntrega === t && s.opcTxtAtiva]}>{t}</Text>
          </TouchableOpacity>
        ))}
      </View>

      {tipoEntrega === 'Entrega' && (
        <>
          <Campo label="Endereco" valor={endereco} onChange={setEndereco} placeholder="Rua..." />
          <View style={{ flexDirection: 'row', gap: 10 }}>
            <View style={{ flex: 1 }}><Campo label="Numero" valor={numero} onChange={setNumero} placeholder="123" teclado="numeric" /></View>
            <View style={{ flex: 2 }}><Campo label="Bairro" valor={bairro} onChange={setBairro} placeholder="Centro" /></View>
          </View>
        </>
      )}

      <Text style={[s.catTitulo, { marginTop: 14 }]}>Pagamento</Text>
      <View style={{ flexDirection: 'row', flexWrap: 'wrap', gap: 10 }}>
        {['Pix', 'Dinheiro', 'Cartao'].map((p) => (
          <TouchableOpacity key={p} onPress={() => setPagamento(p)}
            style={[s.opc, pagamento === p && s.opcAtiva]}>
            <Text style={[s.opcTxt, pagamento === p && s.opcTxtAtiva]}>{p}</Text>
          </TouchableOpacity>
        ))}
      </View>

      <View style={[s.card, { marginTop: 18 }]}>
        <Text style={{ color: C.suave }}>Total {tipoEntrega === 'Entrega' ? '(com entrega)' : ''}</Text>
        <Text style={{ color: C.roxoClaro, fontSize: 20, fontWeight: 'bold', marginLeft: 'auto' }}>{money(total)}</Text>
      </View>

      <TouchableOpacity style={[s.btnPrimario, { marginTop: 16 }]} onPress={enviar} disabled={enviando}>
        <Text style={s.btnPrimarioTxt}>{enviando ? 'Enviando...' : 'Enviar pedido'}</Text>
      </TouchableOpacity>
      <TouchableOpacity style={s.btnSecundario} onPress={onVoltar}>
        <Text style={s.btnSecundarioTxt}>Voltar ao carrinho</Text>
      </TouchableOpacity>
    </ScrollView>
  );
}

function Campo({ label, valor, onChange, placeholder, teclado }) {
  return (
    <View style={{ marginTop: 10 }}>
      <Text style={s.campoLabel}>{label}</Text>
      <TextInput
        style={s.input} value={valor} onChangeText={onChange}
        placeholder={placeholder} placeholderTextColor={C.suave}
        keyboardType={teclado || 'default'}
      />
    </View>
  );
}

function Sucesso({ pedido, onVoltar }) {
  return (
    <View style={s.centro}>
      <Text style={{ fontSize: 54 }}>✅</Text>
      <Text style={s.sucessoTitulo}>Pedido enviado!</Text>
      <Text style={s.sucessoTxt}>Numero do pedido: <Text style={{ color: C.roxoClaro, fontWeight: 'bold' }}>#{pedido?.idPedido}</Text></Text>
      <Text style={s.sucessoTxt}>Total: {money(pedido?.total || 0)}</Text>
      <Text style={[s.sucessoTxt, { marginTop: 8 }]}>{pedido?.mensagem || 'Aguardando confirmacao da lanchonete.'}</Text>
      <TouchableOpacity style={[s.btnPrimario, { marginTop: 20 }]} onPress={onVoltar}>
        <Text style={s.btnPrimarioTxt}>Fazer outro pedido</Text>
      </TouchableOpacity>
    </View>
  );
}

// ─── Estilos ─────────────────────────────────────────────────────────────────
const s = StyleSheet.create({
  safe: { flex: 1, backgroundColor: C.fundo },
  header: { paddingHorizontal: 16, paddingVertical: 12, borderBottomWidth: 1, borderBottomColor: C.borda },
  logo: { color: C.texto, fontSize: 24, fontWeight: 'bold' },
  sub: { color: C.suave, fontSize: 13 },
  centro: { flex: 1, alignItems: 'center', justifyContent: 'center', padding: 24 },
  carregandoTxt: { color: C.suave, marginTop: 12 },
  erroTxt: { color: C.erro, textAlign: 'center', marginBottom: 16, lineHeight: 20 },
  vazioTxt: { color: C.suave, fontSize: 16, marginBottom: 16 },
  catTitulo: { color: C.laranja, fontSize: 16, fontWeight: 'bold', marginBottom: 8 },
  card: { flexDirection: 'row', alignItems: 'center', backgroundColor: C.cartao, borderRadius: 12, padding: 12, marginBottom: 10, borderWidth: 1, borderColor: C.borda },
  nome: { color: C.texto, fontSize: 15, fontWeight: '600' },
  desc: { color: C.suave, fontSize: 12, marginTop: 2 },
  preco: { color: C.roxoClaro, fontSize: 15, fontWeight: 'bold', marginTop: 4 },
  btnAdd: { backgroundColor: C.roxo, width: 40, height: 40, borderRadius: 20, alignItems: 'center', justifyContent: 'center' },
  btnAddTxt: { color: '#fff', fontSize: 24, lineHeight: 26, fontWeight: 'bold' },
  qtdBox: { flexDirection: 'row', alignItems: 'center', gap: 10 },
  qtdBtn: { backgroundColor: C.borda, width: 34, height: 34, borderRadius: 17, alignItems: 'center', justifyContent: 'center' },
  qtdBtnTxt: { color: C.texto, fontSize: 20, fontWeight: 'bold' },
  qtdNum: { color: C.texto, fontSize: 16, minWidth: 20, textAlign: 'center' },
  obsTaxa: { color: C.suave, fontSize: 12, marginTop: 6, lineHeight: 18 },
  rodape: { position: 'absolute', bottom: 60, left: 0, right: 0, padding: 14, backgroundColor: C.fundo, borderTopWidth: 1, borderTopColor: C.borda },
  totalTxt: { color: C.suave, fontSize: 15, marginBottom: 8 },
  tabbar: { flexDirection: 'row', height: 60, borderTopWidth: 1, borderTopColor: C.borda, backgroundColor: C.cartao },
  tabBtn: { flex: 1, alignItems: 'center', justifyContent: 'center' },
  tabBtnAtivo: { borderTopWidth: 2, borderTopColor: C.roxoClaro },
  tabTxt: { color: C.suave, fontSize: 14 },
  tabTxtAtivo: { color: C.roxoClaro, fontWeight: 'bold' },
  btnPrimario: { backgroundColor: C.roxo, borderRadius: 12, paddingVertical: 14, alignItems: 'center' },
  btnPrimarioTxt: { color: '#fff', fontSize: 16, fontWeight: 'bold' },
  btnSecundario: { paddingVertical: 12, alignItems: 'center', marginTop: 4 },
  btnSecundarioTxt: { color: C.suave, fontSize: 14 },
  campoLabel: { color: C.suave, fontSize: 13, marginBottom: 4 },
  input: { backgroundColor: C.cartao, borderWidth: 1, borderColor: C.borda, borderRadius: 10, paddingHorizontal: 12, paddingVertical: 10, color: C.texto, fontSize: 15 },
  opc: { paddingVertical: 10, paddingHorizontal: 18, borderRadius: 10, borderWidth: 1, borderColor: C.borda, backgroundColor: C.cartao },
  opcAtiva: { borderColor: C.roxoClaro, backgroundColor: C.roxo },
  opcTxt: { color: C.suave, fontWeight: '600' },
  opcTxtAtiva: { color: '#fff' },
  sucessoTitulo: { color: C.texto, fontSize: 22, fontWeight: 'bold', marginTop: 10 },
  sucessoTxt: { color: C.suave, fontSize: 15, marginTop: 4, textAlign: 'center' },
});
