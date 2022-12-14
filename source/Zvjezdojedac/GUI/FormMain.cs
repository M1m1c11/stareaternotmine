using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.IO.Compression;
using Zvjezdojedac.Podaci;
using Zvjezdojedac.Podaci.Jezici;
using Zvjezdojedac.Igra;

namespace Zvjezdojedac.GUI
{
	public partial class FormMain : Form
	{
		Font pocetniFont;

		public FormMain()
		{
			InitializeComponent();
			this.pocetniFont = Font;

#if !DEBUG
			try
			{
#endif
				PodaciAlat.postaviPodatke();
				postaviJezik();
				this.Font = Postavke.FontSucelja(pocetniFont);
#if !DEBUG
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message, "Problem", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
#endif
		}

		private void postaviJezik()
		{
			Dictionary<string, ITekst> jezik = Postavke.Jezik[Kontekst.FormMain];

			btnNovaIgra.Text = jezik["NOVA_IGRA"].tekst(null);
			btnPostavke.Text = jezik["POSTAVKE"].tekst(null);
			btnUcitaj.Text = jezik["UCITAJ"].tekst(null);
			btnUgasi.Text = jezik["UGASI"].tekst(null);
		}

		private void btnNovaIgra_Click(object sender, EventArgs e)
		{
			FormNovaIgra novaIgra = new FormNovaIgra();
			if (novaIgra.ShowDialog() == DialogResult.OK)
			{
				using(FormIgra igra = new FormIgra(new IgraZvj(novaIgra.igraci, novaIgra.mapa, novaIgra.PocetnaPop)))
					igra.ShowDialog();
			}
		}

		private void btnUgasi_Click(object sender, EventArgs e)
		{
			Application.Exit();
		}

		private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
		{
			Postavke.Spremi();
		}

		private void btnUcitaj_Click(object sender, EventArgs e)
		{
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.InitialDirectory = Environment.CurrentDirectory + Path.DirectorySeparatorChar + "pohranjeno";
			dialog.FileName = "sejv.igra";
			dialog.Filter = Postavke.Jezik[Kontekst.WindowsDijalozi, "TIP_SEJVA"].tekst(null) + " (*.igra)|*.igra";

			if (dialog.ShowDialog() == DialogResult.OK) {

				GZipStream zipStream = new GZipStream(new FileStream(dialog.FileName, FileMode.Open), CompressionMode.Decompress);
				StreamReader citac = new StreamReader(zipStream);

				string ucitanaIgra = citac.ReadToEnd();
				citac.Close();

				IgraZvj igra = IgraZvj.Ucitaj(ucitanaIgra);

				using (FormIgra frmIgra = new FormIgra(igra))
					frmIgra.ShowDialog();
				
			}
		}

		private void btnPostavke_Click(object sender, EventArgs e)
		{
			FormPostavke frmPostavke = new FormPostavke();
			if (frmPostavke.ShowDialog() == DialogResult.OK) {
				postaviJezik();
				this.Font = Postavke.FontSucelja(pocetniFont);
			}
		}
	}
}
