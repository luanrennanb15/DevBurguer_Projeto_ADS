# DevBurguer App Garçom

App Android nativo (Java) para o garçom registrar pedidos por mesa, complementando o sistema DevBurguer. Trabalho de faculdade (ADS).

## Estrutura de pastas

```
app/src/main/
├── AndroidManifest.xml          Declara as Activities, permissão de internet e o ícone/tema do app
├── java/com/example/devburguergarcom/
│   ├── LoginActivity.java       Tela de login do garçom (primeira tela do app)
│   ├── MesasActivity.java       Tela com a grade de mesas (mock)
│   ├── CardapioActivity.java    Tela do cardápio da mesa (API real)
│   ├── CarrinhoActivity.java    Tela do pedido (carrinho) montado na mesa
│   ├── ComandaActivity.java     Tela da comanda da mesa (rodadas enviadas)
│   ├── AdaptadorMesa.java       Liga a lista de Mesa ao GridView da tela de mesas
│   ├── AdaptadorProduto.java    Liga a lista de Produto ao ListView do cardápio
│   ├── AdaptadorCarrinho.java   Liga a lista de ItemPedido ao ListView do carrinho
│   ├── AdaptadorRodada.java     Liga a lista de Rodada ao ListView da comanda
│   ├── Dinheiro.java            Formata valores em reais (vírgula decimal)
│   ├── modelo/
│   │   ├── Mesa.java                Classe de modelo: número e status da mesa
│   │   ├── Produto.java             Classe de modelo: item do cardápio (GET /api/produtos)
│   │   ├── Categoria.java           Classe de modelo: categoria do cardápio (GET /api/categorias)
│   │   ├── Garcom.java              Classe de modelo: garçom logado (id, nome, usuário)
│   │   ├── Adicional.java           Classe de modelo: um adicional (nome + preço)
│   │   ├── CatalogoAdicionais.java  Lista fixa de adicionais (a API não tem endpoint pra isso)
│   │   ├── ItemPedido.java          Classe de modelo: um produto + quantidade + observação + adicionais no pedido
│   │   └── Rodada.java              Classe de modelo: uma rodada de itens já enviada (número + status + itens)
│   └── rede/
│       ├── ApiDevBurguer.java   Única classe que fala com a API (HTTP + timeout + JSON)
│       └── OuvinteApi.java      Callback de sucesso/erro de uma chamada à API
└── res/
    ├── layout/
    │   ├── activity_login.xml    Layout da tela de login
    │   ├── activity_mesas.xml    Layout da tela de mesas
    │   ├── item_mesa.xml         Layout de um card da grade de mesas
    │   ├── activity_cardapio.xml Layout da tela de cardápio (carregando/erro/conteúdo)
    │   ├── item_produto.xml      Layout de uma linha de produto do cardápio
    │   ├── activity_carrinho.xml Layout da tela de carrinho (pedido)
    │   ├── item_carrinho.xml     Layout de um card de item do carrinho
    │   ├── activity_comanda.xml  Layout da tela de comanda (lista de rodadas)
    │   └── item_rodada.xml       Layout de um card de rodada da comanda
    ├── values/
    │   ├── strings.xml          Todos os textos da interface
    │   ├── colors.xml           Cores da identidade visual do DevBurguer
    │   └── themes.xml           Tema do app (cores aplicadas nos componentes)
    └── drawable/
        ├── ic_launcher_devburguer.xml   Ícone do app
        ├── ic_hamburguer.xml            Ícone usado no login
        ├── ic_seta_voltar.xml           Seta de "voltar" do cardápio
        ├── fundo_botao_pilula.xml       Fundo arredondado (pílula) dos botões
        ├── fundo_mesa_livre/ocupada.xml   Fundo de cada status de mesa
        └── fundo_chip_selecionado/normal.xml, fundo_botao_quadrado_contorno/vermelho.xml   Chips de categoria e contador +/- do cardápio
```

## Telas

Todas as telas já falam com a API real (nenhuma usa mais dados fictícios para o fluxo principal — só os adicionais, que a API não expõe).

| Tela | Activity | Status |
|---|---|---|
| Login do garçom | `LoginActivity` | API real: `POST /api/garcom/login` |
| Mesas | `MesasActivity` | API real: `GET /api/mesas/ocupadas` (as 12 mesas em si são fixas no app; só o status livre/ocupada vem da API) |
| Cardápio | `CardapioActivity` | API real: `GET /api/produtos` e `GET /api/categorias` |
| Pedido (carrinho) | `CarrinhoActivity` | API real: `POST /api/pedidos/garcom` |
| Comanda da mesa | `ComandaActivity` | API real: `GET /api/mesas/:mesa/comanda` |

### Login do garçom (`LoginActivity`)

Primeira tela do app (é a `LAUNCHER` no `AndroidManifest.xml`). Visual baseado na prototipagem: ícone num círculo vermelho, título "DevBurguer", campos com rótulo e borda arredondada, botão "Entrar" em pílula.

- `configuracao()`: liga as variáveis Java aos componentes do layout (`findViewById`).
- `eventos()`: registra o clique do botão `btnEntrar`, que chama `ApiDevBurguer.loginGarcom()` (`POST /api/garcom/login`).
- O nome do garçom devolvido pela API é salvo num `SharedPreferences` (`LoginActivity.PREFS_SESSAO`), não só passado por `Intent`: assim ele continua disponível mesmo quando a tela de Mesas é recriada do zero por outra Activity (ex.: ao voltar da Comanda).
- Botão "Sair" (na tela de Mesas) limpa esse `SharedPreferences` e volta pro Login.

### Mesas (`MesasActivity`)

Mostra as mesas do salão numa grade de 3 colunas (`GridView`), com cabeçalho "Olá, {garçom}", botão "Sair", legenda de status e aviso no rodapé — seguindo a prototipagem.

- `configuracao()`: liga o `GridView`, lê o nome do garçom do `SharedPreferences` e monta as 12 mesas (fixas) como "Livre".
- `onResume()`: chama `ApiDevBurguer.buscarMesasOcupadas()` (`GET /api/mesas/ocupadas`) e atualiza o status de cada mesa pra "Ocupada" conforme a resposta. Roda de novo toda vez que a tela volta a ficar visível, pra mostrar o status atualizado.
- `eventos()`: clique numa mesa livre abre o Cardápio; numa mesa ocupada, abre a Comanda. Clique em "Sair" confirma e volta pro Login.
- **Status da mesa**: Livre (cinza) e Ocupada (borda vermelha) — vêm só desses dois valores que a API devolve.
- `AdaptadorMesa` é quem desenha cada card da grade (`item_mesa.xml`), lendo os dados da classe `Mesa` e escolhendo a cor/fundo conforme o status.

### Cardápio da mesa (`CardapioActivity`)

Mostra chips de categoria (montados dinamicamente a partir da resposta da API) e, abaixo, a lista de produtos daquela categoria com botão "Adicionar" ou contador -/+ (o pedido fica guardado em memória, por mesa aberta).

- `configuracao()`: liga as views e lê o número da mesa recebido por `Intent`.
- `carregarCardapio()`: busca categorias e depois produtos na API, em outra thread (`ApiDevBurguer`). A tela tem 3 estados: **carregando** (enquanto espera a resposta), **erro** (sem internet, servidor fora, ou timeout — com botão "Tentar novamente") e **conteúdo** (lista carregada).
- Timeout de 60s, porque o plano gratuito do Render pode demorar até ~50s pra "acordar".
- `MODO_TESTE_OFFLINE` em `ApiDevBurguer.java` está em `false` (API real ligada). Só mude pra `true` se precisar testar as telas sem internet — isso faz a classe devolver produtos/categorias fictícios.
- `ApiDevBurguer` (pacote `rede`) é a única classe que conversa com a API: monta a `URL`, usa `HttpURLConnection` (nativo do Android, sem biblioteca nova), lê a resposta e transforma o JSON em `List<Produto>`/`List<Categoria>` usando `org.json` (também nativo).
- Ao tocar no rodapé "Ver pedido", abre o Carrinho com os itens marcados (`startActivityForResult`); ao voltar do Carrinho, o Cardápio recebe a lista atualizada (quantidade, observação, adicionais) e não perde o que já tinha.

### Carrinho / pedido (`CarrinhoActivity`)

Mostra os itens escolhidos no Cardápio (recebidos por `Intent` como `ArrayList<ItemPedido>`), cada um com campo de observação, adicionais e contador -/+. Rodapé com total e botão "Enviar rodada".

- `configuracao()`: lê a mesa e a lista de itens recebida por `Intent`, monta o adaptador e o rodapé.
- `eventos()`: registra o clique de "voltar" (devolve a lista de itens pro Cardápio, pra não perder observação/adicionais já preenchidos) e do botão de enviar.
- Diminuir a quantidade de um item até zero **remove** ele do carrinho (`AdaptadorCarrinho.remove`).
- Botão "Enviar rodada" chama `ApiDevBurguer.enviarRodadaGarcom()` (`POST /api/pedidos/garcom`): envia a mesa e os itens (produto, quantidade, observação, adicionais pelo nome); o preço é sempre recalculado pelo servidor. Em caso de sucesso, limpa o carrinho e abre a Comanda da mesa.

### Comanda da mesa (`ComandaActivity`)

Mostra as rodadas já enviadas daquela mesa: cada card tem o número da rodada, um selo de status e a lista de itens com subtotal. No rodapé, o total da comanda, o botão "Adicionar itens" (volta pro Cardápio pra montar uma nova rodada) e o aviso de que pagamento é só no caixa.

- Chega a essa tela de dois jeitos: tocando numa mesa ocupada na tela de Mesas, ou enviando uma rodada pelo Carrinho.
- `carregarComanda()` chama `ApiDevBurguer.buscarComanda()` (`GET /api/mesas/:mesa/comanda`): a API devolve os itens em lista plana, com o `idPedido` de cada um; o app agrupa por `idPedido` e numera as rodadas pela ordem de chegada (1, 2, 3...), não pelo id bruto do banco.
- Se a comanda vier vazia (mesa foi fechada no caixa entre o garçom tocar nela e a tela carregar), mostra o aviso de mesa liberada em vez de ficar com a lista zerada sem explicação.
- `AdaptadorRodada` desenha cada card e, dentro dele, monta as linhas de item dinamicamente (porque a quantidade de itens varia por rodada).

### Fonte

A identidade visual pede a fonte Poppins. Por ora as telas usam a fonte padrão do Android em negrito (visualmente parecida), para não depender de baixar fontes do Google em tempo de execução (risco na hora da apresentação sem internet). Se os arquivos `.ttf` da Poppins forem fornecidos, é simples trocar depois.

## Fluxo do app

Login do garçom → Mesas → Cardápio → Pedido (carrinho) → Comanda da mesa.

Pagamento e fechamento da comanda são feitos no caixa (desktop), nunca neste app.
