# DevBurguer App Garçom

App Android nativo (Java) para o garçom registrar pedidos por mesa, complementando o sistema DevBurguer. Trabalho de faculdade (ADS).

## Estrutura de pastas

```
app/src/main/
├── AndroidManifest.xml          Declara as Activities e o ícone/tema do app
├── java/com/example/devburguergarcom/
│   └── LoginActivity.java       Tela de login do garçom (primeira tela do app)
└── res/
    ├── layout/
    │   └── activity_login.xml   Layout da tela de login
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
| Mesas | — | Não criada |
| Cardápio | — | Não criada |
| Pedido (carrinho) | — | Não criada |
| Comanda da mesa | — | Não criada |

### Login do garçom (`LoginActivity`)

Primeira tela do app (é a `LAUNCHER` no `AndroidManifest.xml`). Tem dois campos (usuário e senha) e um botão "Entrar".

- `configuracao()`: liga as variáveis Java aos componentes do layout (`findViewById`).
- `eventos()`: registra o clique do botão `btnEntrar`.
- **Mock**: como o login real do garçom ainda está em desenvolvimento na API (Luan), o clique só confere se os campos não estão vazios e mostra uma mensagem de boas-vindas. Não valida usuário/senha de verdade e não chama a API ainda. Isso será trocado quando o contrato da API de login existir.

## Fluxo do app (planejado)

Login do garçom → Mesas → Cardápio → Pedido (carrinho) → Comanda da mesa.

Pagamento e fechamento da comanda são feitos no caixa (desktop), nunca neste app.
