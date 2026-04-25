namespace DevBurguer
{
    partial class FormPagamentoMotoboy
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormPagamentoMotoboy));
            this.cmbMotoboy = new System.Windows.Forms.ComboBox();
            this.btnAtualizar = new System.Windows.Forms.Button();
            this.btnSalvar = new System.Windows.Forms.Button();
            this.lblmotoboy = new System.Windows.Forms.Label();
            this.lblQuantidade = new System.Windows.Forms.Label();
            this.txtQtd = new System.Windows.Forms.TextBox();
            this.lblValorTotalEntregas = new System.Windows.Forms.Label();
            this.txtValorTotalEntregas = new System.Windows.Forms.TextBox();
            this.lblChegada = new System.Windows.Forms.Label();
            this.txtChegada = new System.Windows.Forms.TextBox();
            this.lblTotal = new System.Windows.Forms.Label();
            this.txtTotal = new System.Windows.Forms.TextBox();
            this.dtpData = new System.Windows.Forms.DateTimePicker();
            this.dgvPagamentos = new System.Windows.Forms.DataGridView();
            this.btnRemover = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPagamentos)).BeginInit();
            this.SuspendLayout();
            // 
            // cmbMotoboy
            // 
            this.cmbMotoboy.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbMotoboy.FormattingEnabled = true;
            this.cmbMotoboy.Location = new System.Drawing.Point(56, 53);
            this.cmbMotoboy.Name = "cmbMotoboy";
            this.cmbMotoboy.Size = new System.Drawing.Size(169, 28);
            this.cmbMotoboy.TabIndex = 16;
            // 
            // btnAtualizar
            // 
            this.btnAtualizar.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAtualizar.Location = new System.Drawing.Point(207, 543);
            this.btnAtualizar.Name = "btnAtualizar";
            this.btnAtualizar.Size = new System.Drawing.Size(162, 39);
            this.btnAtualizar.TabIndex = 26;
            this.btnAtualizar.Text = "Atualizar";
            this.btnAtualizar.UseVisualStyleBackColor = true;
            this.btnAtualizar.Click += new System.EventHandler(this.btnAtualizar_Click);
            // 
            // btnSalvar
            // 
            this.btnSalvar.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSalvar.Location = new System.Drawing.Point(32, 543);
            this.btnSalvar.Name = "btnSalvar";
            this.btnSalvar.Size = new System.Drawing.Size(169, 39);
            this.btnSalvar.TabIndex = 25;
            this.btnSalvar.Text = "Salvar";
            this.btnSalvar.UseVisualStyleBackColor = true;
            this.btnSalvar.Click += new System.EventHandler(this.btnSalvar_Click);
            // 
            // lblmotoboy
            // 
            this.lblmotoboy.AutoSize = true;
            this.lblmotoboy.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblmotoboy.Location = new System.Drawing.Point(61, 28);
            this.lblmotoboy.Name = "lblmotoboy";
            this.lblmotoboy.Size = new System.Drawing.Size(78, 22);
            this.lblmotoboy.TabIndex = 28;
            this.lblmotoboy.Text = "Motoboy";
            // 
            // lblQuantidade
            // 
            this.lblQuantidade.AutoSize = true;
            this.lblQuantidade.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblQuantidade.Location = new System.Drawing.Point(61, 101);
            this.lblQuantidade.Name = "lblQuantidade";
            this.lblQuantidade.Size = new System.Drawing.Size(103, 22);
            this.lblQuantidade.TabIndex = 30;
            this.lblQuantidade.Text = "Quantidade";
            // 
            // txtQtd
            // 
            this.txtQtd.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtQtd.Location = new System.Drawing.Point(56, 126);
            this.txtQtd.Name = "txtQtd";
            this.txtQtd.Size = new System.Drawing.Size(76, 27);
            this.txtQtd.TabIndex = 29;
            // 
            // lblValorTotalEntregas
            // 
            this.lblValorTotalEntregas.AutoSize = true;
            this.lblValorTotalEntregas.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblValorTotalEntregas.Location = new System.Drawing.Point(61, 168);
            this.lblValorTotalEntregas.Name = "lblValorTotalEntregas";
            this.lblValorTotalEntregas.Size = new System.Drawing.Size(203, 22);
            this.lblValorTotalEntregas.TabIndex = 32;
            this.lblValorTotalEntregas.Text = "Valor Total De Entregas";
            // 
            // txtValorTotalEntregas
            // 
            this.txtValorTotalEntregas.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtValorTotalEntregas.Location = new System.Drawing.Point(56, 193);
            this.txtValorTotalEntregas.Name = "txtValorTotalEntregas";
            this.txtValorTotalEntregas.Size = new System.Drawing.Size(76, 27);
            this.txtValorTotalEntregas.TabIndex = 31;
            // 
            // lblChegada
            // 
            this.lblChegada.AutoSize = true;
            this.lblChegada.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblChegada.Location = new System.Drawing.Point(61, 235);
            this.lblChegada.Name = "lblChegada";
            this.lblChegada.Size = new System.Drawing.Size(83, 22);
            this.lblChegada.TabIndex = 34;
            this.lblChegada.Text = "Chegada";
            // 
            // txtChegada
            // 
            this.txtChegada.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtChegada.Location = new System.Drawing.Point(56, 260);
            this.txtChegada.Name = "txtChegada";
            this.txtChegada.Size = new System.Drawing.Size(76, 27);
            this.txtChegada.TabIndex = 33;
            // 
            // lblTotal
            // 
            this.lblTotal.AutoSize = true;
            this.lblTotal.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTotal.Location = new System.Drawing.Point(61, 309);
            this.lblTotal.Name = "lblTotal";
            this.lblTotal.Size = new System.Drawing.Size(51, 22);
            this.lblTotal.TabIndex = 36;
            this.lblTotal.Text = "Total";
            // 
            // txtTotal
            // 
            this.txtTotal.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTotal.Location = new System.Drawing.Point(56, 334);
            this.txtTotal.Name = "txtTotal";
            this.txtTotal.ReadOnly = true;
            this.txtTotal.Size = new System.Drawing.Size(76, 27);
            this.txtTotal.TabIndex = 35;
            this.txtTotal.TextChanged += new System.EventHandler(this.CalcularTotal);
            // 
            // dtpData
            // 
            this.dtpData.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dtpData.Location = new System.Drawing.Point(56, 383);
            this.dtpData.Name = "dtpData";
            this.dtpData.Size = new System.Drawing.Size(366, 27);
            this.dtpData.TabIndex = 37;
            // 
            // dgvPagamentos
            // 
            this.dgvPagamentos.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvPagamentos.Dock = System.Windows.Forms.DockStyle.Right;
            this.dgvPagamentos.Location = new System.Drawing.Point(567, 0);
            this.dgvPagamentos.Name = "dgvPagamentos";
            this.dgvPagamentos.Size = new System.Drawing.Size(747, 618);
            this.dgvPagamentos.TabIndex = 38;
            this.dgvPagamentos.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvPagamentos_CellClick);
            // 
            // btnRemover
            // 
            this.btnRemover.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRemover.Location = new System.Drawing.Point(375, 543);
            this.btnRemover.Name = "btnRemover";
            this.btnRemover.Size = new System.Drawing.Size(162, 39);
            this.btnRemover.TabIndex = 39;
            this.btnRemover.Text = "Excluir";
            this.btnRemover.UseVisualStyleBackColor = true;
            this.btnRemover.Click += new System.EventHandler(this.btnRemover_Click);
            // 
            // FormPagamentoMotoboy
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(1314, 618);
            this.Controls.Add(this.btnRemover);
            this.Controls.Add(this.dgvPagamentos);
            this.Controls.Add(this.dtpData);
            this.Controls.Add(this.lblTotal);
            this.Controls.Add(this.txtTotal);
            this.Controls.Add(this.lblChegada);
            this.Controls.Add(this.txtChegada);
            this.Controls.Add(this.lblValorTotalEntregas);
            this.Controls.Add(this.txtValorTotalEntregas);
            this.Controls.Add(this.lblQuantidade);
            this.Controls.Add(this.txtQtd);
            this.Controls.Add(this.lblmotoboy);
            this.Controls.Add(this.btnAtualizar);
            this.Controls.Add(this.btnSalvar);
            this.Controls.Add(this.cmbMotoboy);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Name = "FormPagamentoMotoboy";
            this.Text = "FormPagamentoMotoboy";
            this.Load += new System.EventHandler(this.FormPagamentoMotoboy_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvPagamentos)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cmbMotoboy;
        private System.Windows.Forms.Button btnAtualizar;
        private System.Windows.Forms.Button btnSalvar;
        private System.Windows.Forms.Label lblmotoboy;
        private System.Windows.Forms.Label lblQuantidade;
        private System.Windows.Forms.TextBox txtQtd;
        private System.Windows.Forms.Label lblValorTotalEntregas;
        private System.Windows.Forms.TextBox txtValorTotalEntregas;
        private System.Windows.Forms.Label lblChegada;
        private System.Windows.Forms.TextBox txtChegada;
        private System.Windows.Forms.Label lblTotal;
        private System.Windows.Forms.TextBox txtTotal;
        private System.Windows.Forms.DateTimePicker dtpData;
        private System.Windows.Forms.DataGridView dgvPagamentos;
        private System.Windows.Forms.Button btnRemover;
    }
}