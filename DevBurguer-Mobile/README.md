# DevBurguer — App do Garçom

Aplicativo de **salão** (React Native + Expo) para o garçom lançar os pedidos
mesa a mesa. Consome a mesma API REST do site.

**Fluxo:** Login → escolher a mesa → cardápio (montar a rodada) → enviar →
ver a comanda da mesa (total para pagar no caixa).

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

## Login de teste

- **Usuário:** `garcom`
- **Senha:** `devburguer`

> Esse garçom é criado pela migration `database/migrations/2026-11-11_garcom_auth.sql`.
> Rode essa migration no Supabase antes de testar o login.

## Observações

- O app conversa com a API no Render:
  `https://devburguer-api-9gds.onrender.com/api`
- Como a API é de plano gratuito, o **primeiro acesso pode levar até 1 minuto**
  (ela "dorme" quando fica sem uso). Depois fica rápida.
- As rodadas enviadas entram como `origem = Garçom`, `tipoentrega = Salão` e
  já vão direto para **"Em Produção"** (sem aprovação), aparecendo no painel do
  desktop. O pagamento é sempre no **caixa**.

## Estrutura

```
DevBurguer-Mobile/
├── App.js          <- todo o app (login, mesa, cardápio, rodada, comanda)
├── app.json        <- configuração do Expo
├── index.js        <- ponto de entrada
├── package.json    <- dependências
└── assets/         <- ícones
```

## Telas (Etapa 5 — Mobile)

| Tela      | O que faz                                              |
|-----------|--------------------------------------------------------|
| Login     | Autenticação do garçom (`POST /api/garcom/login`)      |
| Mesa      | Escolhe o número da mesa a atender                     |
| Cardápio  | Lista produtos (`GET /api/produtos`) e monta a rodada  |
| Envio     | Envia a rodada (`POST /api/pedidos/garcom`)            |
| Comanda   | Mostra os itens e o total da mesa (`GET /api/mesas/:mesa/comanda`) |
