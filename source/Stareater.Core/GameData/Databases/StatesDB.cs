﻿using System;
using Stareater.GameData.Databases.Tables;

namespace Stareater.GameData.Databases
{
	internal class StatesDB
	{
		public StarsCollection Stars { get; private set; }
		public WormholeCollection Wormholes { get; private set; }
		public PlanetsCollection Planets { get; private set; }
		
		public TechProgressCollection TechnologyProgresses { get; private set; }
		
		public StatesDB(StarsCollection stars, WormholeCollection wormholes, PlanetsCollection planets, TechProgressCollection technologyProgresses)
		{
			this.Planets = planets;
			this.Stars = stars;
			this.Wormholes = wormholes;
			this.TechnologyProgresses = technologyProgresses;
		}
	}
}
