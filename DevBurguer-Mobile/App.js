/**
 * DEVBURGUER — APP DO GARÇOM (Expo / React Native)
 * -----------------------------------------------------------------------------
 * App de salão para o garçom lançar os pedidos mesa a mesa.
 *
 * Fluxo: Login  ->  Escolher mesa  ->  Cardápio (montar a rodada)
 *        ->  Enviar rodada  ->  Comanda da mesa (total p/ pagar no caixa)
 *
 * Conversa com a mesma API REST do site (Render). A navegação entre telas é
 * feita com um estado simples (sem bibliotecas extras), pra ficar leve e fácil
 * de rodar no Expo Go.
 */

import React, { useState } from 'react';
import {
  SafeAreaView, View, Text, TextInput, TouchableOpacity, ScrollView,
  ActivityIndicator, Alert, StatusBar, StyleSheet, KeyboardAvoidingView, Platform,
} from 'react-native';

// Endereço da API no Render (mesma do site).
const API = 'https://devburguer-api-9gds.onrender.com/api';

// Chamada HTTP com tratamento de erro amigável.
async function chamarApi(caminho, opcoes = {}) {
  const resp = await fetch(API + caminho, {
    headers: { 'Content-Type': 'application/json' },
    ...opcoes,
  });
  let corpo = null;
  try { corpo = await resp.json(); } catch { /* sem corpo */ }
  if (!resp.ok) {
    const msg = (corpo && corpo.erro) ? corpo.erro : 'Erro ' + resp.status;
    throw new Error(msg);
  }
  return corpo;
}

const brl = (v) => 'R$ ' + Number(v || 0).toFixed(2).replace('.', ',');

export default function App() {
  const [tela, setTela]       = useState('login');   // login | mesa | cardapio | comanda
  const [garcom, setGarcom]   = useState(null);
  const [mesa, setMesa]       = useState(null);
  const [produtos, setProdutos] = useState([]);
  const [carrinho, setCarrinho] = useState({});       // idProduto -> { ...produto, quantidade }
  const [comanda, setComanda] = useState(null);
  const [carregando, setCarregando] = useState(false);

  // ─── Ações ────────────────────────────────────────────────────────────────

  function sair() {
    setGarcom(null); setMesa(null); setProdutos([]);
    setCarrinho({}); setComanda(null); setTela('login');
  }

  async function carregarProdutos() {
    setCarregando(true);
    try {
      const lista = await chamarApi('/produtos');
      setProdutos(Array.isArray(lista) ? lista : []);
    } catch (e) {
      Alert.alert('Ops', 'Não consegui carregar o cardápio.\n' + e.message);
    } finally {
      setCarregando(false);
    }
  }

  async function abrirMesa(numero) {
    const n = parseInt(numero, 10);
    if (isNaN(n) || n <= 0) {
      Alert.alert('Mesa inválida', 'Digite o número da mesa.');
      return;
    }
    setMesa(n);
    setCarrinho({});
    await carregarProdutos();
    setTela('cardapio');
  }

  function alterarQtd(produto, delta) {
    setCarrinho((atual) => {
      const item = atual[produto.id] || { ...produto, quantidade: 0 };
      const qtd = Math.max(0, item.quantidade + delta);
      const novo = { ...atual };
      if (qtd === 0) delete novo[produto.id];
      else novo[produto.id] = { ...item, quantidade: qtd };
      return novo;
    });
  }

  const itensCarrinho = Object.values(carrinho);
  const totalCarrinho = itensCarrinho.reduce((s, i) => s + i.preco * i.quantidade, 0);

  async function enviarRodada() {
    if (itensCarrinho.length === 0) return;
    setCarregando(true);
    try {
      const itens = itensCarrinho.map((i) => ({
        idProduto: i.id,
        quantidade: i.quantidade,
      }));
      const r = await chamarApi('/pedidos/garcom', {
        method: 'POST',
        body: JSON.stringify({ mesa, itens }),
      });
      setCarrinho({});
      Alert.alert('Rodada enviada!', 'Mesa ' + mesa + ' • ' + brl(r.total) + ' enviados para a cozinha.');
      await verComanda();
    } catch (e) {
      Alert.alert('Ops', 'Não consegui enviar a rodada.\n' + e.message);
    } finally {
      setCarregando(false);
    }
  }

  async function verComanda() {
    setCarregando(true);
    try {
      const c = await chamarApi('/mesas/' + mesa + '/comanda');
      setComanda(c);
      setTela('comanda');
    } catch (e) {
      Alert.alert('Ops', 'Não consegui carregar a comanda.\n' + e.message);
    } finally {
      setCarregando(false);
    }
  }

  // ─── Telas ──────────────────────────────────────────────────────────────

  function TelaLogin() {
    const [usuario, setUsuario] = useState('');
    const [senha, setSenha]     = useState('');

    async function entrar() {
      if (!usuario.trim() || !senha) {
        Alert.alert('Atenção', 'Preencha usuário e senha.');
        return;
      }
      setCarregando(true);
      try {
        const g = await chamarApi('/garcom/login', {
          method: 'POST',
          body: JSON.stringify({ usuario: usuario.trim(), senha }),
        });
        setGarcom(g);
        setTela('mesa');
      } catch (e) {
        Alert.alert('Não foi possível entrar', e.message);
      } finally {
        setCarregando(false);
      }
    }

    return (
      <KeyboardAvoidingView
        behavior={Platform.OS === 'ios' ? 'padding' : undefined}
        style={styles.flex}>
        <View style={styles.centro}>
          <Text style={styles.logo}>DevBurguer</Text>
          <Text style={styles.sub}>App do Garçom</Text>

          <View style={styles.card}>
            <Text style={styles.label}>Usuário</Text>
            <TextInput
              style={styles.input}
              autoCapitalize="none"
              placeholder="garcom"
              value={usuario}
              onChangeText={setUsuario}
            />
            <Text style={styles.label}>Senha</Text>
            <TextInput
              style={styles.input}
              secureTextEntry
              placeholder="••••••••"
              value={senha}
              onChangeText={setSenha}
            />
            <TouchableOpacity style={styles.botao} onPress={entrar} disabled={carregando}>
              <Text style={styles.botaoTexto}>{carregando ? 'Entrando...' : 'Entrar'}</Text>
            </TouchableOpacity>
          </View>
        </View>
      </KeyboardAvoidingView>
    );
  }

  function TelaMesa() {
    const [numero, setNumero] = useState('');
    return (
      <View style={styles.centro}>
        <Text style={styles.ola}>Olá, {garcom?.nome} 👋</Text>
        <Text style={styles.sub}>Qual mesa você vai atender?</Text>

        <View style={styles.card}>
          <Text style={styles.label}>Número da mesa</Text>
          <TextInput
            style={[styles.input, styles.inputMesa]}
            keyboardType="number-pad"
            placeholder="Ex: 7"
            value={numero}
            onChangeText={setNumero}
          />
          <TouchableOpacity style={styles.botao} onPress={() => abrirMesa(numero)} disabled={carregando}>
            <Text style={styles.botaoTexto}>{carregando ? 'Abrindo...' : 'Abrir mesa'}</Text>
          </TouchableOpacity>
        </View>

        <TouchableOpacity onPress={sair}>
          <Text style={styles.linkSair}>Sair</Text>
        </TouchableOpacity>
      </View>
    );
  }

  function TelaCardapio() {
    // Agrupa os produtos por categoria para exibir em seções.
    const porCategoria = {};
    for (const p of produtos) {
      const cat = p.categoria || 'Outros';
      (porCategoria[cat] = porCategoria[cat] || []).push(p);
    }

    return (
      <View style={styles.flex}>
        <View style={styles.header}>
          <View>
            <Text style={styles.headerTitulo}>Mesa {mesa}</Text>
            <Text style={styles.headerSub}>{garcom?.nome}</Text>
          </View>
          <TouchableOpacity onPress={() => setTela('mesa')}>
            <Text style={styles.headerLink}>Trocar mesa</Text>
          </TouchableOpacity>
        </View>

        {carregando && produtos.length === 0 ? (
          <View style={styles.centro}>
            <ActivityIndicator size="large" color="#E63946" />
            <Text style={styles.aviso}>Carregando cardápio...{'\n'}(a API pode levar até 1 min no 1º acesso)</Text>
          </View>
        ) : (
          <ScrollView contentContainerStyle={{ padding: 16, paddingBottom: 140 }}>
            {Object.keys(porCategoria).map((cat) => (
              <View key={cat} style={{ marginBottom: 18 }}>
                <Text style={styles.categoria}>{cat}</Text>
                {porCategoria[cat].map((p) => {
                  const qtd = carrinho[p.id]?.quantidade || 0;
                  return (
                    <View key={p.id} style={styles.produto}>
                      <View style={styles.flex}>
                        <Text style={styles.produtoNome}>{p.nome}</Text>
                        <Text style={styles.produtoPreco}>{brl(p.preco)}</Text>
                      </View>
                      <View style={styles.stepper}>
                        <TouchableOpacity style={styles.stepBtn} onPress={() => alterarQtd(p, -1)}>
                          <Text style={styles.stepBtnTexto}>−</Text>
                        </TouchableOpacity>
                        <Text style={styles.stepQtd}>{qtd}</Text>
                        <TouchableOpacity style={styles.stepBtn} onPress={() => alterarQtd(p, +1)}>
                          <Text style={styles.stepBtnTexto}>+</Text>
                        </TouchableOpacity>
                      </View>
                    </View>
                  );
                })}
              </View>
            ))}
          </ScrollView>
        )}

        {/* Barra inferior: resumo da rodada */}
        <View style={styles.barra}>
          <TouchableOpacity style={styles.botaoSec} onPress={verComanda} disabled={carregando}>
            <Text style={styles.botaoSecTexto}>Ver comanda</Text>
          </TouchableOpacity>
          <TouchableOpacity
            style={[styles.botao, styles.botaoBarra, itensCarrinho.length === 0 && styles.botaoOff]}
            onPress={enviarRodada}
            disabled={carregando || itensCarrinho.length === 0}>
            <Text style={styles.botaoTexto}>
              {carregando ? 'Enviando...' : 'Enviar rodada • ' + brl(totalCarrinho)}
            </Text>
          </TouchableOpacity>
        </View>
      </View>
    );
  }

  function TelaComanda() {
    const c = comanda || { itens: [], total: 0, rodadas: 0, aberta: false };
    return (
      <View style={styles.flex}>
        <View style={styles.header}>
          <View>
            <Text style={styles.headerTitulo}>Comanda — Mesa {mesa}</Text>
            <Text style={styles.headerSub}>{c.rodadas} rodada(s) • {garcom?.nome}</Text>
          </View>
          <TouchableOpacity onPress={verComanda}>
            <Text style={styles.headerLink}>Atualizar</Text>
          </TouchableOpacity>
        </View>

        <ScrollView contentContainerStyle={{ padding: 16, paddingBottom: 150 }}>
          {c.itens.length === 0 ? (
            <Text style={styles.aviso}>Nenhum item em aberto nesta mesa.</Text>
          ) : (
            c.itens.map((it, idx) => (
              <View key={idx} style={styles.linhaComanda}>
                <Text style={styles.comandaQtd}>{it.quantidade}x</Text>
                <Text style={[styles.flex, styles.comandaNome]}>{it.produto}</Text>
                <Text style={styles.comandaPreco}>{brl(it.preco * it.quantidade)}</Text>
              </View>
            ))
          )}
        </ScrollView>

        <View style={styles.barraTotal}>
          <View style={styles.totalLinha}>
            <Text style={styles.totalLabel}>Total da mesa</Text>
            <Text style={styles.totalValor}>{brl(c.total)}</Text>
          </View>
          <Text style={styles.obsCaixa}>O pagamento é feito no caixa.</Text>
          <View style={styles.barra}>
            <TouchableOpacity style={styles.botaoSec} onPress={() => setTela('cardapio')}>
              <Text style={styles.botaoSecTexto}>Nova rodada</Text>
            </TouchableOpacity>
            <TouchableOpacity style={[styles.botao, styles.botaoBarra]} onPress={() => setTela('mesa')}>
              <Text style={styles.botaoTexto}>Outra mesa</Text>
            </TouchableOpacity>
          </View>
        </View>
      </View>
    );
  }

  // ─── Render ───────────────────────────────────────────────────────────────

  let conteudo;
  if (tela === 'login') conteudo = <TelaLogin />;
  else if (tela === 'mesa') conteudo = <TelaMesa />;
  else if (tela === 'cardapio') conteudo = <TelaCardapio />;
  else conteudo = <TelaComanda />;

  return (
    <SafeAreaView style={styles.tela}>
      <StatusBar barStyle="light-content" backgroundColor="#1D1D1D" />
      {conteudo}
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  tela:  { flex: 1, backgroundColor: '#FAFAFA' },
  flex:  { flex: 1 },
  centro:{ flex: 1, justifyContent: 'center', alignItems: 'center', padding: 24 },

  logo:  { fontSize: 38, fontWeight: '800', color: '#E63946' },
  sub:   { fontSize: 16, color: '#666', marginBottom: 24, marginTop: 2 },
  ola:   { fontSize: 24, fontWeight: '700', color: '#1D1D1D' },

  card:  { width: '100%', maxWidth: 420, backgroundColor: '#FFF', borderRadius: 16, padding: 20,
           shadowColor: '#000', shadowOpacity: 0.08, shadowRadius: 12, shadowOffset: { width: 0, height: 4 }, elevation: 3 },
  label: { fontSize: 13, color: '#555', marginBottom: 6, marginTop: 10, fontWeight: '600' },
  input: { borderWidth: 1, borderColor: '#DDD', borderRadius: 10, paddingHorizontal: 14, paddingVertical: 12, fontSize: 16, backgroundColor: '#FFF' },
  inputMesa: { fontSize: 22, textAlign: 'center', fontWeight: '700' },

  botao: { backgroundColor: '#E63946', borderRadius: 10, paddingVertical: 14, alignItems: 'center', marginTop: 18 },
  botaoTexto: { color: '#FFF', fontSize: 16, fontWeight: '700' },
  botaoOff: { backgroundColor: '#C9ADB0' },
  botaoBarra: { flex: 1, marginTop: 0, marginLeft: 10 },
  botaoSec: { borderWidth: 1.5, borderColor: '#E63946', borderRadius: 10, paddingVertical: 13, paddingHorizontal: 16, alignItems: 'center' },
  botaoSecTexto: { color: '#E63946', fontSize: 15, fontWeight: '700' },
  linkSair: { color: '#999', marginTop: 24, fontSize: 15, textDecorationLine: 'underline' },

  header: { backgroundColor: '#1D1D1D', paddingHorizontal: 18, paddingVertical: 16, paddingTop: 20,
            flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center' },
  headerTitulo: { color: '#FFF', fontSize: 20, fontWeight: '800' },
  headerSub: { color: '#BBB', fontSize: 13, marginTop: 2 },
  headerLink: { color: '#FF8A94', fontSize: 14, fontWeight: '600' },

  categoria: { fontSize: 14, fontWeight: '800', color: '#E63946', textTransform: 'uppercase', marginBottom: 8, letterSpacing: 0.5 },
  produto: { flexDirection: 'row', alignItems: 'center', backgroundColor: '#FFF', borderRadius: 12, padding: 14, marginBottom: 8,
             borderWidth: 1, borderColor: '#EEE' },
  produtoNome: { fontSize: 15, fontWeight: '600', color: '#222' },
  produtoPreco: { fontSize: 13, color: '#777', marginTop: 2 },

  stepper: { flexDirection: 'row', alignItems: 'center' },
  stepBtn: { width: 34, height: 34, borderRadius: 17, backgroundColor: '#F0E0E1', justifyContent: 'center', alignItems: 'center' },
  stepBtnTexto: { fontSize: 20, fontWeight: '800', color: '#E63946' },
  stepQtd: { width: 34, textAlign: 'center', fontSize: 16, fontWeight: '700', color: '#222' },

  barra: { position: 'absolute', left: 0, right: 0, bottom: 0, flexDirection: 'row', alignItems: 'center',
           padding: 14, backgroundColor: '#FFF', borderTopWidth: 1, borderTopColor: '#EEE' },

  linhaComanda: { flexDirection: 'row', alignItems: 'center', paddingVertical: 10, borderBottomWidth: 1, borderBottomColor: '#EEE' },
  comandaQtd: { width: 38, fontSize: 15, fontWeight: '800', color: '#E63946' },
  comandaNome: { fontSize: 15, color: '#222' },
  comandaPreco: { fontSize: 15, fontWeight: '600', color: '#222' },

  barraTotal: { position: 'absolute', left: 0, right: 0, bottom: 0, backgroundColor: '#FFF', borderTopWidth: 1, borderTopColor: '#EEE', paddingTop: 12 },
  totalLinha: { flexDirection: 'row', justifyContent: 'space-between', paddingHorizontal: 18 },
  totalLabel: { fontSize: 16, color: '#555', fontWeight: '600' },
  totalValor: { fontSize: 24, fontWeight: '800', color: '#E63946' },
  obsCaixa: { textAlign: 'center', color: '#999', fontSize: 12, marginTop: 4, marginBottom: 8 },

  aviso: { textAlign: 'center', color: '#888', marginTop: 12, lineHeight: 20 },
});
