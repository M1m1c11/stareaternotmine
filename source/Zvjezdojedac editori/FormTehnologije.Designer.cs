namespace Zvjezdojedac_editori
{
	partial class FormTehnologije
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
			this.radRazvoj = new System.Windows.Forms.RadioButton();
			this.radIstrazivanje = new System.Windows.Forms.RadioButton();
			this.lstvTehnologije = new System.Windows.Forms.ListView();
			this.columnHeader0 = new System.Windows.Forms.ColumnHeader();
			this.label1 = new System.Windows.Forms.Label();
			this.txtNaziv = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.picSlika = new System.Windows.Forms.PictureBox();
			this.txtKod = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.txtCijena = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.lblCijenaGreska = new System.Windows.Forms.Label();
			this.txtMaxNivo = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.lblMaxNivoGreska = new System.Windows.Forms.Label();
			this.lblKodGreska = new System.Windows.Forms.Label();
			this.btnSlika = new System.Windows.Forms.Button();
			this.lblSlikaGreska = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.lstvPreduvjeti = new System.Windows.Forms.ListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this.btnPreduvjeti = new System.Windows.Forms.Button();
			this.txtSlika = new System.Windows.Forms.TextBox();
			this.btnGore = new System.Windows.Forms.Button();
			this.btnDolje = new System.Windows.Forms.Button();
			this.btnNovaTeh = new System.Windows.Forms.Button();
			this.btnUkloni = new System.Windows.Forms.Button();
			this.txtOpis = new System.Windows.Forms.TextBox();
			this.label7 = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.picSlika)).BeginInit();
			this.SuspendLayout();
			// 
			// radRazvoj
			// 
			this.radRazvoj.AutoSize = true;
			this.radRazvoj.Location = new System.Drawing.Point(12, 12);
			this.radRazvoj.Name = "radRazvoj";
			this.radRazvoj.Size = new System.Drawing.Size(58, 17);
			this.radRazvoj.TabIndex = 0;
			this.radRazvoj.TabStop = true;
			this.radRazvoj.Text = "Razvoj";
			this.radRazvoj.UseVisualStyleBackColor = true;
			this.radRazvoj.CheckedChanged += new System.EventHandler(this.radRazvoj_CheckedChanged);
			// 
			// radIstrazivanje
			// 
			this.radIstrazivanje.AutoSize = true;
			this.radIstrazivanje.Location = new System.Drawing.Point(76, 12);
			this.radIstrazivanje.Name = "radIstrazivanje";
			this.radIstrazivanje.Size = new System.Drawing.Size(78, 17);
			this.radIstrazivanje.TabIndex = 1;
			this.radIstrazivanje.TabStop = true;
			this.radIstrazivanje.Text = "Istraživanje";
			this.radIstrazivanje.UseVisualStyleBackColor = true;
			this.radIstrazivanje.CheckedChanged += new System.EventHandler(this.radIstrazivanje_CheckedChanged);
			// 
			// lstvTehnologije
			// 
			this.lstvTehnologije.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader0});
			this.lstvTehnologije.FullRowSelect = true;
			this.lstvTehnologije.GridLines = true;
			this.lstvTehnologije.HideSelection = false;
			this.lstvTehnologije.Location = new System.Drawing.Point(12, 35);
			this.lstvTehnologije.MultiSelect = false;
			this.lstvTehnologije.Name = "lstvTehnologije";
			this.lstvTehnologije.Size = new System.Drawing.Size(268, 296);
			this.lstvTehnologije.TabIndex = 2;
			this.lstvTehnologije.UseCompatibleStateImageBehavior = false;
			this.lstvTehnologije.View = System.Windows.Forms.View.Details;
			this.lstvTehnologije.SelectedIndexChanged += new System.EventHandler(this.lstvTehnologije_SelectedIndexChanged);
			// 
			// columnHeader0
			// 
			this.columnHeader0.Text = "Naziv";
			this.columnHeader0.Width = 246;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(372, 38);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(37, 13);
			this.label1.TabIndex = 3;
			this.label1.Text = "Naziv:";
			// 
			// txtNaziv
			// 
			this.txtNaziv.Location = new System.Drawing.Point(417, 35);
			this.txtNaziv.Name = "txtNaziv";
			this.txtNaziv.Size = new System.Drawing.Size(196, 20);
			this.txtNaziv.TabIndex = 4;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(372, 168);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(33, 13);
			this.label2.TabIndex = 5;
			this.label2.Text = "Slika:";
			// 
			// picSlika
			// 
			this.picSlika.Location = new System.Drawing.Point(286, 35);
			this.picSlika.Name = "picSlika";
			this.picSlika.Size = new System.Drawing.Size(80, 80);
			this.picSlika.TabIndex = 6;
			this.picSlika.TabStop = false;
			// 
			// txtKod
			// 
			this.txtKod.Location = new System.Drawing.Point(417, 61);
			this.txtKod.Name = "txtKod";
			this.txtKod.Size = new System.Drawing.Size(196, 20);
			this.txtKod.TabIndex = 8;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(372, 64);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(29, 13);
			this.label3.TabIndex = 7;
			this.label3.Text = "Kod:";
			// 
			// txtCijena
			// 
			this.txtCijena.Location = new System.Drawing.Point(417, 113);
			this.txtCijena.Name = "txtCijena";
			this.txtCijena.Size = new System.Drawing.Size(196, 20);
			this.txtCijena.TabIndex = 10;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(372, 116);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(39, 13);
			this.label4.TabIndex = 9;
			this.label4.Text = "Cijena:";
			// 
			// lblCijenaGreska
			// 
			this.lblCijenaGreska.AutoSize = true;
			this.lblCijenaGreska.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.lblCijenaGreska.ForeColor = System.Drawing.Color.Red;
			this.lblCijenaGreska.Location = new System.Drawing.Point(616, 116);
			this.lblCijenaGreska.Name = "lblCijenaGreska";
			this.lblCijenaGreska.Size = new System.Drawing.Size(16, 13);
			this.lblCijenaGreska.TabIndex = 11;
			this.lblCijenaGreska.Text = "*!";
			this.lblCijenaGreska.Visible = false;
			// 
			// txtMaxNivo
			// 
			this.txtMaxNivo.Location = new System.Drawing.Point(440, 139);
			this.txtMaxNivo.Name = "txtMaxNivo";
			this.txtMaxNivo.Size = new System.Drawing.Size(173, 20);
			this.txtMaxNivo.TabIndex = 13;
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(372, 142);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(62, 13);
			this.label5.TabIndex = 12;
			this.label5.Text = "Maks. nivo:";
			// 
			// lblMaxNivoGreska
			// 
			this.lblMaxNivoGreska.AutoSize = true;
			this.lblMaxNivoGreska.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.lblMaxNivoGreska.ForeColor = System.Drawing.Color.Red;
			this.lblMaxNivoGreska.Location = new System.Drawing.Point(616, 142);
			this.lblMaxNivoGreska.Name = "lblMaxNivoGreska";
			this.lblMaxNivoGreska.Size = new System.Drawing.Size(16, 13);
			this.lblMaxNivoGreska.TabIndex = 14;
			this.lblMaxNivoGreska.Text = "*!";
			this.lblMaxNivoGreska.Visible = false;
			// 
			// lblKodGreska
			// 
			this.lblKodGreska.AutoSize = true;
			this.lblKodGreska.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.lblKodGreska.ForeColor = System.Drawing.Color.Red;
			this.lblKodGreska.Location = new System.Drawing.Point(619, 64);
			this.lblKodGreska.Name = "lblKodGreska";
			this.lblKodGreska.Size = new System.Drawing.Size(16, 13);
			this.lblKodGreska.TabIndex = 15;
			this.lblKodGreska.Text = "*!";
			this.lblKodGreska.Visible = false;
			// 
			// btnSlika
			// 
			this.btnSlika.Location = new System.Drawing.Point(566, 191);
			this.btnSlika.Name = "btnSlika";
			this.btnSlika.Size = new System.Drawing.Size(47, 23);
			this.btnSlika.TabIndex = 16;
			this.btnSlika.Text = "Slika...";
			this.btnSlika.UseVisualStyleBackColor = true;
			this.btnSlika.Click += new System.EventHandler(this.btnSlika_Click);
			// 
			// lblSlikaGreska
			// 
			this.lblSlikaGreska.AutoSize = true;
			this.lblSlikaGreska.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.lblSlikaGreska.ForeColor = System.Drawing.Color.Red;
			this.lblSlikaGreska.Location = new System.Drawing.Point(616, 168);
			this.lblSlikaGreska.Name = "lblSlikaGreska";
			this.lblSlikaGreska.Size = new System.Drawing.Size(16, 13);
			this.lblSlikaGreska.TabIndex = 17;
			this.lblSlikaGreska.Text = "*!";
			this.lblSlikaGreska.Visible = false;
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(372, 219);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(57, 13);
			this.label6.TabIndex = 18;
			this.label6.Text = "Preduvjeti:";
			// 
			// lstvPreduvjeti
			// 
			this.lstvPreduvjeti.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
			this.lstvPreduvjeti.FullRowSelect = true;
			this.lstvPreduvjeti.GridLines = true;
			this.lstvPreduvjeti.HideSelection = false;
			this.lstvPreduvjeti.Location = new System.Drawing.Point(375, 235);
			this.lstvPreduvjeti.MultiSelect = false;
			this.lstvPreduvjeti.Name = "lstvPreduvjeti";
			this.lstvPreduvjeti.Size = new System.Drawing.Size(238, 96);
			this.lstvPreduvjeti.TabIndex = 19;
			this.lstvPreduvjeti.UseCompatibleStateImageBehavior = false;
			this.lstvPreduvjeti.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Tehnologija";
			this.columnHeader1.Width = 106;
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "Nivo";
			this.columnHeader2.Width = 117;
			// 
			// btnPreduvjeti
			// 
			this.btnPreduvjeti.Location = new System.Drawing.Point(538, 337);
			this.btnPreduvjeti.Name = "btnPreduvjeti";
			this.btnPreduvjeti.Size = new System.Drawing.Size(75, 23);
			this.btnPreduvjeti.TabIndex = 20;
			this.btnPreduvjeti.Text = "Preduvjeti...";
			this.btnPreduvjeti.UseVisualStyleBackColor = true;
			this.btnPreduvjeti.Click += new System.EventHandler(this.btnPreduvjeti_Click);
			// 
			// txtSlika
			// 
			this.txtSlika.Location = new System.Drawing.Point(417, 165);
			this.txtSlika.Name = "txtSlika";
			this.txtSlika.Size = new System.Drawing.Size(196, 20);
			this.txtSlika.TabIndex = 21;
			// 
			// btnGore
			// 
			this.btnGore.Location = new System.Drawing.Point(286, 142);
			this.btnGore.Name = "btnGore";
			this.btnGore.Size = new System.Drawing.Size(42, 23);
			this.btnGore.TabIndex = 22;
			this.btnGore.Text = "/\\";
			this.btnGore.UseVisualStyleBackColor = true;
			this.btnGore.Click += new System.EventHandler(this.btnGore_Click);
			// 
			// btnDolje
			// 
			this.btnDolje.Location = new System.Drawing.Point(286, 171);
			this.btnDolje.Name = "btnDolje";
			this.btnDolje.Size = new System.Drawing.Size(42, 23);
			this.btnDolje.TabIndex = 23;
			this.btnDolje.Text = "\\/";
			this.btnDolje.UseVisualStyleBackColor = true;
			this.btnDolje.Click += new System.EventHandler(this.btnDolje_Click);
			// 
			// btnNovaTeh
			// 
			this.btnNovaTeh.Location = new System.Drawing.Point(123, 337);
			this.btnNovaTeh.Name = "btnNovaTeh";
			this.btnNovaTeh.Size = new System.Drawing.Size(75, 23);
			this.btnNovaTeh.TabIndex = 24;
			this.btnNovaTeh.Text = "&Nova";
			this.btnNovaTeh.UseVisualStyleBackColor = true;
			this.btnNovaTeh.Click += new System.EventHandler(this.btnNovaTeh_Click);
			// 
			// btnUkloni
			// 
			this.btnUkloni.Location = new System.Drawing.Point(204, 337);
			this.btnUkloni.Name = "btnUkloni";
			this.btnUkloni.Size = new System.Drawing.Size(75, 23);
			this.btnUkloni.TabIndex = 25;
			this.btnUkloni.Text = "&Ukoni";
			this.btnUkloni.UseVisualStyleBackColor = true;
			this.btnUkloni.Click += new System.EventHandler(this.btnUkloni_Click);
			// 
			// txtOpis
			// 
			this.txtOpis.Location = new System.Drawing.Point(417, 87);
			this.txtOpis.Name = "txtOpis";
			this.txtOpis.Size = new System.Drawing.Size(196, 20);
			this.txtOpis.TabIndex = 27;
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(372, 90);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(31, 13);
			this.label7.TabIndex = 26;
			this.label7.Text = "Opis:";
			// 
			// FormTehnologije
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(644, 375);
			this.Controls.Add(this.txtOpis);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.btnUkloni);
			this.Controls.Add(this.btnNovaTeh);
			this.Controls.Add(this.btnDolje);
			this.Controls.Add(this.btnGore);
			this.Controls.Add(this.txtSlika);
			this.Controls.Add(this.btnPreduvjeti);
			this.Controls.Add(this.lstvPreduvjeti);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.lblSlikaGreska);
			this.Controls.Add(this.btnSlika);
			this.Controls.Add(this.lblKodGreska);
			this.Controls.Add(this.lblMaxNivoGreska);
			this.Controls.Add(this.txtMaxNivo);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.lblCijenaGreska);
			this.Controls.Add(this.txtCijena);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.txtKod);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.picSlika);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.txtNaziv);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.lstvTehnologije);
			this.Controls.Add(this.radIstrazivanje);
			this.Controls.Add(this.radRazvoj);
			this.Name = "FormTehnologije";
			this.Text = "Tehnologije";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormTehnologije_FormClosing);
			((System.ComponentModel.ISupportInitialize)(this.picSlika)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.RadioButton radRazvoj;
		private System.Windows.Forms.RadioButton radIstrazivanje;
		private System.Windows.Forms.ListView lstvTehnologije;
		private System.Windows.Forms.ColumnHeader columnHeader0;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox txtNaziv;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.PictureBox picSlika;
		private System.Windows.Forms.TextBox txtKod;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox txtCijena;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label lblCijenaGreska;
		private System.Windows.Forms.TextBox txtMaxNivo;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label lblMaxNivoGreska;
		private System.Windows.Forms.Label lblKodGreska;
		private System.Windows.Forms.Button btnSlika;
		private System.Windows.Forms.Label lblSlikaGreska;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.ListView lstvPreduvjeti;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.Button btnPreduvjeti;
		private System.Windows.Forms.TextBox txtSlika;
		private System.Windows.Forms.Button btnGore;
		private System.Windows.Forms.Button btnDolje;
		private System.Windows.Forms.Button btnNovaTeh;
		private System.Windows.Forms.Button btnUkloni;
		private System.Windows.Forms.TextBox txtOpis;
		private System.Windows.Forms.Label label7;
	}
}