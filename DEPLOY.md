# DevBurguer — Guia de Deploy (100% grátis)

Passo a passo pra colocar o **site + API + banco** na internet sem pagar nada
e sem cartão. As três peças vão em serviços diferentes:

| Peça | Onde | Custo |
|------|------|-------|
| Banco de dados (PostgreSQL) | **Supabase** | grátis, sem cartão |
| API (Node/Express) | **Render** | grátis (dorme após 15 min ocioso) |
| Site (HTML/CSS/JS) | **GitHub Pages** | grátis |

> O **sistema desktop** NÃO vai pra internet — ele é app do Windows e continua
> rodando no PC. Ele fica fora deste guia (a união dele com a API é a Fase 3).

---

## Pré-requisito: subir o projeto pro GitHub

O Render e o Pages puxam o código do GitHub. Então, se ainda não fez:

1. Crie uma conta em github.com.
2. Crie um repositório (ex.: `devburguer`) e suba a pasta do projeto.
   - Confirme que o `DevBurguer-API/api/.env` **não** vai junto (já está no
     `.gitignore` — só o `.env.exemplo` sobe, e sem senha).

---

## Passo 1 — Banco de dados no Supabase

1. Acesse **supabase.com** e entre com a conta do GitHub (não pede cartão).
2. **New project**. Dê um nome, escolha a região mais próxima (South America),
   e defina uma **senha do banco** — **anote essa senha**.
3. Espere ~2 min o projeto subir.
4. No menu lateral, abra **SQL Editor** → **New query**.
5. Cole todo o conteúdo do arquivo **`deploy_schema_postgres.sql`** e clique
   em **Run**. Isso cria as tabelas.
6. (Recomendado) Insira alguns **produtos**, senão o site abre com o cardápio
   vazio. Rode um INSERT como este (ajuste os itens):
   ```sql
   INSERT INTO produtos (nome, preco, categoria, ingredientes, ativo) VALUES
   ('DevClassic', 25.00, 'Lanche Tradicional', 'Pao, carne, queijo', true),
   ('ByteBurger', 32.00, 'Lanche Gourmet', 'Pao brioche, blend, bacon', true),
   ('Coca-Cola',   7.00, 'Bebidas', '', true);
   ```
7. Pegue os dados de conexão: **Project Settings → Database → Connection info**.
   Você vai usar: **Host**, **Port** (5432), **User** (`postgres`),
   **Password** (a que você definiu) e **Database** (`postgres`).

---

## Passo 2 — API no Render

1. Acesse **render.com** e entre com o GitHub.
2. **New → Web Service** e conecte o seu repositório.
3. Configure:
   - **Root Directory:** `DevBurguer-API/api`
   - **Build Command:** `npm install`
   - **Start Command:** `npm start`
   - **Instance Type:** Free
4. Em **Environment**, adicione as variáveis (valores do Supabase):
   ```
   DB_SERVER   = (o Host do Supabase, ex.: db.xxxx.supabase.co)
   DB_DATABASE = postgres
   DB_USER     = postgres
   DB_PASSWORD = (a senha do banco)
   DB_PORT     = 5432
   DB_SSL      = true
   CORS_ORIGIN = *   (troca depois pela URL do site — Passo 4)
   ```
   > Não precisa definir `PORT` — o Render injeta sozinho, e o código já usa.
5. **Create Web Service** e espere o deploy.
6. Copie a **URL da API** (ex.: `https://devburguer-api.onrender.com`).
   Abra ela no navegador: deve aparecer um JSON com `"status": "online"`.

> No plano grátis, a API "dorme" após ~15 min sem uso. A primeira chamada
> depois disso demora ~30-50s pra acordar. Pra demo, abra o site 1 min antes.

---

## Passo 3 — Site no GitHub Pages

1. Edite **`DevBurguerFront-main/js/config.js`** e troque a linha do `baseUrl`
   pela URL da API do Render (com `/api` no final):
   ```js
   api: {
       baseUrl: 'https://devburguer-api.onrender.com/api',
   },
   ```
   Faça commit/push dessa mudança.
2. No GitHub, vá em **Settings → Pages**.
3. Em **Source**, escolha **Deploy from a branch**.
4. Selecione a branch (`main`) e a pasta do site. Se o Pages só deixar escolher
   `/` ou `/docs`, o mais simples é pôr o site num repositório próprio na raiz.
5. Salve. Em ~1 min o Pages te dá a **URL do site**
   (ex.: `https://seu-usuario.github.io/devburguer/`).

---

## Passo 4 — Ligar o CORS (importante)

Com a URL do site em mãos, volte no **Render → Environment** e troque:
```
CORS_ORIGIN = https://seu-usuario.github.io
```
Salve (o Render re-deploya). Assim a API aceita chamadas só do seu site.

---

## Passo 5 — Testar tudo

1. Abra a **URL do site** (Pages).
2. O cardápio deve carregar (vindo da API → Supabase).
3. Monte um pedido e finalize — deve gravar no banco (status "Aguardando").
4. Se o cardápio não aparecer: abra o **console do navegador (F12)** e veja se
   é erro de CORS (revê o Passo 4) ou a API "dormindo" (recarrega após ~40s).

---

## Pega-ratão

- **API HTTPS obrigatório:** o site (Pages) é HTTPS, então a API também precisa
  ser HTTPS. O Render já entrega HTTPS — só não use `localhost` no `config.js`.
- **Render dorme** (grátis): primeira chamada após ociosidade é lenta.
- **Supabase pausa** o projeto após ~1 semana sem uso — é só reativar no painel.
- **Senhas:** nunca commite o `.env` real. Só o `.env.exemplo` (sem senha).
