﻿using System.Linq;
using Ikadn.Ikon.Types;
using Stareater.Localization;

namespace Stareater.Galaxy
{
	public class StartingConditions
	{
		public const int MaxColonies = 8; //TODO(v0.8) move to map assets

		public int Colonies { get; private set; }
		public long Population { get; private set; }
		public long Infrastructure { get; private set; }

		private readonly string nameKey;

		public StartingConditions(long population, int colonies, long infrastructure, string nameKey)
		{
			this.Colonies = colonies;
			this.Population = population;
			this.Infrastructure = infrastructure;
			this.nameKey = nameKey;
		}

		public string Name
		{
			get
			{
				return LocalizationManifest.Get.CurrentLanguage["StartingConditions"][nameKey].Text();
			}
		}

		public IkonComposite BuildSaveData()
		{
			var lastGameData = new IkonComposite("StartinConditions")
			{
				{ ColoniesKey, new IkonInteger(Colonies) },
				{ PopulationKey, new IkonInteger(Population) },
				{ InfrastructureKey, new IkonInteger(Infrastructure) },
				{ NameKey, new IkonText(nameKey) }
			};

			return lastGameData;
		}

		internal static StartingConditions Load(IkonComposite ikstonData)
		{
			var requiredKeys = new[] { PopulationKey, ColoniesKey, InfrastructureKey, NameKey };
			if (!requiredKeys.All(ikstonData.Keys.Contains))
				return null;

			try
			{
				var population = ikstonData[PopulationKey].To<long>();
				var colonies = ikstonData[ColoniesKey].To<int>();
				var infrastructure = ikstonData[InfrastructureKey].To<long>();
				var nameKey = ikstonData[NameKey].To<string>();

				if (population < 0 || colonies < 0 || infrastructure < 0)
					return null;

				return new StartingConditions(population, colonies, infrastructure, nameKey);
			}
			catch
			{
				return null;
			}
		}

		#region Equals and GetHashCode implementation
		public override bool Equals(object obj)
		{
			var other = obj as StartingConditions;
			if (other == null)
				return false;

			return this.Colonies == other.Colonies &&
				this.Infrastructure == other.Infrastructure &&
				this.Population == other.Population;
		}

		public override int GetHashCode()
		{
			return Colonies.GetHashCode() + Population.GetHashCode() * 31 + Infrastructure.GetHashCode() * 967;
		}
		
		public static bool operator ==(StartingConditions lhs, StartingConditions rhs) {
			if (ReferenceEquals(lhs, rhs))
				return true;
			if (lhs is null || rhs is null)
				return false;
			return lhs.Equals(rhs);
		}

		public static bool operator !=(StartingConditions lhs, StartingConditions rhs) {
			return !(lhs == rhs);
		}
		#endregion

		#region Attribute keys
		const string ColoniesKey = "colonies";
		const string PopulationKey = "population";
		const string InfrastructureKey = "infrastructure";
		const string NameKey = "nameKey";
		#endregion
	}
}
