/* ============================================================================
   DEVBURGUER — SCHEMA PARA BANCO NA NUVEM (Azure SQL / SQL gerenciado)
   ----------------------------------------------------------------------------
   Cria as tabelas, chaves e relacionamentos. Sem instrucoes de nivel-banco
   (CREATE DATABASE / ALTER DATABASE / arquivos / filegroup), que o banco
   gerenciado nao aceita.

   COMO USAR:
     1. Crie o banco 'DevBurguerDB' no portal do provedor (Azure SQL etc.).
     2. Conecte NESSE banco (nao no master) pelo SSMS / Azure Data Studio.
     3. Rode este script (F5). Depois rode popular_banco_demo.sql se quiser dados.
   ============================================================================ */

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

CREATE TABLE [dbo].[Adicionais](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Nome] [nvarchar](100) NULL,
	[Preco] [decimal](10, 2) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)
GO
/****** Object:  Table [dbo].[Clientes]    Script Date: 10/06/2026 16:57:39 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Clientes](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Nome] [varchar](100) NULL,
	[Telefone] [varchar](20) NULL,
	[Endereco] [varchar](200) NULL,
	[CPF] [nvarchar](20) NULL,
	[Numero] [nvarchar](10) NULL,
	[Bairro] [nvarchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)
GO
/****** Object:  Table [dbo].[EscalaMotoboy]    Script Date: 10/06/2026 16:57:39 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EscalaMotoboy](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[IdMotoboy] [int] NOT NULL,
	[DiaSemana] [int] NOT NULL,
	[Ativo] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)
GO
/****** Object:  Table [dbo].[ItensPedido]    Script Date: 10/06/2026 16:57:39 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ItensPedido](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[IdPedido] [int] NULL,
	[IdProduto] [int] NULL,
	[Quantidade] [int] NULL,
	[Observacao] [varchar](200) NULL,
	[Preco] [decimal](10, 2) NULL,
	[Adicionais] [nvarchar](300) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)
GO
/****** Object:  Table [dbo].[Motoboys]    Script Date: 10/06/2026 16:57:39 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Motoboys](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Nome] [nvarchar](100) NULL,
	[Endereco] [nvarchar](200) NULL,
	[Telefone1] [nvarchar](20) NULL,
	[Telefone2] [nvarchar](20) NULL,
	[CPF] [nvarchar](20) NULL,
	[Numero] [varchar](10) NULL,
	[Bairro] [varchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)
GO
/****** Object:  Table [dbo].[PagamentoMotoboy]    Script Date: 10/06/2026 16:57:39 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PagamentoMotoboy](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[IdMotoboy] [int] NULL,
	[QuantidadeEntregas] [int] NULL,
	[ValorTotalEntregas] [decimal](10, 2) NULL,
	[ValorChegada] [decimal](10, 2) NULL,
	[TotalPagar] [decimal](10, 2) NULL,
	[DataPagamento] [datetime] NULL,
	[Comentario] [nvarchar](300) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)
GO
/****** Object:  Table [dbo].[Pedidos]    Script Date: 10/06/2026 16:57:39 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Pedidos](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[IdCliente] [int] NULL,
	[Data] [datetime] NULL,
	[Total] [decimal](10, 2) NULL,
	[Status] [nvarchar](20) NOT NULL,
	[TipoEntrega] [nvarchar](10) NULL,
	[IdMotoboy] [int] NULL,
	[TrocoPara] [decimal](10, 2) NULL,
	[Origem] [nvarchar](20) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)
GO
/****** Object:  Table [dbo].[Produtos]    Script Date: 10/06/2026 16:57:39 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Produtos](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Nome] [varchar](100) NULL,
	[Preco] [decimal](10, 2) NULL,
	[Categoria] [varchar](50) NULL,
	[Ingredientes] [nvarchar](500) NULL,
	[Ativo] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)
GO
/****** Object:  Table [dbo].[Usuarios]    Script Date: 10/06/2026 16:57:39 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Usuarios](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Usuario] [varchar](50) NULL,
	[Senha] [varchar](64) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)
GO
/****** Object:  Index [IX_ItensPedido_IdPedido]    Script Date: 10/06/2026 16:57:39 ******/
CREATE NONCLUSTERED INDEX [IX_ItensPedido_IdPedido] ON [dbo].[ItensPedido]
(
	[IdPedido] ASC
)
INCLUDE([IdProduto],[Quantidade],[Adicionais],[Observacao]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
/****** Object:  Index [IX_PagamentoMotoboy_DataPagamento]    Script Date: 10/06/2026 16:57:39 ******/
CREATE NONCLUSTERED INDEX [IX_PagamentoMotoboy_DataPagamento] ON [dbo].[PagamentoMotoboy]
(
	[DataPagamento] ASC
)
INCLUDE([IdMotoboy],[QuantidadeEntregas],[ValorTotalEntregas],[ValorChegada],[TotalPagar]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
/****** Object:  Index [IX_Pedidos_Data]    Script Date: 10/06/2026 16:57:39 ******/
CREATE NONCLUSTERED INDEX [IX_Pedidos_Data] ON [dbo].[Pedidos]
(
	[Data] ASC
)
INCLUDE([Status],[Total]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Pedidos_Status_Producao]    Script Date: 10/06/2026 16:57:39 ******/
CREATE NONCLUSTERED INDEX [IX_Pedidos_Status_Producao] ON [dbo].[Pedidos]
(
	[Status] ASC
)
INCLUDE([IdCliente],[Total],[TipoEntrega],[Data],[IdMotoboy],[TrocoPara]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO
ALTER TABLE [dbo].[EscalaMotoboy] ADD  DEFAULT ((1)) FOR [Ativo]
GO
ALTER TABLE [dbo].[Pedidos] ADD  CONSTRAINT [DF_Pedidos_Data]  DEFAULT (getdate()) FOR [Data]
GO
ALTER TABLE [dbo].[Pedidos] ADD  DEFAULT ('Em Producao') FOR [Status]
GO
ALTER TABLE [dbo].[Pedidos] ADD  CONSTRAINT [DF_Pedidos_Origem]  DEFAULT ('Desktop') FOR [Origem]
GO
ALTER TABLE [dbo].[Produtos] ADD  CONSTRAINT [DF_Produtos_Ativo]  DEFAULT ((1)) FOR [Ativo]
GO
ALTER TABLE [dbo].[EscalaMotoboy]  WITH CHECK ADD  CONSTRAINT [FK_Escala_Motoboy] FOREIGN KEY([IdMotoboy])
REFERENCES [dbo].[Motoboys] ([Id])
GO
ALTER TABLE [dbo].[EscalaMotoboy] CHECK CONSTRAINT [FK_Escala_Motoboy]
GO
ALTER TABLE [dbo].[ItensPedido]  WITH CHECK ADD FOREIGN KEY([IdPedido])
REFERENCES [dbo].[Pedidos] ([Id])
GO
ALTER TABLE [dbo].[ItensPedido]  WITH CHECK ADD FOREIGN KEY([IdProduto])
REFERENCES [dbo].[Produtos] ([Id])
GO
ALTER TABLE [dbo].[PagamentoMotoboy]  WITH CHECK ADD FOREIGN KEY([IdMotoboy])
REFERENCES [dbo].[Motoboys] ([Id])
GO
ALTER TABLE [dbo].[Pedidos]  WITH CHECK ADD FOREIGN KEY([IdCliente])
REFERENCES [dbo].[Clientes] ([Id])
GO
GO
