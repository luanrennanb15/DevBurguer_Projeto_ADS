# DevBurguer — Mapa das Pastas e Deploy

## O que é cada pasta / arquivo

| Item | O que é | Vai pra onde? |
|------|---------|---------------|
| **DevBurguer/** | Sistema **desktop** em C# (WinForms, .NET). O painel de gestão que roda no PC da lanchonete. | Fica no PC (NÃO vai pra web) |
| **DevBurguer-API/** | A **API** (Node/Express). É a ponte entre o site e o banco de dados. | Render (nuvem) |
| **DevBurguerFront-main/** | O **site do cliente** (HTML/CSS/JS) — o original do seu projeto. | É a fonte; publica-se pela pasta `docs/` |
| **docs/** | **Cópia do site** já no formato que o GitHub Pages exige (`index.html` na raiz + `.nojekyll`). **É ESTA que o Pages publica.** | GitHub Pages |
| **Documentação/** | O trabalho escrito do PIM (o `.docx`). | — |
| **imagens_system/** | Prints do sistema, usados na documentação. | — |
| **README.md** | Descrição do projeto. É o que aparece na página do repositório no GitHub (e era o que aparecia no Pages por engano). | — |
| **deploy_schema_postgres.sql** | Cria as tabelas do banco no **Supabase** (PostgreSQL). | Rodar no Supabase |
| **deploy_schema_nuvem.sql** | Mesma coisa, versão SQL Server (reserva). | — |
| **popular_banco_demo.sql** | Dados de teste (SQL Server) para o desktop/apresentação. | SQL Server local |
| **pagamentos_motoboy_demo.sql** | Pagamentos de motoboy de teste (SQL Server). | SQL Server local |
| **DEPLOY.md** | Guia passo a passo pra subir tudo na internet. | — |
| **REFATORACAO_DevBurguer.md** | O plano/roadmap de melhorias do projeto. | — |
| **cardapio.txt / info api.txt** | Suas anotações. | — |

## Por que o Pages mostrava o README (e não o site)

O GitHub Pages publica a **raiz** do repositório. Na raiz NÃO existe um
`index.html` — existe o `README.md`. Sem `index.html`, o Pages pega o README e
mostra ele. O `index.html` do seu site está dentro de subpastas, não na raiz.

## O jeito CERTO de publicar o site (resolve o problema)

Já deixei a pasta **`docs/`** pronta (com `index.html` na raiz dela + `.nojekyll`).
Só falta:

1. Subir a pasta **`docs`** pro seu repositório no GitHub (commit + push).
2. No GitHub: **Settings → Pages → Source = Deploy from a branch** →
   escolha a sua **branch** → troque a pasta de **`/ (root)`** para **`/docs`** →
   **Save**.
3. Espere ~1 min e recarregue o link. Agora aparece o **site de verdade**.

> Observação: quando o site aparecer, o **cardápio virá vazio** até a API + o
> banco estarem no ar (o menu vem do banco). Isso é o próximo passo (ver DEPLOY.md).
