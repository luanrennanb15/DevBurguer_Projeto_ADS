using System;

namespace DevBurguer
{
    partial class FormMenu
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMenu));
            this.btnClientes = new System.Windows.Forms.Button();
            this.btnProdutos = new System.Windows.Forms.Button();
            this.btnPedidos = new System.Windows.Forms.Button();
            this.btnFaturamento = new System.Windows.Forms.Button();
            this.btnSair = new System.Windows.Forms.Button();
            this.btnRelatorioProdutos = new System.Windows.Forms.Button();
            this.panelMenu = new System.Windows.Forms.Panel();
            this.btnPagmentoDeMotoboy = new System.Windows.Forms.Button();
            this.btnCadastroDeMotoboy = new System.Windows.Forms.Button();
            this.btnFaturamentoMotoboy = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panelConteudo = new System.Windows.Forms.Panel();
            this.panelMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnClientes
            // 
            this.btnClientes.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClientes.Location = new System.Drawing.Point(29, 90);
            this.btnClientes.Name = "btnClientes";
            this.btnClientes.Size = new System.Drawing.Size(192, 53);
            this.btnClientes.TabIndex = 0;
            this.btnClientes.Text = "Cadastro de Clientes";
            this.btnClientes.UseVisualStyleBackColor = true;
            this.btnClientes.Click += new System.EventHandler(this.btnClientes_Click);
            // 
            // btnProdutos
            // 
            this.btnProdutos.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnProdutos.Location = new System.Drawing.Point(29, 31);
            this.btnProdutos.Name = "btnProdutos";
            this.btnProdutos.Size = new System.Drawing.Size(192, 53);
            this.btnProdutos.TabIndex = 1;
            this.btnProdutos.Text = "Cadastro de Produtos";
            this.btnProdutos.UseVisualStyleBackColor = true;
            this.btnProdutos.Click += new System.EventHandler(this.btnProdutos_Click);
            // 
            // btnPedidos
            // 
            this.btnPedidos.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPedidos.Location = new System.Drawing.Point(29, 209);
            this.btnPedidos.Name = "btnPedidos";
            this.btnPedidos.Size = new System.Drawing.Size(192, 54);
            this.btnPedidos.TabIndex = 2;
            this.btnPedidos.Text = "Pedidos";
            this.btnPedidos.UseVisualStyleBackColor = true;
            this.btnPedidos.Click += new System.EventHandler(this.btnPedidos_Click);
            // 
            // btnFaturamento
            // 
            this.btnFaturamento.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnFaturamento.Location = new System.Drawing.Point(29, 329);
            this.btnFaturamento.Name = "btnFaturamento";
            this.btnFaturamento.Size = new System.Drawing.Size(192, 54);
            this.btnFaturamento.TabIndex = 3;
            this.btnFaturamento.Text = "Faturamento da Lanchonete";
            this.btnFaturamento.UseVisualStyleBackColor = true;
            this.btnFaturamento.Click += new System.EventHandler(this.btnFaturamento_Click);
            // 
            // btnSair
            // 
            this.btnSair.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSair.Location = new System.Drawing.Point(29, 764);
            this.btnSair.Name = "btnSair";
            this.btnSair.Size = new System.Drawing.Size(192, 54);
            this.btnSair.TabIndex = 4;
            this.btnSair.Text = "Sair";
            this.btnSair.UseVisualStyleBackColor = true;
            this.btnSair.Click += new System.EventHandler(this.btnSair_Click);
            // 
            // btnRelatorioProdutos
            // 
            this.btnRelatorioProdutos.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRelatorioProdutos.Location = new System.Drawing.Point(29, 269);
            this.btnRelatorioProdutos.Name = "btnRelatorioProdutos";
            this.btnRelatorioProdutos.Size = new System.Drawing.Size(192, 54);
            this.btnRelatorioProdutos.TabIndex = 5;
            this.btnRelatorioProdutos.Text = "Mais Vendidos";
            this.btnRelatorioProdutos.UseVisualStyleBackColor = true;
            this.btnRelatorioProdutos.Click += new System.EventHandler(this.btnRelatorioProdutos_Click);
            // 
            // panelMenu
            // 
            this.panelMenu.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.panelMenu.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panelMenu.Controls.Add(this.btnPagmentoDeMotoboy);
            this.panelMenu.Controls.Add(this.btnCadastroDeMotoboy);
            this.panelMenu.Controls.Add(this.btnFaturamentoMotoboy);
            this.panelMenu.Controls.Add(this.btnClientes);
            this.panelMenu.Controls.Add(this.btnSair);
            this.panelMenu.Controls.Add(this.btnRelatorioProdutos);
            this.panelMenu.Controls.Add(this.btnProdutos);
            this.panelMenu.Controls.Add(this.btnPedidos);
            this.panelMenu.Controls.Add(this.btnFaturamento);
            this.panelMenu.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelMenu.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.panelMenu.Location = new System.Drawing.Point(0, 0);
            this.panelMenu.Name = "panelMenu";
            this.panelMenu.Size = new System.Drawing.Size(250, 861);
            this.panelMenu.TabIndex = 6;
            // 
            // btnPagmentoDeMotoboy
            // 
            this.btnPagmentoDeMotoboy.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPagmentoDeMotoboy.Location = new System.Drawing.Point(29, 449);
            this.btnPagmentoDeMotoboy.Name = "btnPagmentoDeMotoboy";
            this.btnPagmentoDeMotoboy.Size = new System.Drawing.Size(192, 54);
            this.btnPagmentoDeMotoboy.TabIndex = 8;
            this.btnPagmentoDeMotoboy.Text = "Pagamento dos Motoboy";
            this.btnPagmentoDeMotoboy.UseVisualStyleBackColor = true;
            this.btnPagmentoDeMotoboy.Click += new System.EventHandler(this.btnPagamentoDeMotoboy_Click);
            // 
            // btnCadastroDeMotoboy
            // 
            this.btnCadastroDeMotoboy.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCadastroDeMotoboy.Location = new System.Drawing.Point(29, 149);
            this.btnCadastroDeMotoboy.Name = "btnCadastroDeMotoboy";
            this.btnCadastroDeMotoboy.Size = new System.Drawing.Size(192, 54);
            this.btnCadastroDeMotoboy.TabIndex = 7;
            this.btnCadastroDeMotoboy.Text = "Cadastro De Motoboy";
            this.btnCadastroDeMotoboy.UseVisualStyleBackColor = true;
            this.btnCadastroDeMotoboy.Click += new System.EventHandler(this.btnCadastroDeMotoboy_Click);
            // 
            // btnFaturamentoMotoboy
            // 
            this.btnFaturamentoMotoboy.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnFaturamentoMotoboy.Location = new System.Drawing.Point(29, 389);
            this.btnFaturamentoMotoboy.Name = "btnFaturamentoMotoboy";
            this.btnFaturamentoMotoboy.Size = new System.Drawing.Size(192, 54);
            this.btnFaturamentoMotoboy.TabIndex = 6;
            this.btnFaturamentoMotoboy.Text = "Faturamento Motoboy";
            this.btnFaturamentoMotoboy.UseVisualStyleBackColor = true;
            this.btnFaturamentoMotoboy.Click += new System.EventHandler(this.btnFaturamentoMotoboy_Click);
            // 
            // panel2
            // 
            this.panel2.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panel2.BackgroundImage")));
            this.panel2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(250, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1334, 200);
            this.panel2.TabIndex = 7;
            // 
            // panelConteudo
            // 
            this.panelConteudo.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panelConteudo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelConteudo.Location = new System.Drawing.Point(250, 200);
            this.panelConteudo.Name = "panelConteudo";
            this.panelConteudo.Size = new System.Drawing.Size(1334, 661);
            this.panelConteudo.TabIndex = 8;
            // 
            // FormMenu
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(1584, 861);
            this.Controls.Add(this.panelConteudo);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panelMenu);
            this.DoubleBuffered = true;
            this.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Name = "FormMenu";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MENU";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.panelMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        private System.Windows.Forms.Button btnClientes;
        private System.Windows.Forms.Button btnProdutos;
        private System.Windows.Forms.Button btnPedidos;
        private System.Windows.Forms.Button btnFaturamento;
        private System.Windows.Forms.Button btnSair;
        private System.Windows.Forms.Button btnRelatorioProdutos;
        private System.Windows.Forms.Panel panelMenu;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panelConteudo;
        private System.Windows.Forms.Button btnPagmentoDeMotoboy;
        private System.Windows.Forms.Button btnCadastroDeMotoboy;
        private System.Windows.Forms.Button btnFaturamentoMotoboy;
    }
}