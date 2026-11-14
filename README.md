# 🍔 DevBurguer

Sistema integrado de gestão para hamburgueria, desenvolvido como **Projeto Integrado
Multidisciplinar IV (PIM IV)** — Análise e Desenvolvimento de Sistemas / UNIP.

A solução é composta por **quatro aplicações que compartilham um único banco de dados
PostgreSQL na nuvem**: um sistema desktop para a operação interna, uma API REST de
integração, um site responsivo para o cliente final e um **aplicativo Android nativo
para o garçom** (atendimento no salão). Um pedido feito no site ou pelo app do garçom
chega **em tempo real** ao painel de produção do desktop, porque todos os módulos falam
com o mesmo banco.

## 🌐 No ar

| Módulo          | Endereço                                                        |
|-----------------|-----------------------------------------------------------------|
| Site (cliente)  | https://luanrennanb15.github.io/DevBurguer_Projeto_ADS/         |
| API REST        | https://devburguer-api-9gds.onrender.com/api                    |
| Banco de dados  | PostgreSQL — Supabase                                            |

> O app do garçom não fica "no ar": é instalado no aparelho Android (APK) e consome a API acima.

## 🏗️ Arquitetura (visão geral)

```
    Cliente (site)          Garçom (app Android)
          │                         │
          │  HTTP/JSON              │  HTTP/JSON
          ▼                         ▼
        ┌───────────────────────────────┐
        │         API REST (Node)        │
        └───────────────┬───────────────┘
                        │ SQL
                        ▼
  ┌───────────────────────────┐   SQL (Npgsql)   ┌───────────────────────────┐
  │   PostgreSQL (Supabase)    │◀─────────────────│  Desktop (C# WinForms)     │
  └───────────────────────────┘                  └───────────────────────────┘
```

O **site** e o **app do garçom** acessam os dados pela **API REST**; o **desktop** acessa
o banco diretamente (Npgsql). Todos compartilham o mesmo PostgreSQL, então um pedido feito
em qualquer canal aparece na mesma esteira de produção.

Detalhes e decisões em **[ARQUITETURA.md](ARQUITETURA.md)**.

## 📁 Estrutura do repositório

```
DevBurguer/
├── docs/                 # 🌐 Site do cliente (HTML/CSS/JS) — publicado pelo GitHub Pages
├── DevBurguer-API/       # 🔌 API REST (Node.js + Express) — hospedada no Render
│   └── api/
├── DevBurguer-Desktop/   # 🖥️ Sistema desktop (C# WinForms / .NET Framework 4.8)
├── DevBurguerAppGarcom/  # 📱 App do garçom (Android nativo, Java / Android Studio)
├── database/             # 🗄️ Scripts do banco (PostgreSQL)
│   ├── schema/           #     estrutura das tabelas
│   ├── seeds/            #     dados iniciais
│   ├── migrations/       #     alterações posteriores
│   ├── procedures/       #     funções/procedimentos
│   └── triggers/         #     gatilhos (auditoria/histórico)
├── documentacao/         # 📄 PIM, portfólio, regras e fotos
│   ├── pim/
│   ├── portfolio/
│   ├── regras/
│   └── fotos/
├── assets/               # 🎨 Logos do sistema
├── ARQUITETURA.md        # 📐 Decisões de arquitetura
├── render.yaml           # ⚙️ Configuração de deploy da API (Render)
└── README.md
```

> ℹ️ O site fica em `docs/` porque essa é a pasta que o **GitHub Pages** publica.
> A documentação do projeto (PIM, portfólio) está em `documentacao/`.

## 🛠️ Tecnologias

| Camada   | Stack                                                                |
|----------|----------------------------------------------------------------------|
| Desktop  | C# · WinForms · .NET Framework 4.8 · Npgsql                           |
| Mobile   | Java · Android nativo (Android Studio) · HttpURLConnection · org.json |
| API      | Node.js · Express · node-postgres (pg) · Helmet · CORS               |
| Web      | HTML5 · CSS3 · JavaScript (vanilla) · VLibras (acessibilidade)        |
| Banco    | PostgreSQL (Supabase)                                                 |
| Deploy   | GitHub Pages (site) · Render (API) · Supabase (banco)                |

## ▶️ Como rodar

### 1. Banco de dados
No seu PostgreSQL (Supabase → SQL Editor), rode nesta ordem:
1. `database/schema/01_schema_postgres.sql`
2. `database/seeds/*.sql`
3. `database/procedures/*.sql` e `database/triggers/*.sql`
4. `database/migrations/*.sql`

### 2. API (local)
```bash
cd DevBurguer-API/api
npm install
cp .env.exemplo .env      # preencha com os dados do seu Postgres
npm start
```

### 3. Desktop
Abra `DevBurguer-Desktop/DevBurguer.sln` no Visual Studio, deixe o NuGet restaurar o
pacote **Npgsql** e compile (Ctrl+Shift+B). A string de conexão fica em `config.txt`
(editável pela tela de Configurações).

### 4. Site
Abra `docs/index.html` no navegador — em produção é servido pelo GitHub Pages.

### 5. App do garçom (mobile)
Abra a pasta `DevBurguerAppGarcom/` no **Android Studio**, deixe o Gradle sincronizar e
execute (▶) em um emulador ou em um aparelho Android real (Android 8.0+). O app consome
a mesma **API REST** — não precisa de configuração extra.

## 🔐 Perfis de acesso

O sistema tem três perfis de usuário, todos na tabela `usuarios`:

- **Administrador** — acesso total; gerencia usuários e redefine senhas.
- **Operador** — acesso às telas operacionais (sem as áreas administrativas).
- **Garçom** — acessa **somente** o app Android; é bloqueado no desktop.

As senhas são guardadas com hash **PBKDF2 + salt**.

## 🗺️ Próximos passos (roadmap)

Evoluções planejadas para tornar o sistema mais **automatizado** e completo:

- **📱 Autoatendimento do cliente nas mesas** — *planejado*
  Um app (ou página) para **tablets, um em cada mesa**, em que o **próprio cliente** monta
  e envia o pedido, sem chamar atendente. É diferente do app do garçom (que já existe): aqui
  quem usa é o cliente. O pedido cairia **em tempo real** no painel de produção, como já
  acontece com o site.

- **💳 Pagamento online** (Pix / cartão) integrado a um gateway, para o cliente pagar direto pelo site/app.
- **🔔 Acompanhamento do pedido em tempo real** pelo cliente (recebido → em produção → pronto).
- **🏢 Suporte a múltiplas lojas** (multi-tenant), para usar o sistema em vários estabelecimentos.
- **🔄 Deploy automatizado (CI/CD)** e testes a cada mudança, para publicar sem risco.

## 👥 Autores

- **Luan Rennan** — sistema desktop, API, site e banco de dados
- **Mauricio Reis** — aplicativo do garçom (Android nativo)

Análise e Desenvolvimento de Sistemas — UNIP.
