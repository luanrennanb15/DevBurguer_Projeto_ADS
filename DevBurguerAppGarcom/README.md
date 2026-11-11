# DevBurguer App Garçom

App Android nativo (Java) para o garçom registrar pedidos por mesa, complementando o sistema DevBurguer. Trabalho de faculdade (ADS).

## Estrutura de pastas

```
app/src/main/
├── AndroidManifest.xml          Declara as Activities e o ícone/tema do app
├── java/com/example/devburguergarcom/
│   ├── LoginActivity.java       Tela de login do garçom (primeira tela do app)
│   ├── MesasActivity.java       Tela com a lista de mesas (mock)
│   ├── AdaptadorMesa.java       Liga a lista de Mesa ao ListView da tela de mesas
│   └── modelo/
│       └── Mesa.java            Classe de modelo: número e status da mesa
└── res/
    ├── layout/
    │   ├── activity_login.xml   Layout da tela de login
    │   ├── activity_mesas.xml   Layout da tela de mesas
    │   └── item_mesa.xml        Layout de uma linha da lista de mesas
    ├── values/
    │   ├── strings.xml          Todos os textos da interface
    │   ├── colors.xml           Cores da identidade visual do DevBurguer
    │   └── themes.xml           Tema do app (cores aplicadas nos componentes)
    └── drawable/
        ├── ic_launcher_devburguer.xml   Ícone do app
        └── fundo_botao_pilula.xml       Fundo arredondado (pílula) dos botões
```

## Telas

| Tela | Activity | Status |
|---|---|---|
| Login do garçom | `LoginActivity` | Criada (mock — não valida senha real ainda) |
| Mesas | `MesasActivity` | Criada (mock — lista fixa de mesas) |
| Cardápio | — | Não criada |
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

### Fonte

A identidade visual pede a fonte Poppins. Por ora as telas usam a fonte padrão do Android em negrito (visualmente parecida), para não depender de baixar fontes do Google em tempo de execução (risco na hora da apresentação sem internet). Se os arquivos `.ttf` da Poppins forem fornecidos, é simples trocar depois.

## Fluxo do app (planejado)

Login do garçom → Mesas → Cardápio → Pedido (carrinho) → Comanda da mesa.

Pagamento e fechamento da comanda são feitos no caixa (desktop), nunca neste app.
