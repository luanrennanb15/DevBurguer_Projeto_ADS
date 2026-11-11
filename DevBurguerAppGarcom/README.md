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
│   ├── AdaptadorMesa.java       Liga a lista de Mesa ao GridView da tela de mesas
│   ├── AdaptadorProduto.java    Liga a lista de Produto ao ListView do cardápio
│   ├── Dinheiro.java            Formata valores em reais (vírgula decimal)
│   ├── modelo/
│   │   ├── Mesa.java            Classe de modelo: número e status da mesa
│   │   ├── Produto.java         Classe de modelo: item do cardápio (GET /api/produtos)
│   │   └── Categoria.java       Classe de modelo: categoria do cardápio (GET /api/categorias)
│   └── rede/
│       ├── ApiDevBurguer.java   Única classe que fala com a API (HTTP + timeout + JSON)
│       └── OuvinteApi.java      Callback de sucesso/erro de uma chamada à API
└── res/
    ├── layout/
    │   ├── activity_login.xml    Layout da tela de login
    │   ├── activity_mesas.xml    Layout da tela de mesas
    │   ├── item_mesa.xml         Layout de um card da grade de mesas
    │   ├── activity_cardapio.xml Layout da tela de cardápio (carregando/erro/conteúdo)
    │   └── item_produto.xml      Layout de uma linha de produto do cardápio
    ├── values/
    │   ├── strings.xml          Todos os textos da interface
    │   ├── colors.xml           Cores da identidade visual do DevBurguer
    │   └── themes.xml           Tema do app (cores aplicadas nos componentes)
    └── drawable/
        ├── ic_launcher_devburguer.xml   Ícone do app
        ├── ic_hamburguer.xml            Ícone usado no login
        ├── ic_seta_voltar.xml           Seta de "voltar" do cardápio
        ├── fundo_botao_pilula.xml       Fundo arredondado (pílula) dos botões
        ├── fundo_mesa_livre/ocupada/pronto.xml   Fundo de cada status de mesa
        └── fundo_chip_selecionado/normal.xml, fundo_botao_quadrado_contorno/vermelho.xml   Chips de categoria e contador +/- do cardápio
```

## Telas

| Tela | Activity | Status |
|---|---|---|
| Login do garçom | `LoginActivity` | Criada (mock — não valida senha real ainda) |
| Mesas | `MesasActivity` | Criada (mock — lista fixa de mesas) |
| Cardápio | `CardapioActivity` | Criada (API real: `GET /api/produtos` e `/api/categorias`) |
| Pedido (carrinho) | — | Não criada |
| Comanda da mesa | — | Não criada |

### Login do garçom (`LoginActivity`)

Primeira tela do app (é a `LAUNCHER` no `AndroidManifest.xml`). Visual baseado na prototipagem: ícone num círculo vermelho, título "DevBurguer", campos com rótulo e borda arredondada, botão "Entrar" em pílula.

- `configuracao()`: liga as variáveis Java aos componentes do layout (`findViewById`).
- `eventos()`: registra o clique do botão `btnEntrar`.
- **Mock**: como o login real do garçom ainda está em desenvolvimento na API (Luan), o clique só confere se os campos não estão vazios, mostra uma mensagem de boas-vindas e avança para a tela de Mesas (passando o usuário digitado como "nome do garçom"). Não valida usuário/senha de verdade e não chama a API ainda. Isso será trocado quando o contrato da API de login existir.

### Mesas (`MesasActivity`)

Mostra as mesas do salão numa grade de 3 colunas (`GridView`), com cabeçalho "Olá, {garçom}", botão "Sair", legenda de status e aviso no rodapé — seguindo a prototipagem.

- `configuracao()`: liga o `GridView`, lê o nome do garçom recebido por `Intent` e monta a lista de mesas.
- `eventos()`: registra o clique numa mesa e o clique no botão "Sair" (volta para o Login).
- **Status da mesa (3 tipos, cores da identidade visual)**: Livre (cinza), Ocupada (borda vermelha), Pedido pronto (fundo dourado).
- **Mock**: a lista de mesas é fixa no código (`MesasActivity.configuracao()`), porque a API de mesas ainda está em desenvolvimento pelo Luan. Ao tocar numa mesa, por enquanto só aparece um aviso dizendo o que aconteceria (mesa livre abriria o Cardápio, mesa ocupada/pronta abriria a Comanda) — essas telas ainda não existem.
- `AdaptadorMesa` é quem desenha cada card da grade (`item_mesa.xml`), lendo os dados da classe `Mesa` e escolhendo a cor/fundo conforme o status.

### Cardápio da mesa (`CardapioActivity`)

Primeira tela do app que fala com a **API real** (`GET /api/produtos` e `GET /api/categorias`, hospedadas no Render). Mostra chips de categoria (montados dinamicamente a partir da resposta da API) e, abaixo, a lista de produtos daquela categoria com botão "Adicionar" ou contador -/+ (o pedido fica guardado em memória, por mesa aberta).

- `configuracao()`: liga as views e lê o número da mesa recebido por `Intent`.
- `carregarCardapio()`: busca categorias e depois produtos na API, em outra thread (`ApiDevBurguer`). A tela tem 3 estados: **carregando** (enquanto espera a resposta), **erro** (sem internet, servidor fora, ou timeout — com botão "Tentar novamente") e **conteúdo** (lista carregada).
- Timeout de 60s, porque o plano gratuito do Render pode demorar até ~50s pra "acordar".
- `ApiDevBurguer` (pacote `rede`) é a única classe que conversa com a API: monta a `URL`, usa `HttpURLConnection` (nativo do Android, sem biblioteca nova), lê a resposta e transforma o JSON em `List<Produto>`/`List<Categoria>` usando `org.json` (também nativo).
- Tocar no rodapé "Ver pedido" ainda não faz nada de verdade — a tela de Carrinho é o próximo passo.

### Fonte

A identidade visual pede a fonte Poppins. Por ora as telas usam a fonte padrão do Android em negrito (visualmente parecida), para não depender de baixar fontes do Google em tempo de execução (risco na hora da apresentação sem internet). Se os arquivos `.ttf` da Poppins forem fornecidos, é simples trocar depois.

## Fluxo do app (planejado)

Login do garçom → Mesas → Cardápio → Pedido (carrinho) → Comanda da mesa.

Pagamento e fechamento da comanda são feitos no caixa (desktop), nunca neste app.
