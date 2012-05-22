﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Zvjezdojedac.Podaci.Formule;

namespace Zvjezdojedac.Igra.Brodovi
{
	public class Potisnici : Komponenta<Potisnici.PotisnikInfo>
	{
		public class PotisnikInfo : AKomponentaInfo
		{
			#region Statično
			public static List<PotisnikInfo> Potisnici = new List<PotisnikInfo>();

			public static void UcitajPotisnikInfo(Dictionary<string, string> podaci)
			{
				string naziv = podaci["IME"];
				string opis = podaci["OPIS"];
				Image slika = Image.FromFile(podaci["SLIKA"]);
				List<Preduvjet> preduvjeti = Preduvjet.NaciniPreduvjete(podaci["PREDUVJETI"]);
				int maxNivo = int.Parse(podaci["MAX_NIVO"]);

				Formula brzina = Formula.IzStringa(podaci["BRZINA"]);
				Formula pokretljivost = Formula.IzStringa(podaci["POKRETLJIVOST"]);

				Potisnici.Add(new PotisnikInfo(
					naziv, opis, slika, preduvjeti, maxNivo,
					brzina, pokretljivost)
					);
			}

			public static IEnumerable<Potisnici> Dostupni(Dictionary<string, double> varijable)
			{
				List<Potisnici> rez = new List<Potisnici>();
				foreach (PotisnikInfo pi in Potisnici)
					if (pi.dostupno(varijable))
						rez.Add(pi.naciniKomponentu(varijable));
				return rez;
			}

			public static Potisnici NajboljiPotisnici(Dictionary<string, double> varijable)
			{
				double max = double.MinValue;
				Potisnici naj = null;

				foreach(PotisnikInfo pi in Potisnici)
					if (pi.dostupno(varijable, 0))
					{
						Potisnici trenutni = pi.naciniKomponentu(varijable);

						if (trenutni.brzina > max)
						{
							max = trenutni.brzina;
							naj = trenutni;
						}
					}

				return naj;
			}

			public static PotisnikInfo IzIda(int id)
			{
				foreach (PotisnikInfo info in Potisnici)
					if (info.id == id)
						return info;
				throw new ArgumentException("Nepostojeći id potisnika.");
			}
			#endregion

			private Formula brzina;
			private Formula pokretljivost;

			private PotisnikInfo(string naziv, string opis, Image slika,
				List<Preduvjet> preduvjeti, int maxNivo,
				Formula brzina, Formula pokretljivost)
				:
				base(naziv, opis, slika, preduvjeti, maxNivo)
			{
				this.brzina = brzina;
				this.pokretljivost = pokretljivost;
			}

			public Potisnici naciniKomponentu(Dictionary<string, double> varijable)
			{
				int nivo = maxDostupanNivo(varijable);
				return naciniKomponentu(nivo);
			}

			public Potisnici naciniKomponentu(int nivo)
			{
				return new Potisnici(
					this,
					nivo,
					Evaluiraj(brzina, nivo),
					Evaluiraj(pokretljivost, nivo)
					);
			}
		}

		public double brzina { get; private set; }
		public double pokretljivost { get; private set; }

		private Potisnici(PotisnikInfo info, int nivo, double brzina, double pokretljivost)
			: base(info, nivo)
		{
			this.brzina = brzina;
			this.pokretljivost = pokretljivost;
		}
	}
}
