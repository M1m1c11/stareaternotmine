using System;
using System.Collections.Generic;
using System.Linq;
using Stareater.Controllers.Views;
using Stareater.Controllers.Views.Ships;
using Stareater.Galaxy;
using Stareater.GameData;
using Stareater.GameData.Construction;
using Stareater.GameData.Databases.Tables;
using Stareater.GameLogic.Planning;
using Stareater.Players;
using Stareater.Ships.Missions;
using Stareater.Utils;

namespace Stareater.Controllers
{
	public class PlayerController
	{
		public int PlayerIndex { get; private set; }
		private readonly GameController gameController;
		
		internal PlayerController(int playerIndex, GameController gameController)
		{
			this.PlayerIndex = playerIndex;
			this.gameController = gameController;
		}
		
		private MainGame gameInstance
		{
			get { return this.gameController.GameInstance; }
		}

		internal Player PlayerInstance(MainGame game)
		{
			if (this.PlayerIndex < game.MainPlayers.Length)
				return game.MainPlayers[this.PlayerIndex];
			else
				return game.StareaterOrganelles;
		}
		
		public PlayerInfo Info
		{
			get { return new PlayerInfo(this.PlayerInstance(this.gameInstance)); }
		}

		public StareaterController Stareater
		{
			get
			{
				var game = this.gameInstance;
				return new StareaterController(game, this.PlayerInstance(game));
			}
		}

		public LibraryController Library 
		{
			get { return new LibraryController(this.gameController); }
		}

		public int Turn
		{
			get { return this.gameInstance.Turn + 1; }
		}

		#region Turn progression
		public void EndGalaxyPhase()
		{
			this.gameController.EndGalaxyPhase(this);
		}

		public bool IsReadOnly
		{
			get { return this.gameInstance.IsReadOnly; }
		}
		#endregion
			
		#region Map related
		public bool IsStarVisited(StarInfo star)
		{
			if (star == null)
				throw new ArgumentNullException(nameof(star));

			return this.PlayerInstance(this.gameInstance).Intelligence.About(star.Data).IsVisited;
		}
		
		public IEnumerable<ColonyInfo> KnownColonies(StarInfo star)
		{
			if (star == null)
				throw new ArgumentNullException(nameof(star));

			var game = this.gameInstance;
			var starKnowledge = this.PlayerInstance(game).Intelligence.About(star.Data);
			
			foreach(var colony in game.States.Colonies.AtStar[star.Data])
				if (starKnowledge.Planets[colony.Location.Planet].Discovered)
					yield return new ColonyInfo(colony, game.Derivates[colony], starKnowledge.Planets[colony.Location.Planet]);
		}
		
		public StarSystemController OpenStarSystem(StarInfo star)
		{
			if (star == null)
				throw new ArgumentNullException(nameof(star));

			var game = this.gameInstance;

			if (!game.States.Stars.Contains(star.Data))
#pragma warning disable CA1303 // Do not pass literals as localized parameters
				throw new ArgumentException("Star doesn't exist");
#pragma warning restore CA1303 // Do not pass literals as localized parameters

			return new StarSystemController(game, star.Data, game.IsReadOnly, this);
		}
		
		public StarSystemController OpenStarSystem(Vector2D position)
		{
			return this.OpenStarSystem(new StarInfo(this.gameInstance.States.Stars.At[position]));
		}
		
		public IEnumerable<Circle> ScanAreas()
		{
			var game = this.gameInstance;

			return game.Derivates[this.PlayerInstance(game)].ScanRanges.GetAll();
		}

		public StarInfo Star(Vector2D position)
		{
			return new StarInfo(this.gameInstance.States.Stars.At[position]);
		}
		
		public int StarCount
		{
			get 
			{
				return this.gameInstance.States.Stars.Count;
			}
		}
		
		public IEnumerable<StarInfo> Stars
		{
			get
			{
				return this.gameInstance.States.Stars.Select(x => new StarInfo(x));
			}
		}

		public IEnumerable<WormholeInfo> Wormholes
		{
			get
			{
				var game = this.gameInstance;
				var intel = this.PlayerInstance(game).Intelligence;

				return game.States.Wormholes.
					Where(x => intel.StarlaneKnowledge[x]).
					Select(x => new WormholeInfo(x));
			}
		}

		public StarInfo StareaterSystem
		{
			get
			{
				var game = this.gameInstance;
				var star = game.States.StareaterBrain;

				return this.PlayerInstance(game).Intelligence.About(star).IsVisited ? new StarInfo(star) : null;
			}
		}
		#endregion

		#region Fleet related
		public FleetController SelectFleet(FleetInfo fleet)
		{
			if (fleet == null)
				throw new ArgumentNullException(nameof(fleet));

			var game = this.gameInstance;
			return new FleetController(fleet, game);
		}

		public IEnumerable<FleetInfo> FleetsAll
		{
			get
			{
				var game = this.gameInstance;
				var orders = game.Orders[this.PlayerInstance(game)];
				var player = this.PlayerInstance(game);
				var playerProc = game.Derivates[player];

				return game.States.Fleets.
					Where(x => playerProc.CanSee(x, game)).
					Concat(orders.ShipOrders.SelectMany(x => x.Value)).
					Select(
						x => new FleetInfo(x, game.Derivates[x.Owner])
					);
			}
		}

		public IEnumerable<FleetInfo> FleetsMine
		{
			get
			{
				var game = this.gameInstance;

				return game.Derivates[this.PlayerInstance(game)].
					MyFleets(game).
					Select(
						x => new FleetInfo(x, game.Derivates[x.Owner])
					);
			}
		}

		public IEnumerable<FleetInfo> FleetsAt(Vector2D position)
		{
			var game = this.gameInstance;
			var player = this.PlayerInstance(game);
			var orders = game.Orders[this.PlayerInstance(game)];
			var fleets = game.States.Fleets.At[position].Where(x => x.Owner != player || !game.Orders[x.Owner].ShipOrders.ContainsKey(x.Position));

			if (orders.ShipOrders.ContainsKey(position))
				fleets = fleets.Concat(orders.ShipOrders[position]);

			return fleets.Select(x => new FleetInfo(x, game.Derivates[x.Owner]));
		}

		public double FuelAvailable
		{
			get
			{
				var game = this.gameInstance;

				return game.Derivates.Colonies.
					OwnedBy[this.PlayerInstance(game)].
					Sum(x => x.FuelProduction);
			}
		}

		public double FuelUsage
		{
			get
			{
				var game = this.gameInstance;
				return Math.Max(game.Derivates[this.PlayerInstance(game)].TotalFuelUsage(game), 0);
			}
		}
		#endregion

		#region Stellarises and colonies
		public IEnumerable<StellarisInfo> Stellarises()
		{
			var game = this.gameInstance;
			foreach(var stellaris in game.States.Stellarises.OwnedBy[this.PlayerInstance(game)])
				yield return new StellarisInfo(stellaris, game);
		}
		#endregion
		
		#region Ship designs
		public ShipDesignController NewDesign()
		{
			var game = this.gameInstance;
			return (!game.IsReadOnly) ? new ShipDesignController(game, this.PlayerInstance(game)) : null;
		}
		
		public IEnumerable<DesignInfo> ShipsDesigns()
		{
			var game = this.gameInstance;
			return game.States.Designs.
				OwnedBy[this.PlayerInstance(game)].
				Select(x => new DesignInfo(x, game.Derivates[this.PlayerInstance(game)].DesignStats[x]));
		}
		
		public void DisbandDesign(DesignInfo design)
		{
			if (design == null)
				throw new ArgumentNullException(nameof(design));

			var game = this.gameInstance;
			if (game.IsReadOnly)
				return;

			game.Orders[this.PlayerInstance(game)].DiscardOrders.Add(design.Data);
		}

		public bool IsMarkedForRemoval(DesignInfo design)
		{
			if (design == null)
				throw new ArgumentNullException(nameof(design));

			var game = this.gameInstance;
			var orders = game.Orders[this.PlayerInstance(game)];

			return orders.DiscardOrders.Contains(design.Data);
		}
		
		public void KeepDesign(DesignInfo design)
		{
			if (design == null)
				throw new ArgumentNullException(nameof(design));

			var game = this.gameInstance;
			if (game.IsReadOnly)
				return;

			game.Orders[this.PlayerInstance(game)].RefitOrders.Remove(design.Data);
			game.Orders[this.PlayerInstance(game)].DiscardOrders.Remove(design.Data);
		}
		
		public IEnumerable<DesignInfo> RefitCandidates(DesignInfo design)
		{
			if (design == null)
				throw new ArgumentNullException(nameof(design));

			var game = this.gameInstance;
			var playerProc = game.Derivates[this.PlayerInstance(game)];
			
			return playerProc.RefitCosts[design.Data].
				Where(x => !x.Key.IsObsolete).
				Select(x => new DesignInfo(x.Key, playerProc.DesignStats[x.Key]));
		}
		
		public double RefitCost(DesignInfo design, DesignInfo refitWith)
		{
			if (design == null)
				throw new ArgumentNullException(nameof(design));
			if (refitWith == null)
				throw new ArgumentNullException(nameof(refitWith));

			var game = this.gameInstance;
			return game.Derivates[this.PlayerInstance(game)].RefitCosts[design.Data][refitWith.Data];
		}
		
		public void RefitDesign(DesignInfo design, DesignInfo refitWith)
		{
			if (design == null)
				throw new ArgumentNullException(nameof(design));
			if (refitWith == null)
				throw new ArgumentNullException(nameof(refitWith));

			var game = this.gameInstance;
			if (game.IsReadOnly)
				return;
			
			var player = this.PlayerInstance(game);
			var playerProc = game.Derivates[this.PlayerInstance(game)];
			var orders = game.Orders[player];

			if (!refitWith.Constructable ||
				orders.RefitOrders.ContainsKey(refitWith.Data) || 
			    !playerProc.RefitCosts[design.Data].ContainsKey(refitWith.Data))
				return;

			orders.RefitOrders[design.Data] = refitWith.Data;
		}
		
		public DesignInfo RefittingWith(DesignInfo design)
		{
			if (design == null)
				throw new ArgumentNullException(nameof(design));

			var game = this.gameInstance;
			var player = this.PlayerInstance(game);
			var orders = game.Orders[player];

			if (game.IsReadOnly || !orders.RefitOrders.ContainsKey(design.Data))
				return null;
			
			var targetDesign = orders.RefitOrders[design.Data];

			return new DesignInfo(targetDesign, game.Derivates[targetDesign.Owner].DesignStats[targetDesign]);
		}
		
		public long ShipCount(DesignInfo design)
		{
			var game = this.gameInstance;

			return game.States.Fleets.
				OwnedBy[this.PlayerInstance(game)].
				SelectMany(x => x.Ships).
				Where(x => x.Design == design.Data).
				Aggregate(0L, (sum, x) => sum + x.Quantity);
		}
		#endregion
		
		#region Colonization related
		public IEnumerable<ColonizationController> ColonizationProjects
		{
			get
			{
				var game = this.gameInstance;
				var player = this.PlayerInstance(game);
				var planets = new HashSet<Planet>();
				planets.UnionWith(game.States.ColonizationProjects.OwnedBy[player].Select(x => x.Destination));
				planets.UnionWith(game.Orders[player].ColonizationTargets);

				foreach (var planet in planets)
					yield return new ColonizationController(game, planet, game.IsReadOnly, this);
			}
		}

		public void AddColonizationSource(StellarisInfo source)
		{
			if (source == null)
				throw new ArgumentNullException(nameof(source));

			var game = this.gameInstance;
			if (game.IsReadOnly)
				return;

			game.Orders[this.PlayerInstance(game)].
				ColonizationSources.
				Add(source.Stellaris);
		}

		public void RemoveColonizationSource(StellarisInfo source)
		{
			if (source == null)
				throw new ArgumentNullException(nameof(source));

			var game = this.gameInstance;
			if (game.IsReadOnly)
				return;

			game.Orders[this.PlayerInstance(game)].
				ColonizationSources.
				Remove(source.Stellaris);
		}

		public IEnumerable<StellarisInfo> AvailableColonizationSources
		{
			get
			{
				var game = this.gameInstance;
				var player = this.PlayerInstance(game);
				var usedSources = game.Orders[player].ColonizationSources;
				return game.States.Stellarises.
					OwnedBy[player].
					Where(x => !usedSources.Contains(x)).
					Select(x => new StellarisInfo(x, game));
			}
		}

		public IEnumerable<StellarisInfo> ColonizationSources
		{
			get
			{
				var game = this.gameInstance;
				return game.Orders[this.PlayerInstance(game)].
					ColonizationSources.
					Select(x => new StellarisInfo(x, game));
			}
		}

		public IEnumerable<FleetInfo> EnrouteColonizers(PlanetInfo destination)
		{
			if (destination == null)
				throw new ArgumentNullException(nameof(destination));

			var game = this.gameInstance;
			var player = this.PlayerInstance(game);

			foreach (var fleet in game.States.Fleets.OwnedBy[player].Where(x => x.Ships.Any(s => s.PopulationTransport > 0)))
			{
				var lastMove = fleet.Missions.
					Where(x => x is MoveMission).
					Select(x => (x as MoveMission).Destination).
					LastOrDefault();

				if (fleet.Position == destination.HostStar.Position || lastMove != null && lastMove == destination.HostStar.Data)
					yield return new FleetInfo(fleet, game.Derivates[fleet.Owner]);
			}
		}

		public DesignInfo ColonizerDesign
		{
			get
			{
				var game = this.gameInstance;
				var player = this.PlayerInstance(game);
				var design = game.Orders[player].ColonizerDesign;

				return new DesignInfo(design, game.Derivates[player].DesignStats[design]);
			}
			set
			{
				if (value == null)
					throw new ArgumentNullException(nameof(value));

				var game = this.gameInstance;
				if (game.IsReadOnly)
					return;
				
				game.Orders[this.PlayerInstance(game)].ColonizerDesign = value.Data;
			}
		}

		public IEnumerable<DesignInfo> ColonizerDesignOptions
		{
			get
			{
				var game = this.gameInstance;
				var playerProc = game.Derivates[this.PlayerInstance(game)];

				return playerProc.ColonizerDesignOptions.
					Select(x => new DesignInfo(x, playerProc.DesignStats[x]));
			}
		}

		public long TargetTransportCapacity
		{
			get
			{
				var game = this.gameInstance;
				return game.Orders[this.PlayerInstance(game)].TargetTransportCapacity;
			}
			set
			{
				var game = this.gameInstance;

				if (game.IsReadOnly || value < 0)
					return;

				game.Orders[this.PlayerInstance(game)].TargetTransportCapacity = value;
			}
		}
		#endregion

		#region Development related
		public IEnumerable<DevelopmentTopicInfo> DevelopmentTopics()
		{
			var game = this.gameInstance;
			var player = this.PlayerInstance(game);
			var playerTechs = game.Derivates[player].DevelopmentOrder(game.States.DevelopmentAdvances, game.States.ResearchAdvances, game);

			if (game.Derivates[player].DevelopmentPlan == null)
				game.Derivates[player].CalculateDevelopment(
					game,
					game.Derivates.Colonies.OwnedBy[player]
				);

			var investments = game.Derivates[player].DevelopmentPlan.ToDictionary(x => x.Item);
			
			foreach(var techProgress in playerTechs)
				if (investments.ContainsKey(techProgress))
					yield return new DevelopmentTopicInfo(techProgress, investments[techProgress]);
				else
					yield return new DevelopmentTopicInfo(techProgress);
			
		}
		
		public IEnumerable<DevelopmentTopicInfo> ReorderDevelopmentTopics(IEnumerable<string> idCodeOrder)
		{
			if (idCodeOrder == null)
				throw new ArgumentNullException(nameof(idCodeOrder));

			var game = this.gameInstance;
			if (game.IsReadOnly)
				return DevelopmentTopics();

			var modelQueue = game.Orders[this.PlayerInstance(game)].DevelopmentQueue;
			modelQueue.Clear();
			
			int i = 0;
			foreach (var idCode in idCodeOrder) {
				modelQueue.Add(idCode, i);
				i++;
			}

			game.Derivates[this.PlayerInstance(game)].InvalidateDevelopment();
			return DevelopmentTopics();
		}
		
		public DevelopmentFocusInfo[] DevelopmentFocusOptions()
		{
			return this.gameInstance.Statics.DevelopmentFocusOptions.Select(x => new DevelopmentFocusInfo(x)).ToArray();
		}
		
		public int DevelopmentFocusIndex 
		{ 
			get
			{
				var game = this.gameInstance;
				return game.Orders[this.PlayerInstance(game)].DevelopmentFocusIndex;
			}
			
			set
			{
				var game = this.gameInstance;
				if (game.IsReadOnly)
					return;
				
				var player = this.PlayerInstance(game);

				if (value >= 0 && value < game.Statics.DevelopmentFocusOptions.Count)
					game.Orders[player].DevelopmentFocusIndex = value;

				game.Derivates[player].InvalidateDevelopment();
			}
		}
		
		public double DevelopmentPoints 
		{ 
			get
			{
				var game = this.gameInstance;
				
				return game.Derivates.Colonies.OwnedBy[this.PlayerInstance(game)].Sum(x => x.Development);
			}
		}
		#endregion
		
		#region Research related
		public ResearchTopicInfo[] ResearchTopics()
		{
			var game = this.gameInstance;
			var player = this.PlayerInstance(game);
			
			if (game.Derivates[player].ResearchPlan == null)
				game.Derivates[player].CalculateResearch(game);

			var investments = game.Derivates[player].ResearchPlan.ToDictionary(x => x.Item.Topic);
			var finishedFields = game.States.ResearchAdvances.
				Of[player].
				Where(x => !investments.ContainsKey(x.Topic)).
				ToDictionary(x => x.Topic);
   
			var infos = new ResearchTopicInfo[game.Statics.ResearchTopics.Count];
			for (int i = 0; i < infos.Length; i++)
			{
				var field = game.Statics.ResearchTopics[i];
				infos[i] = investments.ContainsKey(field) ?
					new ResearchTopicInfo(investments[field].Item, investments[field]) :
					new ResearchTopicInfo(finishedFields[field]);
			}

			return infos;
		}
		
		public ResearchTopicInfo ResearchFocus
		{
			get 
			{
				var game = this.gameInstance;
				string focused = game.Orders[this.PlayerInstance(game)].ResearchFocus;
				var fieldProgress = game.States.ResearchAdvances.
					Of[this.PlayerInstance(game), focused];

				return fieldProgress.CanProgress ? new ResearchTopicInfo(fieldProgress) : null;
			}

			set
			{
				if (value == null)
					throw new ArgumentNullException(nameof(value));

				var game = this.gameInstance;
				if (game.IsReadOnly)
					return;

				game.Orders[this.PlayerInstance(game)].ResearchFocus = value.Topic.IdCode;
				game.Derivates[this.PlayerInstance(game)].InvalidateResearch();
			}
		}

		public IEnumerable<DevelopmentTopicInfo> ResearchUnlockPriorities(ResearchTopicInfo field)
		{
			if (field == null)
				throw new ArgumentNullException(nameof(field));

			var game = this.gameInstance;
			var player = this.PlayerInstance(game);
			var orderedPriorities = game.Orders[player].ResearchPriorities;

			var unlockIds = orderedPriorities.ContainsKey(field.IdCode) ?
				orderedPriorities[field.IdCode] :
				field.Topic.Unlocks[field.NextLevel];

			var developmentTopics = game.Statics.DevelopmentTopics;

			return unlockIds.Select(id => new DevelopmentTopicInfo(new DevelopmentProgress(
				game.Statics.DevelopmentTopics.First(x => x.IdCode == id), 
				player
			))).ToArray();
		}

		public void ResearchReorderPriority(ResearchTopicInfo field, DevelopmentTopicInfo unlock, int index)
		{
			if (field == null)
				throw new ArgumentNullException(nameof(field));
			if (unlock == null)
				throw new ArgumentNullException(nameof(unlock));

			if (index < 0 || index >= field.Topic.Unlocks[field.NextLevel].Length)
				return;

			var game = this.gameInstance;
			var orderedPriorities = game.Orders[this.PlayerInstance(game)].ResearchPriorities;

			if (!orderedPriorities.ContainsKey(field.IdCode))
				orderedPriorities[field.IdCode] = field.Topic.Unlocks[field.NextLevel];

			var fieldPriorities = orderedPriorities[field.IdCode].Where(x => x != unlock.IdCode).ToList();

			fieldPriorities.Insert(index, unlock.IdCode);
			orderedPriorities[field.IdCode] = fieldPriorities.ToArray();
		}
		#endregion

		#region Report related
		public IEnumerable<IReportInfo> Reports
		{
			get 
			{
				var game = this.gameInstance;
				var wrapper = new ReportWrapper();

				foreach (var report in game.States.Reports.Of[this.PlayerInstance(game)])
					yield return wrapper.Wrap(report);
			}
		}
		#endregion

		#region Diplomacy related
		public IEnumerable<ContactInfo> DiplomaticContacts()
		{
			var game = this.gameInstance;
			var player = this.PlayerInstance(game);

			foreach (var otherPlayer in game.MainPlayers)
				if (otherPlayer != player && game.States.Contacts.Contains(new Pair<Player>(otherPlayer, player)))
					yield return new ContactInfo(otherPlayer, game.States.Treaties.Of[player, otherPlayer]);
		}

		public bool IsAudienceRequested(ContactInfo contact)
		{
			if (contact == null)
				throw new ArgumentNullException(nameof(contact));

			var game = this.gameInstance;
			var contactIndex = Array.IndexOf(game.MainPlayers, contact.PlayerData);
			
			return game.Orders[this.PlayerInstance(game)].AudienceRequests.Contains(contactIndex);
		}
		
		public void RequestAudience(ContactInfo contact)
		{
			if (contact == null)
				throw new ArgumentNullException(nameof(contact));

			var game = this.gameInstance;
			if (game.IsReadOnly)
				return;
			
			var contactIndex = Array.IndexOf(game.MainPlayers, contact.PlayerData);
			game.Orders[this.PlayerInstance(game)].AudienceRequests.Add(contactIndex);
		}
		
		public void CancelAudience(ContactInfo contact)
		{
			if (contact == null)
				throw new ArgumentNullException(nameof(contact));

			var game = this.gameInstance;
			if (game.IsReadOnly)
				return;
			
			var contactIndex = Array.IndexOf(game.MainPlayers, contact.PlayerData);
			game.Orders[this.PlayerInstance(game)].AudienceRequests.Remove(contactIndex);
		}
		#endregion

		#region Automation
		public void RunAutomation()
		{
			var game = this.gameInstance;
			var player = this.PlayerInstance(game);
			var orders = game.Orders[player];
			var colonizationThreshold = game.Statics.ColonyFormulas.ColonizationPopulationThreshold.Evaluate(null);
			foreach (var stellaris in game.States.Stellarises.OwnedBy[player])
				orders.AutomatedConstruction[stellaris] = new ConstructionOrders(0);

			var transportFleets = this.FleetsMine.Where(x => x.PopulationCapacity > 0).Select(x => new FleetController(x, game)).ToList();
			for(int i = 0; i < transportFleets.Count; i++)
			{
				foreach (var group in transportFleets[i].ShipGroups)
					transportFleets[i].SelectGroup(group, group.PopulationCapacity > 0  ? group.Quantity : 0);

				transportFleets[i] = transportFleets[i].SplitSelection();
			}
			var colonistDemand = orders.ColonizationTargets.
				GroupBy(x => x.Star).
				ToDictionary(x => x.Key, x => colonizationThreshold * x.Count());

			// Enroute colonists
			foreach (var fleet in transportFleets.Select(x => x.Fleet.FleetData))
			{
				var destination = new TransportDestinationVisitior().Trace(fleet);

				if (!destination.HasValue || !game.States.Stars.At.Contains(destination.Value))
					continue;

				var star = game.States.Stars.At[destination.Value];
				if (colonistDemand.ContainsKey(star))
					colonistDemand[star] -= fleet.Ships.Sum(x => x.PopulationTransport);
			}

			var missingCapacity = this.TargetTransportCapacity +
				orders.ColonizationTargets.Sum(x => colonizationThreshold) -
				transportFleets.Sum(x => x.Fleet.PopulationCapacity);

			// Build missing transports
			var colonizerDesign = game.Orders[player].ColonizerDesign;
			var colonizerCost = game.Derivates[player].DesignStats[colonizerDesign].Cost;
			foreach (var source in this.ColonizationSources.Select(x => x.Stellaris))
			{
				if (missingCapacity <= 0)
					break;

				var plan = orders.AutomatedConstruction[source];
				plan.SpendingRatio = 1;
				plan.Queue.Add(new ShipProject(colonizerDesign, colonizerCost, true));
				//TODO(v0.8) calculate number of ships per turn
			}

			var idleTransports = new Queue<FleetController>(
				transportFleets.
				Where(x => !x.Fleet.FleetData.Missions.Any() || x.Fleet.FleetData.Missions.First() is LoadMission)
			);

			//TODO(v0.8) doesn't count supply properly when there are ships enroute and a few stationary. Makes stationary fly away.
			// Send and filter colonizers
			var nonColonizers = new List<FleetController>();
			foreach (var destination in colonistDemand.Where(x => x.Value > 0).Select(x => x.Key).ToList())
				while (colonistDemand[destination] > 0 && idleTransports.Any())
				{
					var fleet = idleTransports.Dequeue();

					if (fleet.Fleet.Position == destination.Position)
					{
						foreach (var group in fleet.ShipGroups)
						{
							var toStay = Math.Min(group.FullTransporters, Math.Ceiling(colonistDemand[destination] / group.Design.ColonizerPopulation));
							toStay = Math.Max(toStay, 0);

							colonistDemand[destination] -= toStay * group.Design.ColonizerPopulation;
							fleet.SelectGroup(group, group.Quantity - (long)toStay);
						}

						fleet = fleet.SplitSelection();
					}
					else
					{
						foreach (var group in fleet.ShipGroups)
						{
							var toSelect = Math.Min(group.FullTransporters, Math.Ceiling(colonistDemand[destination] / group.Design.ColonizerPopulation));
							toSelect = Math.Max(toSelect, 0);

							fleet.SelectGroup(group, (long)toSelect, toSelect * group.Design.ColonizerPopulation);
							colonistDemand[destination] -= toSelect * group.Design.ColonizerPopulation;
						}

						fleet.Send(new StarInfo(destination));
					}

					if (fleet.ShipGroups.Any())
						nonColonizers.Add(fleet);
				}

			foreach (var fleet in nonColonizers)
				idleTransports.Enqueue(fleet);
			
			var emmigrateFrom = game.States.Stellarises.OwnedBy[player].FirstOrDefault(x => game.Derivates.Stellarises.At[x.Location.Star].IsMigrants > 0);

			while (idleTransports.Any() && emmigrateFrom != null)
			{
				var fleet = idleTransports.Dequeue();

				// Load population at emigration point
				if (fleet.Fleet.Position == emmigrateFrom.Location.Star.Position)
				{
					foreach (var group in fleet.ShipGroups)
						fleet.SelectGroup(group, Math.Max(group.Quantity - group.FullTransporters, 0), 0);

					fleet.LoadPopulation();
				}

				// Send empty (non-full) transporters to emigration point
				if (fleet.Fleet.Position != emmigrateFrom.Location.Star.Position)
				{
					foreach (var group in fleet.ShipGroups)
						fleet.SelectGroup(group, Math.Max(group.Quantity - group.FullTransporters, 0), 0);

					fleet.Send(new StarInfo(emmigrateFrom.Location.Star));
				}
			}
		}
		#endregion
	}
}
