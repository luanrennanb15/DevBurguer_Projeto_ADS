/* ============================================================================
   DEVBURGUER — POPULAR BANCO PARA APRESENTAÇÃO (DADOS DE DEMONSTRAÇÃO)
   ----------------------------------------------------------------------------
   Gera vendas dos ÚLTIMOS 30 DIAS (incluindo hoje), de 20 a 30 por dia,
   misturando ENTREGA e RETIRADA, com motoboys, escala e pagamentos.

   COMO USAR:
     1. ABRA O SSMS e conecte no seu SQL Server (localhost).
     2. (RECOMENDADO) Faça um backup antes — veja o comando logo abaixo.
     3. Confirme o nome do banco na linha "USE" (padrão: DevBurguerDB).
     4. Aperte F5 (executar). No fim aparece um resumo do que foi criado.

   BACKUP RÁPIDO (rode antes, ajustando a pasta):
     BACKUP DATABASE DevBurguerDB
     TO DISK = 'C:\temp\DevBurguerDB_antes_demo.bak';

   OBS: o script é ADITIVO (só insere). Ele não apaga seus pedidos reais.
   ============================================================================ */

USE DevBurguerDB;   -- << troque aqui se o seu banco tiver outro nome
SET NOCOUNT ON;

/* ---------------------------------------------------------------------------
   0) PRÉ-REQUISITO: precisa existir cardápio (Produtos ativos)
   --------------------------------------------------------------------------- */
IF (SELECT COUNT(*) FROM Produtos WHERE Ativo = 1) = 0
BEGIN
    RAISERROR('Nao ha produtos ativos cadastrados. Cadastre o cardapio antes de rodar este script.', 16, 1);
    RETURN;
END

/* ---------------------------------------------------------------------------
   1) MOTOBOYS — insere alguns de demonstração se houver poucos
   --------------------------------------------------------------------------- */
IF (SELECT COUNT(*) FROM Motoboys) < 5
BEGIN
    INSERT INTO Motoboys (Nome, Endereco, Telefone1, Telefone2, CPF, Numero, Bairro) VALUES
    ('Carlos Andrade',  'Rua das Acacias',   '(15) 99100-0001', '', '111.111.111-11', '10', 'Centro'),
    ('Bruno Martins',   'Av. Brasil',        '(15) 99100-0002', '', '222.222.222-22', '20', 'Vila Nova'),
    ('Diego Ferreira',  'Rua Sao Paulo',     '(15) 99100-0003', '', '333.333.333-33', '30', 'Jardim Europa'),
    ('Felipe Souza',    'Rua das Palmeiras', '(15) 99100-0004', '', '444.444.444-44', '40', 'Centro'),
    ('Rafael Lima',     'Av. Santos Dumont', '(15) 99100-0005', '', '555.555.555-55', '50', 'Sao Bento');
END

/* ---------------------------------------------------------------------------
   2) CLIENTES — insere alguns de demonstração se houver poucos
   --------------------------------------------------------------------------- */
IF (SELECT COUNT(*) FROM Clientes) < 8
BEGIN
    INSERT INTO Clientes (Nome, Telefone, Endereco, CPF, Numero, Bairro) VALUES
    ('Mariana Souza',    '(15) 98800-0001', 'Rua das Flores',     '101.101.101-01', '100', 'Centro'),
    ('Joao Pereira',     '(15) 98800-0002', 'Av. Paulista',       '102.102.102-02', '250', 'Jardim'),
    ('Ana Carolina',     '(15) 98800-0003', 'Rua XV de Novembro', '103.103.103-03', '32',  'Vila Nova'),
    ('Pedro Henrique',   '(15) 98800-0004', 'Rua do Comercio',    '104.104.104-04', '88',  'Centro'),
    ('Beatriz Almeida',  '(15) 98800-0005', 'Av. Ipiranga',       '105.105.105-05', '410', 'Sao Bento'),
    ('Lucas Oliveira',   '(15) 98800-0006', 'Rua das Laranjeiras','106.106.106-06', '17',  'Jardim Europa'),
    ('Camila Rodrigues', '(15) 98800-0007', 'Rua Tiradentes',     '107.107.107-07', '205', 'Centro'),
    ('Gabriel Santos',   '(15) 98800-0008', 'Av. das Nacoes',     '108.108.108-08', '900', 'Vila Nova'),
    ('Juliana Costa',    '(15) 98800-0009', 'Rua Marechal',       '109.109.109-09', '64',  'Jardim'),
    ('Rodrigo Alves',    '(15) 98800-0010', 'Rua Bahia',          '110.110.110-10', '143', 'Centro');
END

/* ---------------------------------------------------------------------------
   3) ESCALA DE MOTOBOYS — preenche se estiver vazia (alimenta o card do dashboard)
      Cada motoboy trabalha em ~5 dias da semana (DiaSemana: 1=Seg ... 7=Dom)
   --------------------------------------------------------------------------- */
IF (SELECT COUNT(*) FROM EscalaMotoboy WHERE Ativo = 1) = 0
BEGIN
    INSERT INTO EscalaMotoboy (IdMotoboy, DiaSemana, Ativo)
    SELECT m.Id, d.Dia, 1
    FROM Motoboys m
    CROSS JOIN (VALUES (1),(2),(3),(4),(5),(6),(7)) AS d(Dia)
    WHERE ABS(CHECKSUM(NEWID())) % 100 < 70;   -- ~70% de chance por dia
END

/* ---------------------------------------------------------------------------
   4) VENDAS DOS ÚLTIMOS 30 DIAS
   --------------------------------------------------------------------------- */
DECLARE @dia INT = 0;
DECLARE @baseData DATE;
DECLARE @qtdDia INT, @i INT;
DECLARE @idPedido INT, @idCliente INT, @idMotoboy INT;
DECLARE @tipo NVARCHAR(10), @origem NVARCHAR(10), @status NVARCHAR(20);
DECLARE @dataHora DATETIME;
DECLARE @numItens INT, @j INT, @idProduto INT, @qtdItem INT, @preco DECIMAL(18,2);
DECLARE @total DECIMAL(18,2), @taxa DECIMAL(18,2), @troco DECIMAL(10,2);
DECLARE @r INT;
DECLARE @k INT;

BEGIN TRY
    BEGIN TRAN;

    WHILE @dia < 30
    BEGIN
        SET @baseData = DATEADD(DAY, -@dia, CAST(GETDATE() AS DATE));
        -- Tendência de CRESCIMENTO: dias mais antigos vendem menos, dias mais
        -- recentes vendem mais. Assim a previsão por regressão linear mostra
        -- uma reta de alta. (@dia=29 é o mais antigo; @dia=0 é hoje.)
        SET @qtdDia = 16 + ((29 - @dia) * 45 / 100) + (ABS(CHECKSUM(NEWID())) % 5);
        SET @i = 0;

        WHILE @i < @qtdDia
        BEGIN
            -- cliente aleatório
            SELECT TOP 1 @idCliente = Id FROM Clientes ORDER BY NEWID();

            -- tipo de atendimento (60% entrega)
            SET @tipo = CASE WHEN ABS(CHECKSUM(NEWID())) % 100 < 60 THEN 'Entrega' ELSE 'Retirada' END;

            -- origem (35% site; o restante 'Desktop', igual ao default do banco)
            SET @origem = CASE WHEN ABS(CHECKSUM(NEWID())) % 100 < 35 THEN 'Site' ELSE 'Desktop' END;

            -- status: dias passados quase tudo finalizado; HOJE tem mistura (pro Kanban)
            IF @dia = 0
            BEGIN
                SET @r = ABS(CHECKSUM(NEWID())) % 100;
                SET @status = CASE
                    WHEN @r < 55 THEN 'Finalizado'
                    WHEN @r < 70 THEN 'Em Producao'
                    WHEN @r < 83 THEN 'Pronto'
                    WHEN @r < 94 THEN 'A Caminho'
                    ELSE 'Cancelado' END;
            END
            ELSE
            BEGIN
                SET @status = CASE WHEN ABS(CHECKSUM(NEWID())) % 100 < 92 THEN 'Finalizado' ELSE 'Cancelado' END;
            END

            -- data/hora
            IF @dia = 0
                SET @dataHora = DATEADD(MINUTE, -(ABS(CHECKSUM(NEWID())) % 600), GETDATE());  -- hoje, no passado
            ELSE
                SET @dataHora = DATEADD(MINUTE, 660 + ABS(CHECKSUM(NEWID())) % 720, CAST(@baseData AS DATETIME)); -- 11h-23h

            -- motoboy (só entrega)
            SET @idMotoboy = NULL;
            IF @tipo = 'Entrega'
                SELECT TOP 1 @idMotoboy = Id FROM Motoboys ORDER BY NEWID();

            SET @taxa = CASE WHEN @tipo = 'Entrega' THEN 6.00 ELSE 0 END;

            -- cria o pedido (total provisório = 0)
            INSERT INTO Pedidos (IdCliente, Data, Total, Status, TipoEntrega, TrocoPara, Origem, IdMotoboy)
            VALUES (@idCliente, @dataHora, 0, @status, @tipo, 0, @origem, @idMotoboy);
            SET @idPedido = CAST(SCOPE_IDENTITY() AS INT);

            -- itens (1 a 4 produtos)
            SET @numItens = 1 + ABS(CHECKSUM(NEWID())) % 4;
            SET @j = 0;
            WHILE @j < @numItens
            BEGIN
                SELECT TOP 1 @idProduto = Id, @preco = Preco
                FROM Produtos WHERE Ativo = 1 ORDER BY NEWID();

                SET @qtdItem = 1 + ABS(CHECKSUM(NEWID())) % 3;   -- 1 a 3

                INSERT INTO ItensPedido (IdPedido, IdProduto, Quantidade, Observacao, Adicionais, Preco)
                VALUES (@idPedido, @idProduto, @qtdItem, '', '', @preco);

                SET @j = @j + 1;
            END

            -- total = soma dos itens + taxa
            SELECT @total = ISNULL(SUM(Quantidade * Preco), 0) FROM ItensPedido WHERE IdPedido = @idPedido;
            SET @total = @total + @taxa;

            -- troco em ~metade das entregas
            SET @troco = 0;
            IF @tipo = 'Entrega' AND ABS(CHECKSUM(NEWID())) % 2 = 0
                SET @troco = CEILING((@total + 10) / 50.0) * 50;

            UPDATE Pedidos SET Total = @total, TrocoPara = @troco WHERE Id = @idPedido;

            SET @i = @i + 1;
        END

        SET @dia = @dia + 1;
    END

    /* -----------------------------------------------------------------------
       4.1) PEDIDOS DO SITE "AGUARDANDO" — 2 pedidos pendentes de aprovação,
            para demonstrar AO VIVO o alerta sonoro e a aceitação no Kanban.
       ----------------------------------------------------------------------- */
    SET @k = 0;
    WHILE @k < 2
    BEGIN
        SELECT TOP 1 @idCliente = Id FROM Clientes ORDER BY NEWID();

        INSERT INTO Pedidos (IdCliente, Data, Total, Status, TipoEntrega, TrocoPara, Origem, IdMotoboy)
        VALUES (@idCliente,
                DATEADD(MINUTE, -(ABS(CHECKSUM(NEWID())) % 15), GETDATE()),
                0, 'Aguardando', 'Entrega', 0, 'Site', NULL);
        SET @idPedido = CAST(SCOPE_IDENTITY() AS INT);

        SET @numItens = 2 + ABS(CHECKSUM(NEWID())) % 2;   -- 2 a 3 itens
        SET @j = 0;
        WHILE @j < @numItens
        BEGIN
            SELECT TOP 1 @idProduto = Id, @preco = Preco
            FROM Produtos WHERE Ativo = 1 ORDER BY NEWID();
            SET @qtdItem = 1 + ABS(CHECKSUM(NEWID())) % 2;
            INSERT INTO ItensPedido (IdPedido, IdProduto, Quantidade, Observacao, Adicionais, Preco)
            VALUES (@idPedido, @idProduto, @qtdItem, '', '', @preco);
            SET @j = @j + 1;
        END

        SELECT @total = ISNULL(SUM(Quantidade * Preco), 0) FROM ItensPedido WHERE IdPedido = @idPedido;
        SET @total = @total + 6.00;   -- taxa de entrega
        UPDATE Pedidos SET Total = @total WHERE Id = @idPedido;

        SET @k = @k + 1;
    END

    /* -----------------------------------------------------------------------
       5) PAGAMENTOS DE MOTOBOY — gera pagamentos semanais com base nas
          entregas reais que acabamos de criar (alimenta faturamento motoboy)
       ----------------------------------------------------------------------- */
    IF (SELECT COUNT(*) FROM PagamentoMotoboy) = 0
    BEGIN
        INSERT INTO PagamentoMotoboy
            (IdMotoboy, QuantidadeEntregas, ValorTotalEntregas, ValorChegada, TotalPagar, DataPagamento, Comentario)
        SELECT
            p.IdMotoboy,
            COUNT(*)                       AS QuantidadeEntregas,
            COUNT(*) * 6.00                AS ValorTotalEntregas,
            70.00                          AS ValorChegada,
            COUNT(*) * 6.00 + 70.00        AS TotalPagar,
            MAX(p.Data)                    AS DataPagamento,
            'Pagamento semanal (demo)'     AS Comentario
        FROM Pedidos p
        WHERE p.TipoEntrega = 'Entrega'
          AND p.IdMotoboy IS NOT NULL
          AND p.Status = 'Finalizado'
        GROUP BY p.IdMotoboy, DATEPART(WEEK, p.Data);
    END

    COMMIT TRAN;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRAN;
    DECLARE @msg NVARCHAR(2000) = ERROR_MESSAGE();
    RAISERROR('Erro ao popular o banco: %s', 16, 1, @msg);
    RETURN;
END CATCH

/* ---------------------------------------------------------------------------
   6) RESUMO DO QUE FOI CRIADO
   --------------------------------------------------------------------------- */
PRINT '====================================================';
PRINT ' DADOS DE DEMONSTRACAO CRIADOS COM SUCESSO';
PRINT '====================================================';

SELECT 'Pedidos (ultimos 30 dias)' AS Indicador,
       COUNT(*) AS Total
FROM Pedidos
WHERE Data >= DATEADD(DAY, -30, CAST(GETDATE() AS DATE));

SELECT CONVERT(date, Data) AS Dia,
       COUNT(*)            AS Pedidos,
       SUM(CASE WHEN Status = 'Finalizado' THEN Total ELSE 0 END) AS FaturamentoFinalizado
FROM Pedidos
WHERE Data >= DATEADD(DAY, -30, CAST(GETDATE() AS DATE))
GROUP BY CONVERT(date, Data)
ORDER BY Dia;
