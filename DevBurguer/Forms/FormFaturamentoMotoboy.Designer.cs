namespace DevBurguer
{
    partial class FormFaturamentoMotoboy
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormFaturamentoMotoboy));
            this.lblmotoboy = new System.Windows.Forms.Label();
            this.cmbMotoboy = new System.Windows.Forms.ComboBox();
            this.dtpInicio = new System.Windows.Forms.DateTimePicker();
            this.dtpFim = new System.Windows.Forms.DateTimePicker();
            this.btnHoje = new System.Windows.Forms.Button();
            this.btnMes = new System.Windows.Forms.Button();
            this.btnAno = new System.Windows.Forms.Button();
            this.btnBuscar = new System.Windows.Forms.Button();
            this.dgvFaturamento = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.dgvFaturamento)).BeginInit();
            this.SuspendLayout();
            // 
            // lblmotoboy
            // 
            this.lblmotoboy.AutoSize = true;
            this.lblmotoboy.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblmotoboy.Location = new System.Drawing.Point(35, 33);
            this.lblmotoboy.Name = "lblmotoboy";
            this.lblmotoboy.Size = new System.Drawing.Size(78, 22);
            this.lblmotoboy.TabIndex = 30;
            this.lblmotoboy.Text = "Motoboy";
            // 
            // cmbMotoboy
            // 
            this.cmbMotoboy.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbMotoboy.FormattingEnabled = true;
            this.cmbMotoboy.Location = new System.Drawing.Point(39, 58);
            this.cmbMotoboy.Name = "cmbMotoboy";
            this.cmbMotoboy.Size = new System.Drawing.Size(169, 28);
            this.cmbMotoboy.TabIndex = 29;
            // 
            // dtpInicio
            // 
            this.dtpInicio.Location = new System.Drawing.Point(39, 121);
            this.dtpInicio.Name = "dtpInicio";
            this.dtpInicio.Size = new System.Drawing.Size(237, 20);
            this.dtpInicio.TabIndex = 31;
            // 
            // dtpFim
            // 
            this.dtpFim.Location = new System.Drawing.Point(361, 121);
            this.dtpFim.Name = "dtpFim";
            this.dtpFim.Size = new System.Drawing.Size(218, 20);
            this.dtpFim.TabIndex = 32;
            // 
            // btnHoje
            // 
            this.btnHoje.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnHoje.Location = new System.Drawing.Point(325, 54);
            this.btnHoje.Name = "btnHoje";
            this.btnHoje.Size = new System.Drawing.Size(84, 35);
            this.btnHoje.TabIndex = 33;
            this.btnHoje.Text = "Hoje";
            this.btnHoje.UseVisualStyleBackColor = true;
            this.btnHoje.Click += new System.EventHandler(this.btnHoje_Click);
            // 
            // btnMes
            // 
            this.btnMes.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnMes.Location = new System.Drawing.Point(438, 54);
            this.btnMes.Name = "btnMes";
            this.btnMes.Size = new System.Drawing.Size(113, 35);
            this.btnMes.TabIndex = 34;
            this.btnMes.Text = "Mês";
            this.btnMes.UseVisualStyleBackColor = true;
            this.btnMes.Click += new System.EventHandler(this.btnMes_Click);
            // 
            // btnAno
            // 
            this.btnAno.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAno.Location = new System.Drawing.Point(576, 54);
            this.btnAno.Name = "btnAno";
            this.btnAno.Size = new System.Drawing.Size(122, 35);
            this.btnAno.TabIndex = 35;
            this.btnAno.Text = "Ano";
            this.btnAno.UseVisualStyleBackColor = true;
            this.btnAno.Click += new System.EventHandler(this.btnAno_Click);
            // 
            // btnBuscar
            // 
            this.btnBuscar.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnBuscar.Location = new System.Drawing.Point(719, 54);
            this.btnBuscar.Name = "btnBuscar";
            this.btnBuscar.Size = new System.Drawing.Size(84, 35);
            this.btnBuscar.TabIndex = 36;
            this.btnBuscar.Text = "Buscar";
            this.btnBuscar.UseVisualStyleBackColor = true;
            this.btnBuscar.Click += new System.EventHandler(this.btnBuscar_Click);
            // 
            // dgvFaturamento
            // 
            this.dgvFaturamento.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvFaturamento.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.dgvFaturamento.Location = new System.Drawing.Point(0, 242);
            this.dgvFaturamento.Name = "dgvFaturamento";
            this.dgvFaturamento.Size = new System.Drawing.Size(1318, 380);
            this.dgvFaturamento.TabIndex = 37;
            // 
            // FormFaturamentoMotoboy
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(1318, 622);
            this.Controls.Add(this.dgvFaturamento);
            this.Controls.Add(this.btnBuscar);
            this.Controls.Add(this.btnAno);
            this.Controls.Add(this.btnMes);
            this.Controls.Add(this.btnHoje);
            this.Controls.Add(this.dtpFim);
            this.Controls.Add(this.dtpInicio);
            this.Controls.Add(this.lblmotoboy);
            this.Controls.Add(this.cmbMotoboy);
            this.DoubleBuffered = true;
            this.Name = "FormFaturamentoMotoboy";
            this.Text = "FormFaturamentoMotoboy";
            this.Load += new System.EventHandler(this.FormFaturamentoMotoboy_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvFaturamento)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblmotoboy;
        private System.Windows.Forms.ComboBox cmbMotoboy;
        private System.Windows.Forms.DateTimePicker dtpInicio;
        private System.Windows.Forms.DateTimePicker dtpFim;
        private System.Windows.Forms.Button btnHoje;
        private System.Windows.Forms.Button btnMes;
        private System.Windows.Forms.Button btnAno;
        private System.Windows.Forms.Button btnBuscar;
        private System.Windows.Forms.DataGridView dgvFaturamento;
    }
}