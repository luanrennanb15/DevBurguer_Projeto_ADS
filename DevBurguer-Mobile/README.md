# DevBurguer Mobile

Aplicativo do cliente (React Native + Expo) que consome a mesma API REST do
site. Permite consultar o cardápio, montar o carrinho e enviar o pedido.

## O que você precisa

- **Node.js** (já instalado — é o mesmo da API)
- **Expo Go** no celular — baixe grátis na **Play Store** (Android) ou **App Store** (iPhone)

## Como rodar (passo a passo)

1. Abra o terminal **dentro desta pasta** (`DevBurguer-Mobile`):
   ```
   cd DevBurguer-Mobile
   ```
2. Instale as dependências (só na primeira vez):
   ```
   npm install
   ```
3. Inicie o app:
   ```
   npx expo start
   ```
4. Vai aparecer um **QR Code** no terminal.
5. Abra o **Expo Go** no celular e **escaneie o QR Code**
   (Android: pela câmera do próprio Expo Go / iPhone: pela câmera do celular).
6. O app abre no seu celular. Pronto! 🎉

> Dica: o celular e o computador precisam estar na **mesma rede Wi-Fi**.
> Se o celular não conectar, rode `npx expo start --tunnel`.

## Observações

- O app conversa com a API no Render:
  `https://devburguer-api-9gds.onrender.com/api`
- Como a API é de plano gratuito, o **primeiro acesso pode levar até 1 minuto**
  (ela "dorme" quando fica sem uso). Depois fica rápida.
- Os pedidos enviados pelo app entram no sistema com situação **"Aguardando"**,
  igual aos do site, e aparecem no painel de produção do desktop para aprovação.

## Estrutura

```
DevBurguer-Mobile/
├── App.js          <- todo o app (cardápio, carrinho, checkout)
├── app.json        <- configuração do Expo
├── index.js        <- ponto de entrada
├── package.json    <- dependências
└── assets/         <- ícones
```
