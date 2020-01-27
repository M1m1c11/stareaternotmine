﻿using Stareater.Galaxy;
using Stareater.GameData.Databases.Tables;
using Stareater.Players;
using Stareater.Utils;
using Stareater.Utils.StateEngine;
using System.Collections.Generic;

namespace Stareater.GameData.Databases
{
	class StatesDB
	{
		private const string DesignIdPrefix = "ShipDesign";
		
		[StateProperty]
		public StarCollection Stars { get; private set; }
		[StateProperty]
		public WormholeCollection Wormholes { get; private set; }
		[StateProperty]
		//TODO(v0.8) maybe give natives stellarises with special buildings and use it in
		//derivates to deduce which star is the brain
		public StarData StareaterBrain { get; private set; }

		[StateProperty]
		public PlanetCollection Planets { get; private set; }
		[StateProperty]
		public ColonyCollection Colonies { get; private set; }
		[StateProperty]
		public StellarisCollection Stellarises { get; private set; }

		[StateProperty]
		public ColonizationCollection ColonizationProjects { get; private set; }
		[StateProperty]
		public FleetCollection Fleets { get; private set; }

		[StateProperty]
		public DesignCollection Designs { get; private set; }
		[StateProperty]
		public ReportCollection Reports { get; private set; }
		[StateProperty]
		public DevelopmentProgressCollection DevelopmentAdvances { get; private set; }
		[StateProperty]
		public ResearchProgressCollection ResearchAdvances { get; private set; }
		[StateProperty]
		public HashSet<Pair<Player>> Contacts { get; internal set; }
		[StateProperty]
		public TreatyCollection Treaties { get; private set; }

		private int nextDesignId = 0; //TODO(v0.8) may not work correctly after loading
		
		public StatesDB(StarCollection stars, StarData stareaterBrain, WormholeCollection wormholes, PlanetCollection planets, 
		                ColonyCollection Colonies, StellarisCollection stellarises,
		                DevelopmentProgressCollection developmentAdvances, ResearchProgressCollection researchAdvances,
						HashSet<Pair<Player>> contacts, TreatyCollection treaties,ReportCollection reports, DesignCollection designs,
						FleetCollection fleets, ColonizationCollection colonizations)
		{
			this.Colonies = Colonies;
			this.Contacts = contacts;
			this.Planets = planets;
			this.Stars = stars;
			this.StareaterBrain = stareaterBrain;
            this.Stellarises = stellarises;
			this.Wormholes = wormholes;
			this.DevelopmentAdvances = developmentAdvances;
			this.ResearchAdvances = researchAdvances;
			this.Reports = reports;
			this.Designs = designs;
			this.Fleets = fleets;
			this.ColonizationProjects = colonizations;
			this.Treaties = treaties;
		}

		private StatesDB()
		{ }

		public string MakeDesignId()
		{
			this.nextDesignId++;
			
			return DesignIdPrefix + nextDesignId;
		}
	}
}
