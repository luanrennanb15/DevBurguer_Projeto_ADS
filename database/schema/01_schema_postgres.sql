-- ============================================================================
-- DEVBURGUER — SCHEMA POSTGRESQL (Supabase / Neon / qualquer PostgreSQL)
-- ----------------------------------------------------------------------------
-- Estrutura base do banco. Nomes de coluna em minúsculo (convenção Postgres).
-- Ordem de execução recomendada:
--   1) database/schema/01_schema_postgres.sql   (este arquivo)
--   2) database/seeds/*.sql                      (dados)
--   3) database/migrations/*.sql                 (alterações posteriores)
-- ============================================================================

DROP TABLE IF EXISTS pagamentomotoboy, escalamotoboy, itenspedido, pedidos,
                     usuarios, produtos, motoboys, clientes, adicionais CASCADE;

CREATE TABLE adicionais (
    id     SERIAL PRIMARY KEY,
    nome   VARCHAR(100),
    preco  NUMERIC(10,2)
);

CREATE TABLE clientes (
    id        SERIAL PRIMARY KEY,
    nome      VARCHAR(100),
    telefone  VARCHAR(20),
    endereco  VARCHAR(200),
    cpf       VARCHAR(20),
    numero    VARCHAR(10),
    bairro    VARCHAR(100)
);

CREATE TABLE motoboys (
    id         SERIAL PRIMARY KEY,
    nome       VARCHAR(100),
    endereco   VARCHAR(200),
    telefone1  VARCHAR(20),
    telefone2  VARCHAR(20),
    cpf        VARCHAR(20),
    numero     VARCHAR(10),
    bairro     VARCHAR(100)
);

CREATE TABLE produtos (
    id           SERIAL PRIMARY KEY,
    nome         VARCHAR(100),
    preco        NUMERIC(10,2),
    categoria    VARCHAR(50),
    ingredientes VARCHAR(500),
    ativo        BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE TABLE usuarios (
    id       SERIAL PRIMARY KEY,
    usuario  VARCHAR(50),
    senha    VARCHAR(64) NOT NULL
);

CREATE TABLE pedidos (
    id             SERIAL PRIMARY KEY,
    idcliente      INT REFERENCES clientes(id),
    data           TIMESTAMP DEFAULT NOW(),
    total          NUMERIC(10,2),
    status         VARCHAR(20) NOT NULL DEFAULT 'Em Producao',
    tipoentrega    VARCHAR(10),
    idmotoboy      INT REFERENCES motoboys(id),
    trocopara      NUMERIC(10,2),
    formapagamento VARCHAR(30) DEFAULT '',
    origem         VARCHAR(20) NOT NULL DEFAULT 'Desktop'
);

CREATE TABLE itenspedido (
    id          SERIAL PRIMARY KEY,
    idpedido    INT REFERENCES pedidos(id),
    idproduto   INT REFERENCES produtos(id),
    quantidade  INT,
    observacao  VARCHAR(200),
    preco       NUMERIC(10,2),
    adicionais  VARCHAR(300)
);

CREATE TABLE escalamotoboy (
    id         SERIAL PRIMARY KEY,
    idmotoboy  INT NOT NULL REFERENCES motoboys(id),
    diasemana  INT NOT NULL,
    ativo      BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE TABLE pagamentomotoboy (
    id                  SERIAL PRIMARY KEY,
    idmotoboy           INT REFERENCES motoboys(id),
    quantidadeentregas  INT,
    valortotalentregas  NUMERIC(10,2),
    valorchegada        NUMERIC(10,2),
    totalpagar          NUMERIC(10,2),
    datapagamento       TIMESTAMP,
    comentario          VARCHAR(300)
);

CREATE INDEX ix_itenspedido_idpedido ON itenspedido(idpedido);
CREATE INDEX ix_pedidos_status ON pedidos(status);
