using System;
using System.Collections.Generic;
using System.Linq;

using Stareater.AppData.Expressions;
using Stareater.Galaxy;
using Stareater.GameData;
using Stareater.GameData.Databases;
using Stareater.GameData.Databases.Tables;
using Stareater.GameData.Ships;
using Stareater.Players;
using Stareater.Players.Reports;
using Stareater.Ships;
using Stareater.Ships.Missions;
using Stareater.Utils;
using Stareater.Utils.Collections;
using Stareater.Utils.StateEngine;
using Stareater.GameLogic.Planning;
using Stareater.GameLogic.Combat;

namespace Stareater.GameLogic
{
	class PlayerProcessor
	{
		public const string LevelSufix = "Lvl";
		public const string UpgradeSufix = "Upg";

		[StatePropertyAttribute]
		public Player Player { get; private set; }

		[StatePropertyAttribute]
		public IEnumerable<DevelopmentResult> DevelopmentPlan { get; protected set; }
		[StatePropertyAttribute]
		public IEnumerable<ResearchResult> ResearchPlan { get; protected set; }
		[StatePropertyAttribute]
		public IDictionary<string, double> TechLevels { get; private set; }
		[StatePropertyAttribute]
		public double MaintenanceRatio { get; private set; }

		[StatePropertyAttribute]
		public Dictionary<Design, DesignStats> DesignStats { get; private set; }
		[StatePropertyAttribute]
		public Dictionary<Design, Dictionary<Design, double>> RefitCosts { get; private set; }
		[StatePropertyAttribute]
		public List<Design> ColonizerDesignOptions { get; private set; }
		[StatePropertyAttribute]
		public QuadTree<Circle> ScanRanges { get; private set; }

		[StatePropertyAttribute]
		public bool ControlsStareater { get; private set; }
		[StatePropertyAttribute]
		public double EjectEta { get; private set; }
		//TODO(v0.8) make a class which unifies ejection data
		[StatePropertyAttribute]
		public Dictionary<Player, double> EjectVictoryPoints { get; private set; } 

		public PlayerProcessor(Player player, IEnumerable<DevelopmentTopic> technologies)
		{
			this.Player = player;

			this.DevelopmentPlan = null;
			this.ResearchPlan = null;
			this.DesignStats = new Dictionary<Design, DesignStats>();
			this.RefitCosts = new Dictionary<Design, Dictionary<Design, double>>();
			this.ScanRanges = new QuadTree<Circle>();
			this.TechLevels = new Dictionary<string, double>();

			foreach (var tech in technologies)
			{
				this.TechLevels.Add(tech.IdCode + LevelSufix, 0);
				this.TechLevels.Add(tech.IdCode + UpgradeSufix, DevelopmentProgress.NotStarted);
			}
		}

		public PlayerProcessor(Player player)
		{
			this.Player = player;
		}

        private PlayerProcessor()
        { }
		
		public void Initialize(MainGame game)
		{
            this.initTechAdvances(game.States.DevelopmentAdvances.Of[this.Player]);
            this.unlockPredefinedDesigns(game);

			foreach (var design in game.States.Designs.OwnedBy[this.Player])
				this.Analyze(design, game.Statics);

			this.updateDesigns(game);
			this.CalculateStareater(game);
		}

		public void CalculateBaseEffects(MainGame game)
		{
			var maintenanceCost = game.Derivates.Colonies.OwnedBy[this.Player].Sum(x => x.MaintenanceCost);
            var availabeMaintenance = game.Derivates.Colonies.OwnedBy[this.Player].Sum(x => x.MaintenanceLimit);
			var maintenanceLimit = 0.5; //TODO(later) make player adjustable

			if (maintenanceCost > availabeMaintenance * maintenanceLimit)
			{
				availabeMaintenance *= maintenanceLimit;
                var needMaintenance = game.Derivates.Colonies.OwnedBy[this.Player].
					Where(x => x.MaintenanceCost > 0).
					OrderBy(x => x.MaintenancePerPop);

				foreach (var colony in needMaintenance)
				{
					var spent = Math.Min(availabeMaintenance, colony.MaintenanceCost);
					availabeMaintenance -= spent;
					colony.MaintenancePenalty = colony.MaintenancePerPop * (1 - spent / colony.MaintenanceCost);
                }
			}

			var fuelDeficit = 
				game.States.Fleets.OwnedBy[this.Player].Sum(x => this.FuelUsage(x, game)) - 
				game.Derivates.Colonies.OwnedBy[this.Player].Sum(x => x.FuelProduction);
			if (fuelDeficit > 0)
				maintenanceCost += fuelDeficit * game.Statics.ColonyFormulas.FuelCost.Evaluate(null);

			this.MaintenanceRatio = Methods.Clamp(
				maintenanceCost / availabeMaintenance, 
				0, 
				game.Statics.ColonyFormulas.MaintenanceTotalLimit.Evaluate(null));

			this.ScanRanges.Clear();
			foreach (var stellaris in game.Derivates.Stellarises.OwnedBy[this.Player])
			{
				var range = new Circle(stellaris.Location.Position, stellaris.ScanRange);
				this.ScanRanges.Add(range, range.Center, new Vector2D(range.Radius * 2, range.Radius * 2));
			}
			foreach(var fleet in game.States.Fleets.OwnedBy[this.Player])
			{
				var range = new Circle(fleet.Position, fleet.Ships.Max(x => this.DesignStats[x.Design].ScanRange));
				this.ScanRanges.Add(range, range.Center, new Vector2D(range.Radius * 2, range.Radius * 2));
			}
		}

		public void CalculateIsMigration(MainGame game)
		{
			var stellarises = game.Derivates.Stellarises.OwnedBy[this.Player];
			var demand = stellarises.ToDictionary(
				x => x, 
				x => x.MissingPopulation - x.Stellaris.IncomingMigrants.Sum(pop => pop.Value)
			);

			foreach (var stellaris in stellarises)
				stellaris.IsMigrationPlan = new Dictionary<StarData, double>();

			foreach (var stellaris in stellarises.Where(x => x.IsMigrants > 0).OrderBy(x => x.IsMigrants))
			{
				Methods.DistributePoints(
					stellaris.IsMigrants,
					demand.
						Where(x => x.Key != stellaris).
						Select(x => new PointReceiver(
						   x.Value,
						   x.Value,
						   pop => { stellaris.IsMigrationPlan[x.Key.Location] = pop; }
					   )
				));
			}
		}

		#region Technology related
		public void CalculateDevelopment(MainGame game, IList<ColonyProcessor> colonyProcessors)
		{
			double developmentPoints = 0;
			
			foreach (var colonyProc in colonyProcessors)
				developmentPoints += colonyProc.Development;
			
			var focus = game.Statics.DevelopmentFocusOptions[game.Orders[Player].DevelopmentFocusIndex];
			var advanceOrder = this.DevelopmentOrder(game.States.DevelopmentAdvances, game.States.ResearchAdvances, game).ToList();
			
			var results = new List<DevelopmentResult>();
			Methods.DistributePoints(
				developmentPoints, 
				advanceOrder.Take(focus.Weights.Length).Select((techProgress, i) => new PointReceiver(
					focus.Weights[i],
					techProgress.InvestmentLimit,
					p => results.Add(techProgress.SimulateInvestment(p))
				))
			);

			//TODO(v0.8) do something with leftover points
			
			this.DevelopmentPlan = results;
		}

		public void CalculateResearch(MainGame game)
		{
			var advanceOrder = this.ResearchOrder(game.States.ResearchAdvances).ToList();
			string focused = game.Orders[Player].ResearchFocus;
			
			if (advanceOrder.Count > 0 && advanceOrder.All(x => x.Topic.IdCode != focused))
				focused = advanceOrder[0].Topic.IdCode;
			
			var results = new List<ResearchResult>();
			Methods.DistributePoints(1, advanceOrder.Select(techProgress => new PointReceiver(
				techProgress.Topic.IdCode == focused ? game.Statics.PlayerFormulas.FocusedResearchWeight : 1,
				techProgress.InvestmentLimit,
				p => results.Add(techProgress.SimulateInvestment(p))
			)));

			this.ResearchPlan = results;
		}
		
		public void InvalidateDevelopment()
		{
			this.DevelopmentPlan = null;
		}
		
		public void InvalidateResearch()
		{
			this.ResearchPlan = null;
		}
		
		public IEnumerable<DevelopmentProgress> DevelopmentOrder(DevelopmentProgressCollection developmentAdvances, ResearchProgressCollection researchAdvances, MainGame game)
		{
			var researchLevels = researchAdvances.Of[this.Player].ToDictionary(x => x.Topic.IdCode, x => (double)x.Level);
			var playerTechs = developmentAdvances
				.Of[this.Player]
				.Where(x => (x.CanProgress(researchLevels, game.Statics)))
				.ToList();
			var orders = game.Orders[this.Player].DevelopmentQueue;

			var orderedTech = playerTechs.
				Where(x => orders.ContainsKey(x.Topic.IdCode)).
				OrderBy(x => orders[x.Topic.IdCode]).
				ToList();
			orderedTech.AddRange(
				playerTechs.
				Where(x => !orders.ContainsKey(x.Topic.IdCode)).
				OrderBy(x => x.Topic.IdCode)
            );
			
			return orderedTech;
		}
		
		public IEnumerable<ResearchProgress> ResearchOrder(ResearchProgressCollection techAdvances)
		{
			var playerTechs = techAdvances
				.Of[Player]
				.Where(x =>	x.CanProgress)
				.OrderBy(x => x.Topic.IdCode)
				.ToList();
			
			return playerTechs;
		}

		private void initTechAdvances(IEnumerable<DevelopmentProgress> techAdvances)
		{
			foreach (var tech in techAdvances)
			{
				this.TechLevels[tech.Topic.IdCode + LevelSufix] = tech.Level == DevelopmentProgress.NotStarted ? 0 : (tech.Level + 1);
				this.TechLevels[tech.Topic.IdCode + UpgradeSufix] = tech.Level;
			}
		}
		#endregion

		#region Galaxy phase
		public bool CanSee(Fleet fleet, MainGame game)
		{
			if (fleet.Owner == this.Player)
				return !game.Orders[fleet.Owner].ShipOrders.ContainsKey(fleet.Position);
			else
				return this.ScanRanges.
					Query(fleet.Position, new Vector2D()).
					Any(x => (x.Center - fleet.Position).Length <= x.Radius);
		}

		public void CalculateStareater(MainGame game)
		{
			this.ControlsStareater = game.States.Fleets.
					At[game.States.StareaterBrain.Position, this.Player].
					Any();

			this.EjectEta = this.ControlsStareater ? 1 : 0; //TODO(later) calculate ETA
			this.EjectVictoryPoints = game.MainPlayers.ToDictionary(x => x, x => 0.0);
			var targetStar = game.Orders[this.Player].EjectingStar;
			var vars = new Var("pop", 0).And("turn", game.Turn).Get;

			if (this.ControlsStareater && targetStar != null)
			{
				foreach (var colony in game.States.Colonies.AtStar[targetStar])
				{
					vars["pop"] = colony.Population;
					this.EjectVictoryPoints[colony.Owner] = game.Statics.ColonyFormulas.VictoryPointWorth.Evaluate(vars);
				}
			}
		}

		public IEnumerable<Fleet> MyFleets(MainGame game)
		{
			var orders = game.Orders[this.Player].ShipOrders;

			return game.States.Fleets.
				OwnedBy [this.Player].
				Where(x => !orders.ContainsKey(x.Position)).
				Concat(orders.SelectMany(x => x.Value));
		}

		public double TotalFuelUsage(MainGame game)
		{
			return this.MyFleets(game).Sum(fleet => this.FuelUsage(fleet, game));
		}

		public double FuelUsage(Fleet fleet, MainGame game)
		{
			var order = fleet.Missions.SkipWhile(x => !(x is MoveMission) && !x.FullTurnAction).FirstOrDefault();

			if (order is MoveMission moveOrder)
				return this.FuelUsage(fleet, moveOrder.Start.Position, moveOrder.Destination.Position, game);
			else
				return this.FuelUsage(fleet, fleet.Position, fleet.Position, game);
		}

		public double FuelUsage(Fleet fleet, Vector2D fromPosition, Vector2D toPosition, MainGame game)
		{
			//TODO(v0.8) make temporary stat for fleets
			var fleetSize = fleet.Ships.Sum(x => x.Design.UsesFuel ? this.DesignStats[x.Design].Size * x.Quantity : 0);
			var stellarises = game.States.Stellarises.OwnedBy[this.Player];

			if (fleetSize <= 0)
				return 0;
			else if (fromPosition == toPosition)
				return fleetSize * game.Statics.ShipFormulas.FuelUsage.Evaluate(new Var("dist", 0).Get);
			else if (stellarises.Any())
			{
				var seed = stellarises.First().Location.Star.Position;
				var intervals = new LinkedList<FuelSupply>();
				intervals.AddLast(new FuelSupply(fromPosition, seed, fromPosition, toPosition));
				intervals.AddLast(new FuelSupply(toPosition, seed, fromPosition, toPosition));

				foreach (var center in stellarises.Skip(1).Select(x => x.Location.Star.Position))
				{
					var node = intervals.First;
					while(node.Next != null)
					{
						var intersection = FuelSupply.Intersection(node.Value, node.Next.Value, center);

						node.Value = node.Value.Improve(center);

						if (intersection != null)
						{
							intervals.AddAfter(node, intersection);
							node = node.Next;
						}

						node = node.Next;
					}

					intervals.Last.Value = intervals.Last.Value.Improve(center);
				}

				var supplyDistance = intervals.Max(x => x.Distance);
				return fleetSize * game.Statics.ShipFormulas.FuelUsage.Evaluate(new Var("dist", supplyDistance).Get);
			}
			else
				return double.PositiveInfinity;
		}

		public IEnumerable<Move<StarData>> ShortestPathTo(StarData fromStar, StarData toStar, Dictionary<Design, long> shipGroups, MainGame game)
		{
			var speedFormula = game.Statics.ShipFormulas.GalaxySpeed;
			var voidSpeed = speedFormula.Evaluate(FleetProcessor.SpeedVars(game.Statics, this, shipGroups, false).Get);
			var laneSpeed = speedFormula.Evaluate(FleetProcessor.SpeedVars(game.Statics, this, shipGroups, true).Get);

			//TODO(later) cache result
			return Methods.AStar(
				fromStar, toStar,
				x => (x.Position - toStar.Position).Length / laneSpeed,
				(a, b) => (a.Position - b.Position).Length / (this.VisibleWormholeAt(a, b, game) != null ? laneSpeed : voidSpeed),
				x => game.States.Stars
			);
		}

		public Wormhole VisibleWormholeAt(StarData starA, StarData starB, MainGame game)
		{
			var wormhole = game.States.Wormholes.At.GetOrDefault(starA, starB);

			return wormhole != null && this.Player.Intelligence.StarlaneKnowledge[wormhole] ? wormhole : null;
		}
		#endregion

		#region Precombat processing
		public void ProcessPrecombat(MainGame game)
		{
			this.updateColonizationOrders(game);

			foreach (var colonyProc in game.Derivates.Colonies.OwnedBy[this.Player])
				colonyProc.ProcessPrecombat(game);

			foreach (var stellarisProc in game.Derivates.Stellarises.OwnedBy[this.Player])
				stellarisProc.ProcessPrecombat(game);
		}
		
		public void SpawnShip(StarData star, Design design, long quantity, double population, IEnumerable<AMission> missions, StatesDB states)
		{
			var missionList = new LinkedList<AMission>(missions);
			var fleet = states.Fleets.At[star.Position, this.Player].FirstOrDefault(x => x.Missions.SequenceEqual(missionList));
			
			if (fleet == null)
			{
				fleet = new Fleet(this.Player, star.Position, missionList);
				states.Fleets.Add(fleet);
			}

			if (fleet.Ships.WithDesign.Contains(design))
			{
				fleet.Ships.WithDesign[design].Quantity += quantity;
				fleet.Ships.WithDesign[design].PopulationTransport += population;
			}
			else
				fleet.Ships.Add(new ShipGroup(design, quantity, 0, 0, population));
#if DEBUG
			Stareater.Controllers.GameController.ShipCounter.Add(design, quantity);
#endif
		}

		private void updateColonizationOrders(MainGame game)
		{
			foreach(var project in game.States.ColonizationProjects.OwnedBy[this.Player])
				if (!game.Orders[this.Player].ColonizationTargets.Contains(project.Destination))
					game.States.ColonizationProjects.PendRemove(project);
			
			foreach(var order in game.Orders[this.Player].ColonizationTargets)
				if (game.States.ColonizationProjects.Of[order].All(x => x.Owner != this.Player))
					game.States.ColonizationProjects.PendAdd(new ColonizationProject(this.Player, order));

			game.States.ColonizationProjects.ApplyPending();
		}
		#endregion
		
		#region Postcombat processing
		public void ProcessPostcombat(MainGame game)
		{
			this.advanceTechnologies(game);
			this.checkColonizationValidity(game);
			this.doConstruction(game);
			this.unlockPredefinedDesigns(game);
			this.updateDesigns(game);
			this.CalculateStareater(game);
			this.checkNewContacts(game);
		}

		private void advanceTechnologies(MainGame game)
		{
			foreach (var techProgress in this.DevelopmentPlan)
			{
				techProgress.Item.Progress(techProgress);
				if (techProgress.CompletedCount > 0)
					game.States.Reports.Add(new DevelopmentReport(techProgress));
			}
			foreach (var techProgress in this.ResearchPlan)
			{
				//TODO(v0.9) limit to at most 1 level per turn
				techProgress.Item.Progress(techProgress);
				if (techProgress.CompletedCount > 0)
				{
					game.States.Reports.Add(new ResearchReport(techProgress));
					var field = techProgress.Item.Topic;
					var unlockIds = game.Orders[this.Player].ResearchPriorities.ContainsKey(field.IdCode) ?
						game.Orders[this.Player].ResearchPriorities[field.IdCode] :
						field.Unlocks[techProgress.Item.Level];
					
					for (int priority = 0; priority < unlockIds.Length; priority++)
						game.States.DevelopmentAdvances.
							Of[this.Player, unlockIds[priority]].Priority = priority;

					game.Orders[this.Player].ResearchPriorities.Remove(field.IdCode);
				}
			}
			this.initTechAdvances(game.States.DevelopmentAdvances.Of[this.Player]);

			var researchLevels = game.States.ResearchAdvances.Of[this.Player].ToDictionary(x => x.Topic.IdCode, x => (double)x.Level);
			var validTechs = new HashSet<string>(
					game.States.DevelopmentAdvances
					.Where(x => x.CanProgress(researchLevels, game.Statics))
					.Select(x => x.Topic.IdCode)
				);

			game.Orders[this.Player].DevelopmentQueue = updateTechQueue(game.Orders[this.Player].DevelopmentQueue, validTechs);
		}

		private void checkColonizationValidity(MainGame game)
		{
			var occupiedTargets = new HashSet<Planet>();
			foreach(var order in game.Orders[this.Player].ColonizationTargets)
				if (game.States.Colonies.AtPlanet.Contains(order)) //TODO(v0.8) use intelligence instead
					occupiedTargets.Add(order);
			foreach(var planet in occupiedTargets)
				game.Orders[this.Player].ColonizationTargets.Remove(planet);
		}

		private void doConstruction(MainGame game)
		{
			var plans = game.Orders[this.Player].ConstructionPlans;

			foreach (var colony in game.States.Colonies.OwnedBy[Player])
				if (plans.ContainsKey(colony)) {
					var updatedPlans = updateConstructionPlans(
						game.Statics,
						plans[colony],
						game.Derivates[colony]
					);

					plans[colony] = updatedPlans;
				}
				else
					plans.Add(colony, new ConstructionOrders(PlayerOrders.DefaultSiteSpendingRatio));

			foreach (var stellaris in game.States.Stellarises.OwnedBy[Player])
				if (plans.ContainsKey(stellaris))
				{
					var updatedPlans = updateConstructionPlans(
						game.Statics,
						plans[stellaris],
						game.Derivates[stellaris]
					);

					plans[stellaris] = updatedPlans;
				}
				else
					plans.Add(stellaris, new ConstructionOrders(PlayerOrders.DefaultSiteSpendingRatio));
		}

		private void updateDesigns(MainGame game)
		{
			//Generate upgraded designs
			var upgradesTo = new Dictionary<Design, Design>();
			var newDesigns = new HashSet<Design>();
			foreach(var design in game.States.Designs.OwnedBy[this.Player])
			{
				var upgrade = this.DesignUpgrade(design, game.Statics, game.States);
				if (game.States.Designs.Contains(upgrade))
					continue;
				
				if (newDesigns.Contains(upgrade))
					upgrade = newDesigns.First(x => x == upgrade);
				else
					this.Analyze(upgrade, game.Statics);
				
				design.IsObsolete = true;
				upgradesTo[design] = upgrade;
				newDesigns.Add(upgrade);
			}
			game.States.Designs.Add(newDesigns);

			//Update refit orders to upgrade obsolete designs
			var orders = game.Orders[this.Player].RefitOrders;
			foreach (var upgrade in upgradesTo)
				if (!orders.ContainsKey(upgrade.Key))
					orders[upgrade.Key] = upgrade.Value;
				else if (orders[upgrade.Key].IsObsolete)
					orders[upgrade.Key] = upgradesTo[orders[upgrade.Key]];
			
			foreach(var site in game.Orders[this.Player].ConstructionPlans.Keys.ToList())
			{
				var updater = new ShipConstructionUpdater(
					game.Orders[this.Player].ConstructionPlans[site].Queue,
					game.Orders[this.Player].RefitOrders,
					game.Derivates[this.Player].DesignStats
				);
				game.Orders[this.Player].ConstructionPlans[site].Queue.Clear();
				game.Orders[this.Player].ConstructionPlans[site].Queue.AddRange(updater.Run());
			}

			//Removing inactive discarded designs
			var shipConstruction = new ShipConstructionCounter();
			shipConstruction.Check(game.Derivates.Stellarises.OwnedBy[this.Player]);
			
			var activeDesigns = new HashSet<Design>(game.States.Fleets.
			                                        SelectMany(x => x.Ships).
			                                        Select(x => x.Design).
			                                        Concat(shipConstruction.Designs));
			var discardedDesigns = game.Orders[this.Player].DiscardOrders.
				Where(x => !activeDesigns.Contains(x)).
				ToList();
			
			foreach(var design in discardedDesigns)
			{
				game.Orders[design.Owner].DiscardOrders.Remove(design);
				game.Orders[design.Owner].RefitOrders.Remove(design);
				game.States.Designs.Remove(design);
				this.DesignStats.Remove(design);
				this.RefitCosts.Remove(design);
			}
			foreach(var design in this.RefitCosts.Keys)
				foreach(var discarded in discardedDesigns)
					this.RefitCosts[design].Remove(discarded);

			//TODO(v0.8) doesn't include obsolete (to be upgraded) designs
			this.ColonizerDesignOptions = this.DesignStats.
				Where(x => x.Value.ColonizerPopulation > 0 && !orders.ContainsKey(x.Key)).
				Select(x => x.Key).ToList();

			if (!this.ColonizerDesignOptions.Contains(game.Orders[this.Player].ColonizerDesign))
				game.Orders[this.Player].ColonizerDesign = this.ColonizerDesignOptions.First();
		}

		private void checkNewContacts(MainGame game)
		{
			var uncontacted = game.MainPlayers.
				Where(x => x != this.Player && !game.States.Contacts.Contains(new Pair<Player>(x, this.Player))).
				ToList();

			foreach(var otherPlayer in uncontacted)
			{
				var seeFleet = game.States.Fleets.OwnedBy[otherPlayer].
					Any(fleet => this.ScanRanges.
						Query(fleet.Position).
						Any(circle => circle.Contains(fleet.Position))
					);
				//TODO(v0.9) query stellarises instead, ensure there are no stellarises without colonies
				var seeColony = game.States.Colonies.OwnedBy[otherPlayer].
					Any(colony => this.ScanRanges.
						Query(colony.Star.Position).
						Any(circle => circle.Contains(colony.Star.Position))
					);

				//TODO(later) produce a report message
				if (seeColony || seeFleet)
				{
					game.States.Contacts.Add(new Pair<Player>(otherPlayer, this.Player));
					game.States.Reports.Add(new ContactReport(this.Player, otherPlayer));
					game.States.Reports.Add(new ContactReport(otherPlayer, this.Player));
				}
			}
		}
		#endregion

		#region Design stats
		public static Var DesignBaseVars(
			Component<HullType> hull, IEnumerable<Component<MissionEquipmentType>> missionEquipment, 
			IEnumerable<Component<SpecialEquipmentType>> specialEquipment, StaticsDB statics)
		{
			var hullVars = new Var(AComponentType.LevelKey, hull.Level).Get;
			
			var shipVars = new Var("hullHp", hull.TypeInfo.ArmorBase.Evaluate(hullVars)).
				And("hullSensor", hull.TypeInfo.SensorsBase.Evaluate(hullVars)).
				And("hullCloak", hull.TypeInfo.CloakingBase.Evaluate(hullVars)).
				And("hullJamming", hull.TypeInfo.JammingBase.Evaluate(hullVars)).
				And("hullInertia", hull.TypeInfo.InertiaBase.Evaluate(hullVars)).
				And("hullSize", hull.TypeInfo.SpaceFree.Evaluate(hullVars)).
				Init(statics.SpecialEquipment.Keys, 0).
				Init(statics.SpecialEquipment.Keys.Select(x => x + AComponentType.LevelSuffix), 0).
				UnionWith(specialEquipment, x => x.TypeInfo.IdCode, x => x.Quantity).
				UnionWith(specialEquipment, x => x.TypeInfo.IdCode + AComponentType.LevelSuffix, x => x.Level).
				Init(statics.MissionEquipment.Keys, 0).
				Init(statics.MissionEquipment.Keys.Select(x => x + AComponentType.LevelSuffix), 0).
				UnionWith(missionEquipment, x => x.TypeInfo.IdCode, x => x.Quantity).
				UnionWith(missionEquipment, x => x.TypeInfo.IdCode + AComponentType.LevelSuffix, x => x.Level);

			return shipVars;
		}
		
		public static Var DesignPoweredVars(
			Component<HullType> hull, Component<ReactorType> reactor,
			IEnumerable<Component<MissionEquipmentType>> missionEquipment, IEnumerable<Component<SpecialEquipmentType>> specialEquipment,
			StaticsDB statics)
		{
			var shipVars = DesignBaseVars(hull, missionEquipment, specialEquipment, statics);
			
			shipVars[AComponentType.LevelKey] = reactor.Level;
			shipVars[ReactorType.SizeKey] = statics.ShipFormulas.ReactorSize.Evaluate(shipVars.Get);
			shipVars[ReactorType.TotalPowerKey] = reactor.TypeInfo.Power.Evaluate(shipVars.Get);
			
			return shipVars;
		}

		public Design DesignUpgrade(Design oldDesign, StaticsDB statics, StatesDB states)
		{
			var techLevels = states.DevelopmentAdvances.Of[this.Player].ToDictionary(x => x.Topic.IdCode, x => (double)x.Level);
			
			var hull = statics.Hulls[oldDesign.Hull.TypeInfo.IdCode].MakeHull(techLevels);
			var specials = oldDesign.SpecialEquipment.Select(
				x => statics.SpecialEquipment[x.TypeInfo.IdCode].MakeBest(techLevels, x.Quantity)
			).ToList();
			var equipment = oldDesign.MissionEquipment.Select(
				x => statics.MissionEquipment[x.TypeInfo.IdCode].MakeBest(techLevels, x.Quantity)
			).ToList();

			var armor = AComponentType.MakeBest(statics.Armors.Values, techLevels);
			var reactor = ReactorType.MakeBest(techLevels, hull, specials, equipment, statics);
			var isDrive = oldDesign.IsDrive != null ? IsDriveType.MakeBest(techLevels, hull, reactor, specials, equipment, statics) : null;
			var sensor = AComponentType.MakeBest(statics.Sensors.Values, techLevels);
			var shield = oldDesign.Shield != null ? statics.Shields[oldDesign.Shield.TypeInfo.IdCode].MakeBest(techLevels) : null;
			
			
			var thruster = AComponentType.MakeBest(statics.Thrusters.Values, techLevels);

			var design = new Design(
				this.Player, oldDesign.Name, oldDesign.Version + 1, oldDesign.ImageIndex, oldDesign.UsesFuel,
				armor, hull, isDrive, reactor, sensor, thruster, shield, equipment, specials
			);
			
			return design;
		}
		
		public void Analyze(Design design, StaticsDB statics)
		{
			this.DesignStats[design] = StatsOf(design, statics);
			this.calcRefitCosts(design, statics);
		}
		
		public static DesignStats StatsOf(Design design, StaticsDB statics)
		{
			var shipVars = DesignPoweredVars(design.Hull, design.Reactor, design.MissionEquipment, design.SpecialEquipment, statics);
			var hullVars = new Var(AComponentType.LevelKey, design.Hull.Level).Get;
			var armorVars = new Var(AComponentType.LevelKey, design.Armor.Level).Get;
			var thrusterVars = new Var(AComponentType.LevelKey, design.Thrusters.Level).Get;
			var sensorVars = new Var(AComponentType.LevelKey, design.Sensors.Level).Get;
			
			shipVars.And("armorFactor", design.Armor.TypeInfo.ArmorFactor.Evaluate(armorVars)).
				And("baseEvasion", design.Thrusters.TypeInfo.Evasion.Evaluate(thrusterVars)).
				And("thrust", design.Thrusters.TypeInfo.Speed.Evaluate(thrusterVars)).
				And("sensor", design.Sensors.TypeInfo.Detection.Evaluate(sensorVars));

			var driveSize = statics.ShipFormulas.IsDriveSize.Evaluate(shipVars.Get);
			double galaxySpeed = 0;
			if (design.IsDrive != null)
			{
				shipVars[AComponentType.LevelKey] = design.IsDrive.Level;
				shipVars[IsDriveType.SizeKey] = driveSize;
				galaxySpeed = design.IsDrive.TypeInfo.Speed.Evaluate(shipVars.Get);
			}
			
			var buildings = new Dictionary<string, double>();
			foreach(var colonyBuilding in statics.ShipFormulas.ColonizerBuildings)
			{
				double amount = colonyBuilding.Value.Evaluate(shipVars.Get);
				if (amount > 0)
					buildings.Add(colonyBuilding.Key, amount);
			}
			
			shipVars[AComponentType.LevelKey] = design.Armor.Level;
			double baseArmorReduction = design.Armor.TypeInfo.Absorption.Evaluate(shipVars.Get);
			shipVars[AComponentType.LevelKey] = design.Hull.Level;
			double hullArFactor = design.Hull.TypeInfo.ArmorAbsorption.Evaluate(shipVars.Get);
				
			double shieldCloaking = 0;
			double shieldJamming = 0;
			double shieldHp = 0;
			double shieldReduction = 0;
			double shieldRegeneration = 0;
			double shieldThickness = 0;
			double shieldPower = 0;
			var shieldSize = statics.ShipFormulas.ShieldSize.Evaluate(shipVars.Get);
			if (design.Shield != null)
			{
				shipVars[AComponentType.LevelKey] = design.Shield.Level;
				shipVars[ShieldType.SizeKey] = shieldSize;
				var hullShieldHp = design.Hull.TypeInfo.ShieldBase.Evaluate(hullVars);
				
				shieldCloaking = design.Shield.TypeInfo.Cloaking.Evaluate(shipVars.Get) * hullShieldHp;
				shieldJamming = design.Shield.TypeInfo.Jamming.Evaluate(shipVars.Get) * hullShieldHp;
				shieldHp = design.Shield.TypeInfo.HpFactor.Evaluate(shipVars.Get) * hullShieldHp;
				shieldReduction = design.Shield.TypeInfo.Reduction.Evaluate(shipVars.Get);
				shieldRegeneration = design.Shield.TypeInfo.RegenerationFactor.Evaluate(shipVars.Get) * hullShieldHp;
				shieldThickness = design.Shield.TypeInfo.Thickness.Evaluate(shipVars.Get);
				shieldPower = design.Shield.TypeInfo.PowerUsage.Evaluate(shipVars.Get);
			}
			shipVars.And("shieldCloak", shieldCloaking);
			shipVars.And("shieldJamming", shieldJamming);
			
			var abilities = new List<AbilityStats>(design.MissionEquipment.SelectMany(
				equip => equip.TypeInfo.Abilities.Select(
					x => AbilityStatsFactory.Create(x, equip.TypeInfo, equip.Level, equip.Quantity, statics)
				)
			));
			shipVars[HullType.SizeKey] = design.Hull.TypeInfo.SpaceFree.Evaluate(hullVars);

			var detection = statics.ShipFormulas.Detection.Evaluate(shipVars.Get);
			shipVars["detection"] = detection;

			return new DesignStats(
				calculateCost(design, shipVars.Get, statics.ShipFormulas),
				design.Hull.TypeInfo.Size,
				driveSize,
				statics.ShipFormulas.ReactorSize.Evaluate(shipVars.Get),
				shieldSize,
				galaxySpeed,
				shipVars[ReactorType.TotalPowerKey],
				statics.ShipFormulas.ScanRange.Evaluate(shipVars.Get),
				statics.ShipFormulas.CombatSpeed.Evaluate(shipVars.Get),
				shipVars[ReactorType.TotalPowerKey] - shieldPower,
				abilities,
				statics.ShipFormulas.CarryCapacity.Evaluate(shipVars.Get),
				statics.ShipFormulas.TowCapacity.Evaluate(shipVars.Get),
				statics.ShipFormulas.Survery.Evaluate(shipVars.Get),
				statics.ShipFormulas.ColonizerPopulation.Evaluate(shipVars.Get),
				buildings,
				statics.ShipFormulas.HitPoints.Evaluate(shipVars.Get),
				shieldHp,
				statics.ShipFormulas.Evasion.Evaluate(shipVars.Get),
				baseArmorReduction * hullArFactor,
				shieldReduction,
				shieldRegeneration,
				shieldThickness,
				detection,
				statics.ShipFormulas.Cloaking.Evaluate(shipVars.Get),
				statics.ShipFormulas.Jamming.Evaluate(shipVars.Get)
			);
		}

		private static double calculateCost(Design design, IDictionary<string, double> shipVars, ShipFormulaSet formulas)
		{
			var hullVars = new Var(AComponentType.LevelKey, design.Hull.Level).Get;
			double hullCost = design.Hull.TypeInfo.Cost.Evaluate(hullVars);
			double hullSize = design.Hull.TypeInfo.SpaceFree.Evaluate(hullVars);

			double isDriveCost = design.IsDrive == null ? 0 :
				design.IsDrive.TypeInfo.Cost.Evaluate(
					new Var(AComponentType.LevelKey, design.IsDrive.Level).
					And(IsDriveType.SizeKey, formulas.IsDriveSize.Evaluate(shipVars)).Get
				);

			double shieldCost = design.Shield == null ? 0 :
				design.Shield.TypeInfo.Cost.Evaluate(
					new Var(AComponentType.LevelKey, design.Shield.Level).
					And(ShieldType.SizeKey, formulas.ShieldSize.Evaluate(shipVars)).Get
				);

			double weaponsCost = design.MissionEquipment.Sum(
				x => x.Quantity * x.TypeInfo.Cost.Evaluate(
					new Var(AComponentType.LevelKey, x.Level).Get
				)
			);

			double specialsCost = design.SpecialEquipment.Sum(
				x => x.Quantity * x.TypeInfo.Cost.Evaluate(
					new Var(AComponentType.LevelKey, x.Level).
					And(HullType.SizeKey, hullSize).Get
				)
			);

			return hullCost + isDriveCost + shieldCost + weaponsCost + specialsCost;
		}

		private void calcRefitCosts(Design design, StaticsDB statics)
		{
			this.RefitCosts[design] = new Dictionary<Design, double>();
			
			var otherDesigns = this.DesignStats.Keys.Where(x => x.Hull.TypeInfo == design.Hull.TypeInfo && x != design).ToList();
			var designStats = this.DesignStats[design];
			foreach (var otherDesign in otherDesigns)
			{
				this.RefitCosts[design][otherDesign] = refitCost(design, otherDesign, this.DesignStats[otherDesign], statics);
				this.RefitCosts[otherDesign][design] = refitCost(otherDesign, design, designStats, statics);
			}
		}
		
		private double refitCost(Design fromDesign, Design toDesign, DesignStats toDesignStats, StaticsDB statics)
		{
			double cost = 0;
			var hullVars = new Var(AComponentType.LevelKey, toDesign.Hull.Level).Get;
			var levelRefitCost = statics.ShipFormulas.LevelRefitCost;

			var hullCost = refitComponentCost(fromDesign.Hull, toDesign.Hull, x => x.Cost, null, levelRefitCost);
			if (hullCost > 0)
				cost += hullCost;
			else
			{
				hullCost = toDesign.Hull.TypeInfo.Cost.Evaluate(new Var(AComponentType.LevelKey, toDesign.Hull.Level).Get);
				cost += hullCost * statics.ShipFormulas.ArmorCostPortion * refitComponentCost(fromDesign.Armor, toDesign.Armor, x => new Formula(1), null, levelRefitCost);
				cost += hullCost * statics.ShipFormulas.ReactorCostPortion * refitComponentCost(fromDesign.Reactor, toDesign.Reactor, x => new Formula(1), null, levelRefitCost);
				cost += hullCost * statics.ShipFormulas.SensorCostPortion * refitComponentCost(fromDesign.Sensors, toDesign.Sensors, x => new Formula(1), null, levelRefitCost);
				cost += hullCost * statics.ShipFormulas.ThrustersCostPortion * refitComponentCost(fromDesign.Thrusters, toDesign.Thrusters, x => new Formula(1), null, levelRefitCost);
			}

			var toDesignVars = DesignBaseVars(toDesign.Hull, toDesign.MissionEquipment, toDesign.SpecialEquipment, statics);
			cost += refitComponentCost(fromDesign.IsDrive, toDesign.IsDrive, x => x.Cost, new Var(IsDriveType.SizeKey, toDesignStats.IsDriveSize), levelRefitCost);
			cost += refitComponentCost(fromDesign.Shield, toDesign.Shield, x => x.Cost, new Var(ShieldType.SizeKey, toDesignStats.ShieldSize), levelRefitCost);
			cost += refitComponentCost(fromDesign.MissionEquipment, toDesign.MissionEquipment, x => x.Cost, null, levelRefitCost);
			cost += refitComponentCost(fromDesign.SpecialEquipment, toDesign.SpecialEquipment, x => x.Cost, new Var(HullType.SizeKey, toDesign.Hull.TypeInfo.SpaceFree.Evaluate(hullVars)), levelRefitCost);
			
			return cost;
		}
		
		private double refitComponentCost<T>(Component<T> fromComponent, Component<T> toComponent, Func<T, Formula> costFormula, Var extraVars, Formula levelRefitCost) where T : AComponentType
		{
			if (toComponent == null)
				return 0;

			if (extraVars == null)
				extraVars = new Var();
			var fullCost = costFormula(toComponent.TypeInfo).Evaluate(new Var(AComponentType.LevelKey, toComponent.Level).UnionWith(extraVars.Get).Get);
			
			if (fromComponent == null || fromComponent.TypeInfo != toComponent.TypeInfo)
				return fullCost;
			
			if (fromComponent.Level < toComponent.Level)
				return fullCost * levelRefitCost.Evaluate(new Var(AComponentType.LevelKey, toComponent.Level - fromComponent.Level).Get);

			//refit from higher to lower or the same level is free
			return 0;
		}

		private double refitComponentCost<T>(List<Component<T>> fromComponents, List<Component<T>> toComponents, Func<T, Formula> costFormula, Var extraVars, Formula levelRefitCost) where T : AComponentType
		{
			double cost = 0;

			var oldEquipment = fromComponents.
				GroupBy(x => x).
				ToDictionary(
					x => x.Key.TypeInfo,
					x => new Component<T>(x.Key.TypeInfo, x.Key.Level, x.Sum(y => y.Quantity))
				);
			foreach (var equip in toComponents)
			{
				var similarQuantity = 0;
				var oldEquip = oldEquipment.ContainsKey(equip.TypeInfo) ? oldEquipment[equip.TypeInfo] : null;

				if (oldEquip != null)
				{
					similarQuantity = Math.Min(oldEquip.Quantity, equip.Quantity);

					if (similarQuantity < oldEquip.Quantity)
						oldEquipment[equip.TypeInfo] = new Component<T>(equip.TypeInfo, oldEquip.Level, oldEquip.Quantity - similarQuantity);
					else
						oldEquipment.Remove(equip.TypeInfo);
				}

				cost += similarQuantity * refitComponentCost(oldEquip, equip, costFormula, extraVars, levelRefitCost);
				cost += (equip.Quantity - similarQuantity) * refitComponentCost(null, equip, costFormula, extraVars, levelRefitCost);
			}

			return cost;
		}
		#endregion
		
		private void makeDesign(StaticsDB statics, StatesDB states, DesignTemplate predefDesign, Dictionary<string, double> techLevels)
		{
			var hull = statics.Hulls[predefDesign.HullCode].MakeHull(techLevels);
			var specials = predefDesign.SpecialEquipment.OrderBy(x => x.Key).Select(
				x => statics.SpecialEquipment[x.Key].MakeBest(techLevels, x.Value)
			).ToList();
			var equipment = predefDesign.MissionEquipment.Select(
				x => statics.MissionEquipment[x.Key].MakeBest(techLevels, x.Value)
			).ToList();

			var armor = AComponentType.MakeBest(statics.Armors.Values, techLevels);
			var reactor = ReactorType.MakeBest(techLevels, hull, specials, equipment, statics);
			var isDrive = predefDesign.HasIsDrive ? IsDriveType.MakeBest(techLevels, hull, reactor, specials, equipment, statics) : null;
			var sensor = AComponentType.MakeBest(statics.Sensors.Values, techLevels);
			var shield = predefDesign.ShieldCode != null ? statics.Shields[predefDesign.ShieldCode].MakeBest(techLevels) : null;
			
			var thruster = AComponentType.MakeBest(statics.Thrusters.Values, techLevels);

			var design = new Design(
				this.Player, predefDesign.Name, 0, predefDesign.HullImageIndex, true,
			    armor, hull, isDrive, reactor, sensor, thruster, shield, equipment, specials
			);
			
			if (!states.Designs.Contains(design))
			{
				states.Designs.Add(design);
				this.Analyze(design, statics);
			}
		}
		
		private void unlockPredefinedDesigns(MainGame game)
		{
			var techLevels = game.States.DevelopmentAdvances.
				Of[this.Player].
				ToDictionary(x => x.Topic.IdCode, x => (double)x.Level);
				
			foreach(var predefDesign in game.Statics.PredeginedDesigns)
				if (!this.Player.UnlockedDesigns.Contains(predefDesign) && Prerequisite.AreSatisfied(predefDesign.Prerequisites(game.Statics), 0, techLevels))
				{
					this.Player.UnlockedDesigns.Add(predefDesign);
					makeDesign(game.Statics, game.States, predefDesign, techLevels);
				}
		}
		
		private static Dictionary<string, int> updateTechQueue(IEnumerable<KeyValuePair<string, int>> queue, ICollection<string> validItems)
		{
			var newOrder = queue
				.Where(x => validItems.Contains(x.Key))
				.OrderBy(x => x.Value)
				.Select(x => x.Key).ToArray();

			var newQueue = new Dictionary<string, int>();
			for (int i = 0; i < newOrder.Length; i++)
				newQueue.Add(newOrder[i], i);

			return newQueue;
		}

		private ConstructionOrders updateConstructionPlans(StaticsDB statics, ConstructionOrders oldOrders, AConstructionSiteProcessor processor)
		{
			var newOrders = new ConstructionOrders(oldOrders.SpendingRatio);
			var vars = processor.LocalEffects(statics).UnionWith(this.TechLevels).Get;

			foreach (var item in oldOrders.Queue)
				if (item.Condition.Evaluate(vars) >= 0)
					newOrders.Queue.Add(item);

			return newOrders;
		}
	}
}
