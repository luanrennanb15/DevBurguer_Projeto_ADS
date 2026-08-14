# 🍔 DevBurguer

Sistema integrado de gestão para hamburgueria, desenvolvido como **Projeto Integrado
Multidisciplinar III (PIM III)** — Análise e Desenvolvimento de Sistemas / UNIP.

A solução é composta por **três aplicações que compartilham um único banco de dados
PostgreSQL na nuvem**: um sistema desktop para a operação interna, uma API REST de
integração e um site responsivo para o cliente final. Um pedido feito no site chega
**em tempo real** ao painel de produção do desktop, porque todos os módulos falam com
o mesmo banco.

## 🌐 No ar

| Módulo          | Endereço                                                        |
|-----------------|-----------------------------------------------------------------|
| Site (cliente)  | https://luanrennanb15.github.io/DevBurguer_Projeto_ADS/         |
| API REST        | https://devburguer-api-9gds.onrender.com/api                    |
| Banco de dados  | PostgreSQL — Supabase                                            |

## 🏗️ Arquitetura (visão geral)

```
   Cliente (navegador)                 Operação interna (loja)
          │                                     │
          ▼                                     ▼
   ┌──────────────┐   HTTP/JSON   ┌──────────────────────────┐
   │  Site (docs) │──────────────▶│      API REST (Node)     │
   └──────────────┘               └────────────┬─────────────┘
                                                │  SQL
   ┌────────────────────────┐   SQL (Npgsql)    ▼
   │  Desktop (C# WinForms)  │────────▶ ┌──────────────────┐
   └────────────────────────┘          │  PostgreSQL       │
                                        │  (Supabase)       │
                                        └──────────────────┘
```

Detalhes e decisões em **[ARQUITETURA.md](ARQUITETURA.md)**.

## 📁 Estrutura do repositório

```
DevBurguer/
├── docs/                 # 🌐 Site do cliente (HTML/CSS/JS) — publicado pelo GitHub Pages
├── DevBurguer-API/       # 🔌 API REST (Node.js + Express) — hospedada no Render
│   └── api/
├── DevBurguer-Desktop/   # 🖥️ Sistema desktop (C# WinForms / .NET Framework 4.8)
├── database/             # 🗄️ Scripts do banco (PostgreSQL)
│   ├── schema/           #     estrutura das tabelas
│   ├── seeds/            #     dados iniciais
│   └── migrations/       #     alterações posteriores
├── documentacao/         # 📄 PIM, portfólio, regras e fotos
│   ├── pim/
│   ├── portfolio/
│   ├── regras/
│   └── fotos/
├── assets/               # 🎨 Logo e imagens do sistema
└── README.md
```

> ℹ️ O site fica em `docs/` porque essa é a pasta que o **GitHub Pages** publica.
> A documentação do projeto (PIM, portfólio) está em `documentacao/`.

## 🛠️ Tecnologias

| Camada   | Stack                                                             |
|----------|-------------------------------------------------------------------|
| Desktop  | C# · WinForms · .NET Framework 4.8 · Npgsql                        |
| API      | Node.js · Express · node-postgres (pg)                            |
| Web      | HTML5 · CSS3 · JavaScript (vanilla) · VLibras (acessibilidade)     |
| Banco    | PostgreSQL (Supabase)                                             |
| Deploy   | GitHub Pages (site) · Render (API) · Supabase (banco)            |

## ▶️ Como rodar

### 1. Banco de dados
No seu PostgreSQL (Supabase → SQL Editor), rode nesta ordem:
1. `database/schema/01_schema_postgres.sql`
2. `database/seeds/*.sql`
3. `database/migrations/*.sql`

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

## 🗺️ Próximos passos (roadmap)

Evoluções planejadas para tornar o sistema mais **automatizado** e completo:

- **📱 App mobile de autoatendimento nas mesas** — *em desenvolvimento*
  Um aplicativo para **tablets, um em cada mesa**, integrado à mesma **API REST** do
  sistema. O próprio cliente monta e envia o pedido **direto da mesa**, sem precisar
  chamar um atendente nem se levantar. O pedido cai **em tempo real** no painel de
  produção da cozinha — exatamente como já acontece com o site. Isso reduz a
  necessidade de garçom, agiliza o atendimento e diminui filas.

- **💳 Pagamento online** (Pix / cartão) integrado a um gateway, para o cliente pagar direto pelo site/app.
- **🔔 Acompanhamento do pedido em tempo real** pelo cliente (recebido → em produção → pronto).
- **🏢 Suporte a múltiplas lojas** (multi-tenant), para usar o sistema em vários estabelecimentos.
- **🔄 Deploy automatizado (CI/CD)** e testes a cada mudança, para publicar sem risco.

## 👤 Autor
**Luan Rennan** — Análise e Desenvolvimento de Sistemas / UNIP
