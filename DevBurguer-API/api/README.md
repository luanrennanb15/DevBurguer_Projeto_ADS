# API DevBurguer

API REST que conecta o site ao banco de dados do sistema desktop.

## O que ela faz

- Serve o cardápio (produtos) do banco para o site
- Recebe pedidos do site e grava no banco como `Status = 'Aguardando'`
- Esses pedidos aparecem no Kanban do desktop para aprovação

## Tecnologia

- Node.js + Express
- Pacote `mssql` para conectar no SQL Server

## Como rodar (primeira vez)

### 1. Instalar o Node.js
Baixe em https://nodejs.org (versão LTS). Confirme no terminal:
```
node --version
```

### 2. Instalar as dependências
Dentro da pasta da API:
```
npm install
```

### 3. Configurar a conexão com o banco
Copie o arquivo `.env.exemplo` para `.env`:
```
copy .env.exemplo .env      (Windows)
```
Abra o `.env` e ajuste `DB_SERVER`, `DB_USER`, `DB_PASSWORD` de acordo
com o seu SQL Server.

### 4. Rodar a API
```
npm start
```
Deve aparecer "API DevBurguer rodando na porta 3001".

### 5. Testar
Abra no navegador: http://localhost:3001
Deve mostrar um JSON com a lista de endpoints.

Teste o cardápio: http://localhost:3001/api/produtos

## Endpoints

| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/api/produtos` | Lista todos os produtos |
| GET | `/api/categorias` | Lista as categorias |
| POST | `/api/pedidos` | Cria um pedido (site) |
| GET | `/api/pedidos/:id/status` | Consulta status de um pedido |

## Exemplo de POST /api/pedidos

```json
{
  "cliente":     { "nome": "Joao Silva", "telefone": "15999998888" },
  "tipoEntrega": "Entrega",
  "endereco":    "Rua das Flores",
  "numero":      "123",
  "bairro":      "Centro",
  "troco":       50.00,
  "itens": [
    { "idProduto": 1, "quantidade": 2, "observacao": "sem cebola" },
    { "idProduto": 5, "quantidade": 1, "observacao": "" }
  ]
}
```

Resposta:
```json
{
  "idPedido": 42,
  "status": "Aguardando",
  "total": 76.00,
  "mensagem": "Pedido recebido! Aguardando confirmacao da lanchonete."
}
```

## Estrutura de pastas

```
api/
├── package.json
├── .env.exemplo        <- modelo de configuração
├── .gitignore
└── src/
    ├── server.js       <- ponto de entrada
    ├── config/
    │   └── categorias.js   <- tradução de categorias banco<->site
    ├── db/
    │   └── db.js           <- conexão com SQL Server
    └── rotas/
        ├── produtos.js     <- GET produtos e categorias
        └── pedidos.js      <- POST pedido, GET status
```

## Segurança implementada

- O preço dos itens é calculado **no servidor** (busca no banco).
  O site não consegue forjar preço.
- Pedidos entram como `Aguardando` — exigem aprovação manual no desktop.
- Tudo gravado em transação (ou grava o pedido inteiro, ou nada).
