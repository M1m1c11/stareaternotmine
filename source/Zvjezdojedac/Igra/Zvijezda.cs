﻿using System;
using System.Collections.Generic;
using System.Text;
using Zvjezdojedac.Alati;
using Zvjezdojedac.Podaci;

namespace Zvjezdojedac.Igra
{
	public class Zvijezda : IPohranjivoSB, IIdentifiable
	{
		public const int Tip_Nikakva = -1;
		public const int Tip_PocetnaPozicija = -2;
		public const int Tip_Nedodijeljen = -3;

		#region Statično i podrazred TipInfo
		public class TipInfo
		{
			public double udioPojave;

			public double velicinaMin;

			public double velicinaMax;

			public int zracenje;

			public class PojavnostPlaneta
			{
				public double tezinaPojave;
				public double gomiliste;
				public double unutarnjaUcestalost;
				public double vanjskaUcestalost;
				public double odstupanje;
				public PojavnostPlaneta(double tezinaPojave, double gomiliste, double unutarnjaUcestalost, double vanjskaUcestalost, double odstupanje)
				{
					this.tezinaPojave = tezinaPojave;
					this.gomiliste = gomiliste;
					this.unutarnjaUcestalost = unutarnjaUcestalost;
					this.vanjskaUcestalost = vanjskaUcestalost;
					this.odstupanje = odstupanje;
				}
			}

			public Dictionary<Planet.Tip, PojavnostPlaneta> pojavnostPlaneta;

			private TipInfo(double velicinaMin, double velicinaMax, double udioPojave, int zracenje, Dictionary<Planet.Tip, PojavnostPlaneta> pojavnostPlaneta)
			{
				this.velicinaMax = velicinaMax;
				this.velicinaMin = velicinaMin;
				this.udioPojave = udioPojave;
				this.zracenje = zracenje;
				this.pojavnostPlaneta = pojavnostPlaneta;
			}

			private static PojavnostPlaneta postaviPlanete(string podatci)
			{
				string[] pojediniPodatci= podatci.Split(new char[]{','});

                double tezinaPojave = Double.Parse(pojediniPodatci[0], PodaciAlat.DecimalnaTocka);
                double gomiliste = Double.Parse(pojediniPodatci[1], PodaciAlat.DecimalnaTocka);
                double unutarnjaUcestalost = Double.Parse(pojediniPodatci[2], PodaciAlat.DecimalnaTocka);
                double vanjskaUcestalost = Double.Parse(pojediniPodatci[3], PodaciAlat.DecimalnaTocka);
                double odstupanje = Double.Parse(pojediniPodatci[4], PodaciAlat.DecimalnaTocka);

				return new PojavnostPlaneta(tezinaPojave, gomiliste, unutarnjaUcestalost, vanjskaUcestalost, odstupanje);
			}

			public static void noviTip(Dictionary<string, string> podatci)
			{
				if (Tipovi == null)	Tipovi = new List<TipInfo>();
				int tip = Tipovi.Count;
				string tipStr = podatci["TIP"];

				Dictionary<Planet.Tip, PojavnostPlaneta> pojavnostPlaneta = new Dictionary<Planet.Tip, PojavnostPlaneta>();
				pojavnostPlaneta[Planet.Tip.NIKAKAV] = postaviPlanete(podatci["PLANETI_NIKAKVI"]);
				pojavnostPlaneta[Planet.Tip.ASTEROIDI] = postaviPlanete(podatci["PLANETI_ASTEROIDI"]);
				pojavnostPlaneta[Planet.Tip.KAMENI] = postaviPlanete(podatci["PLANETI_KAMENI"]);
				pojavnostPlaneta[Planet.Tip.PLINOVITI] = postaviPlanete(podatci["PLANETI_PLINOVITI"]);
                
				imeTipa.Add(podatci["TIP"], tip);
				Tipovi.Add(new TipInfo(
                        double.Parse(podatci["VELICINA_MIN"], PodaciAlat.DecimalnaTocka),
                        double.Parse(podatci["VELICINA_MAX"], PodaciAlat.DecimalnaTocka),
                        double.Parse(podatci["UCESTALOST"], PodaciAlat.DecimalnaTocka),
                        int.Parse(podatci["ZRACENJE"], PodaciAlat.DecimalnaTocka),
						pojavnostPlaneta
						));

				Slike.DodajZvjezdaMapaSliku(podatci["MAPA_SLIKA"], tip);
				Slike.DodajZvjezdaTabSliku(podatci["TAB_SLIKA"], tip);
			}
		}

		public static List<TipInfo> Tipovi = new List<TipInfo>();

		public static Dictionary<string, int> imeTipa = new Dictionary<string, int>();
		#endregion

		private int _tip;
		public double x;
		public double y;
		public List<Planet> planeti;
		public double velicina;
		public string ime;
		public int id { get; set; }
		public HashSet<Zvijezda> crvotocine = new HashSet<Zvijezda>();
		public ZvjezdanaUprava[] efektiPoIgracu = new ZvjezdanaUprava[IgraZvj.MaxIgraca];

		public Zvijezda(int id, int tip, double x, double y)
		{
			this.id = id;
			this._tip = tip;
			this.x = x;
			this.y = y;
			this.planeti = new List<Planet>();

			if (tip > Tip_Nikakva)
				this.velicina = Fje.IzIntervala(Fje.Random.NextDouble(), Tipovi[tip].velicinaMin, Tipovi[tip].velicinaMax);					
			else
				this.velicina = Fje.Random.NextDouble();
		}

		private Zvijezda(int id, int tip, double x, double y, double velicina, string ime)
		{
			this.id = id;
			this._tip = tip;
			this.x = x;
			this.y = y;
			this.planeti = new List<Planet>();
			this.velicina = velicina;
			this.ime = ime;
		}

		public int tip
		{
			get
			{
				return _tip;
			}
			set
			{
				double x = 0;
				if (_tip > Tip_Nikakva)
					x = (velicina - Tipovi[_tip].velicinaMin) / (Tipovi[_tip].velicinaMax - Tipovi[_tip].velicinaMin);
				else
					x = velicina;
				
				_tip = value;

				if (_tip > Tip_Nikakva) velicina = Tipovi[value].velicinaMin + x * (Tipovi[value].velicinaMax - Tipovi[value].velicinaMin);
			}
		}

		public override string ToString()
		{
			return ime;
		}

		public int zracenje()
		{
			return (int)Math.Round(Tipovi[tip].zracenje * velicina / Tipovi[tip].velicinaMax);
		}

		public void promjeniVelicinu(double v)
		{
			this.velicina = Tipovi[tip].velicinaMin +
				v * (Tipovi[tip].velicinaMax - Tipovi[tip].velicinaMin);
		}

		public double udaljenost(Zvijezda zvj)
		{
			return Math.Sqrt((this.x - zvj.x) * (this.x - zvj.x) + (this.y - zvj.y) * (this.y - zvj.y));
		}

		public double udaljenost(double x, double y)
		{
			return Math.Sqrt((this.x - x) * (this.x - x) + (this.y - y) * (this.y - y));
		}

		public void Naseli(Igrac igrac)
		{
			if (efektiPoIgracu[igrac.id] == null)
				efektiPoIgracu[igrac.id] = new ZvjezdanaUprava(this, igrac);
		}

		public void IzracunajEfekte()
		{
			foreach (var uprava in efektiPoIgracu)
				if (uprava != null)
					uprava.IzracunajEfekte();
		}

		public void NoviKrug()
		{
			foreach (var uprava in efektiPoIgracu)
				if (uprava != null)
					uprava.NoviKrug();
		}

		#region Pohrana
		public const string PohranaTip = "ZVIJEZDA";
		private const string PohTip = "TIP";
		private const string PohX = "X";
		private const string PohY = "Y";
		private const string PohVelicina = "VELICINA";
		private const string PohIme = "IME";
		private const string PohCrvotocine = "CRVOTOCINE";
		
		public void pohrani(PodaciPisac izlaz)
		{
			izlaz.dodaj(PohTip, tip);
			izlaz.dodaj(PohX, x);
			izlaz.dodaj(PohY, y);
			izlaz.dodaj(PohVelicina, velicina);
			izlaz.dodaj(PohIme, ime);
			for (int i = 0; i < planeti.Count; i++)
				izlaz.dodaj(Planet.PohranaTip + i, planeti[i]);
			izlaz.dodajIdeve(PohCrvotocine, crvotocine);
		}

		public static Zvijezda Ucitaj(PodaciCitac ulaz, int id)
		{
			int tip = ulaz.podatakInt(PohTip);
			double x = ulaz.podatakDouble(PohX);
			double y = ulaz.podatakDouble(PohY);
			double velicina = ulaz.podatakDouble(PohVelicina);
			string ime = ulaz.podatak(PohIme);

			Zvijezda zvj = new Zvijezda(id, tip, x, y, velicina, ime);
			if (tip >= 0)
				for (int i = 0; i < Mapa.GraditeljMape.BR_PLANETA; i++)
					zvj.planeti.Add(Planet.Ucitaj(ulaz[Planet.PohranaTip + i], zvj, i));

			return zvj;
		}

		public static int[] UcitajCrvotocine(PodaciCitac ulaz)
		{
			return ulaz.podatakIntPolje(PohCrvotocine);
		}
		#endregion
	}
}
