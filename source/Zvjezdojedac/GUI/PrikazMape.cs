﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Zvjezdojedac.Igra;
using Zvjezdojedac.Podaci;

namespace Zvjezdojedac.GUI
{
	public class PrikazMape : IDisposable
	{
		private IgraZvj igra;

		private double minX;
		private double maxX;
		
		private double minY;
		private double maxY;

		private double skala;
		private Image _slikaMape = null;

		public Image slikaMape
		{
			get { return _slikaMape; }
		}

		public const int VELICINA_SLIKE_ZVIJEZDE = 48;

		public const int MIN_SKALA_MAPE = 10;

		public const int MAX_SKALA_MAPE = 100;

		public const int POCETNA_SKALA_MAPE = 50;

		public PrikazMape(IgraZvj igra)
		{
			this.igra = igra;
			this.skala = POCETNA_SKALA_MAPE;

			#region Određivanje dimenzija mape
			minX = igra.mapa.zvijezde[0].x;
			maxX = igra.mapa.zvijezde[0].x;
			minY = igra.mapa.zvijezde[0].y;
			maxY = igra.mapa.zvijezde[0].y;
			foreach (Zvijezda zvj in igra.mapa.zvijezde)
			{
				if (zvj.x < minX) minX = zvj.x;
				if (zvj.x > maxX) maxX = zvj.x;
				if (zvj.y < minY) minY = zvj.y;
				if (zvj.y > maxY) maxY = zvj.y;
			}
			#endregion

			osvjezi();
		}

		private Point xyZvijezde(Zvijezda zvj)
		{
			return new Point(
				(int)((zvj.x - minX + 1) * skala),
				(int)((zvj.y - minY + 1) * skala)
				);
		}

		private Point xyFlote(Flota flota)
		{
			return new Point(
				(int)((flota.x - minX + 1) * skala),
				(int)((flota.y - minY + 1) * skala)
				);
		}

		public Image osvjezi()
		{
			if (_slikaMape != null)
				_slikaMape.Dispose();

			Bitmap bmpSlika = new Bitmap((int) (skala * (maxX - minX + 2)), (int) (skala * (maxY - minY + 2)));
			Graphics g = Graphics.FromImage(bmpSlika);
			g.Clear(Color.Black);
			Font font = new Font(FontFamily.GenericSansSerif, 8, FontStyle.Bold);
			Igrac igrac = igra.trenutniIgrac();
			Pen crvPen = new Pen(Color.DarkBlue);
			Pen linijaOdredistePen = new Pen(Color.Green);

			foreach (Zvijezda zvj in igra.mapa.zvijezde)
				if (igrac.posjeceneZvjezde.Contains(zvj))
					foreach (Zvijezda odrediste in zvj.crvotocine)
						if (odrediste.id > zvj.id || !igrac.posjeceneZvjezde.Contains(odrediste))
							g.DrawLine(crvPen, xyZvijezde(zvj), xyZvijezde(odrediste));

			foreach (Zvijezda zvj in igra.mapa.zvijezde)
			{
				if (zvj.tip == Zvijezda.Tip_Nikakva)
					continue;

				Point xy = xyZvijezde(zvj);
				int d = (int)(Math.Sqrt(zvj.velicina) * VELICINA_SLIKE_ZVIJEZDE);

				g.DrawImage(
					Slike.ZvijezdaMapa[zvj.tip],
					new Rectangle(xy.X - d/2, xy.Y - d/2, d, d)
					);

				if (skala >= VELICINA_SLIKE_ZVIJEZDE / 2) {
					Brush bojaImena;
					if (igrac.posjeceneZvjezde.Contains(zvj)) {
						bool imaKoloniju = false;
						foreach (Planet planet in zvj.planeti)
							if (planet.kolonija != null)
								if (planet.kolonija.Igrac.id == igrac.id)
									imaKoloniju = true;
						bojaImena = (imaKoloniju) ? 
							new SolidBrush(igrac.boja) :
							new SolidBrush(Color.FromArgb(192, 192, 192));
					}
					else
						bojaImena = new SolidBrush(Color.FromArgb(64, 64, 64));
					g.DrawString(zvj.ime, font, bojaImena, xy.X - g.MeasureString(zvj.ime, font).Width / 2, xy.Y + VELICINA_SLIKE_ZVIJEZDE / 2);
				}

				if (igrac.floteStacionarne.ContainsKey(zvj))
					g.DrawImage(Slike.Flota[igrac.boja], xy.X + VELICINA_SLIKE_ZVIJEZDE / 2, xy.Y - VELICINA_SLIKE_ZVIJEZDE / 2);

				if (zvj == igrac.odabranaZvijezda)
				{
					Image img = Slike.SlikaOdabiraZvijezde[0];
					g.DrawImage(img, new Rectangle(xy.X - img.Width/2, xy.Y - img.Height/2, img.Width, img.Height));
				}
				else if (zvj == igrac.odredisnaZvijezda) {
					Image img = Slike.SlikaOdabiraZvijezde[1];
					g.DrawImage(img, new Rectangle(xy.X - img.Width / 2, xy.Y - img.Height / 2, img.Width, img.Height));
				}
			}

			foreach (PokretnaFlota flota in igrac.flotePokretne) {
				Point xy = xyFlote(flota);
				Point xyOdrediste = xyZvijezde(flota.odredisnaZvj);
				g.DrawLine(linijaOdredistePen, xy, xyOdrediste);

				xy.X -= Slike.Flota[igrac.boja].Size.Width / 2;
				xy.Y -= Slike.Flota[igrac.boja].Size.Height / 2;
				g.DrawImage(Slike.Flota[igrac.boja], xy);
			}

			g.Dispose();

			this._slikaMape = bmpSlika;
			return bmpSlika;
		}

		public double XnaMapi(int x)
		{
			return x / skala + minX - 1;
		}

		public double YnaMapi(int y)
		{
			return y / skala + minY - 1;
		}

		public int XsaMape(double x)
		{
			return (int)((x - minX + 1) * skala);
		}

		public int YsaMape(double y)
		{
			return (int)((y - minY + 1) * skala);
		}

		public void zoom(double skala)
		{
			this.skala = MIN_SKALA_MAPE + skala * (MAX_SKALA_MAPE - MIN_SKALA_MAPE);
			osvjezi();
		}

		#region IDisposable Members

		public void Dispose()
		{
			if (_slikaMape != null)
				_slikaMape.Dispose();
		}

		#endregion
	}
}
