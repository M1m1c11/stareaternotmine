﻿using System;
using System.Collections.Generic;
using System.Linq;
using Stareater.Galaxy.BodyTraits;
using Stareater.GameData.Databases;
using Stareater.Utils.Collections;

namespace Stareater.Galaxy.Builders
{
	public class SystemEvaluator
	{
		private readonly StaticsDB statics;

		internal SystemEvaluator(StaticsDB statics)
		{
			this.statics = statics;
		}

		public double StartingScore(StarSystemBuilder system)
		{
			if (system == null)
				throw new ArgumentNullException(nameof(system));

			return this.StartingScore(system.Star, system.Planets);
		}

		internal double StartingScore(StarData star, IEnumerable<Planet> planets)
		{
			foreach (var trait in star.Traits)
				trait.InitialApply(this.statics, star, planets);
			
			return planets.Sum(x => this.startingScore(x));
		}

		public double PotentialScore(StarSystemBuilder system)
		{
			if (system == null)
				throw new ArgumentNullException(nameof(system));

			return this.PotentialScore(system.Star, system.Planets);
		}

		internal double PotentialScore(StarData star, IEnumerable<Planet> planets)
		{
			foreach (var trait in star.Traits)
				trait.InitialApply(this.statics, star, planets);

			return planets.Sum(planet =>
			{
				var typeInfo = this.statics.PlanetForumlas[planet.Type];
				var traits = new HashSet<string>(planet.Traits.Select(x => x.IdCode));
				traits.RemoveWhere(x => typeInfo.WorstTraits.Contains(x) && !typeInfo.UnchangeableTraits.Contains(x));
				traits.UnionWith(typeInfo.BestTraits.Where(x=> !typeInfo.UnchangeableTraits.Contains(x)));

				return this.potentialScore(new Planet(
					planet.Star, planet.Position, planet.Type, planet.Size, 
					traits.Select(x => this.statics.PlanetTraits[x])
				));
			});
		}

		private double startingScore(Planet planet)
		{
			var vars = new Var(GameLogic.ColonyProcessor.PlanetSizeKey, planet.Size).
				Init(this.statics.PlanetTraits.Keys, -1).
				UnionWith(this.statics.PlanetForumlas[planet.Type].ImplicitTraits).
				UnionWith(planet.Traits.Select(x => x.IdCode)).
				Get;

			return this.statics.PlanetForumlas[planet.Type].StartingScore.Evaluate(vars);
		}

		private double potentialScore(Planet planet)
		{
			var vars = new Var(GameLogic.ColonyProcessor.PlanetSizeKey, planet.Size).
				Init(this.statics.PlanetTraits.Keys, -1).
				UnionWith(this.statics.PlanetForumlas[planet.Type].ImplicitTraits).
				UnionWith(planet.Traits.Select(x => x.IdCode)).
				Get;

			return this.statics.PlanetForumlas[planet.Type].PotentialScore.Evaluate(vars);
		}
	}
}
