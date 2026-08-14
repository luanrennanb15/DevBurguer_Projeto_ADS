# Banco de dados (PostgreSQL)

Rode os scripts nesta ordem no seu Postgres (Supabase → SQL Editor):

1. **`schema/`** — cria as tabelas (estrutura).
2. **`seeds/`** — insere dados iniciais (ex.: adicionais).
3. **`migrations/`** — alterações posteriores ao schema, em ordem de data.

Tudo é idempotente/re-executável quando possível (`IF NOT EXISTS`, `WHERE NOT EXISTS`).
