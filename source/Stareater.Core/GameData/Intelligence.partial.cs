﻿using System;
using System.Collections.Generic;
using Ikadn;
using Ikadn.Ikon.Types;
using Stareater.Galaxy;
using Stareater.Galaxy.Builders;
using Stareater.Utils.Collections;

namespace Stareater.GameData
{
	partial class Intelligence
	{
		public void Initialize(IEnumerable<StarSystem> starSystems)
		{
			foreach(var system in starSystems)
				starKnowledge.Add(system.Star, new StarIntelligence(system.Planets));
		}
		
		public void StarFullyVisited(StarData star, int turn)
		{
			var starInfo = starKnowledge[star];
			
			starInfo.Visit(turn);
			foreach(var planetInfo in starInfo.Planets.Values) {
				planetInfo.Visit(turn);
				planetInfo.Explore(PlanetIntelligence.FullyExplored);
			}
		}
		
		public StarIntelligence About(StarData star)
		{
			return starKnowledge[star];
		}
	}
}
