﻿using System;
using System.Collections.Generic;
using System.Linq;

using Stareater.Controllers.Views.Ships;
using Stareater.GameData.Databases.Tables;
using Stareater.GameData.Ships;
using Stareater.GameLogic;
using Stareater.GameLogic.Combat;
using Stareater.Players;
using Stareater.Ships;
using Stareater.Utils.Collections;

namespace Stareater.Controllers
{
	public class ShipDesignController
	{
		private readonly MainGame game;
		private readonly Player player;
		private readonly Dictionary<string, double> playersTechLevels;
		
		internal ShipDesignController(MainGame game, Player player)
		{
			this.game = game;
			this.player = player;
			
			this.playersTechLevels = game.States.DevelopmentAdvances.Of[this.player]
				.ToDictionary(x => x.Topic.IdCode, x => (double)x.Level);
			
			this.armorInfo = bestArmor();
			this.sensorInfo = bestSensor();
			this.thrusterInfo = bestThruster();
			
			var hullType = game.Statics.Hulls.Values.First(x => x.CanPick);
			this.selectedHull = new HullInfo(hullType, hullType.HighestLevel(playersTechLevels));
			this.reactorInfo = bestReactor();
			this.makeDesign();
		}

		private ArmorInfo bestArmor()
		{
			var armor = AComponentType.MakeBest(
				game.Statics.Armors.Values,
				playersTechLevels
			);
			
			return armor != null ? new ArmorInfo(armor.TypeInfo, armor.Level) : null;
		}
		
		private IsDriveInfo bestIsDrive()
		{
			var hull = new Component<HullType>(this.selectedHull.Type, this.selectedHull.Level);
			var reactor = new Component<ReactorType>(this.reactorInfo.Type, this.reactorInfo.Level);
			
			var drive = IsDriveType.MakeBest(
				playersTechLevels, 
				hull,
				reactor,
				this.selectedSpecialEquipment,
				this.selectedMissionEquipment,
				game.Statics
			);

			return drive != null ?
				new IsDriveInfo(drive.TypeInfo, drive.Level, this.stats) :
				null;
		}

		private ReactorInfo bestReactor()
		{
			var hull = new Component<HullType>(this.selectedHull.Type, this.selectedHull.Level);
			var reactor = ReactorType.MakeBest(
				playersTechLevels,
				hull,
				this.selectedSpecialEquipment,
				this.selectedMissionEquipment,
				game.Statics
			);

			return reactor != null ? 
				new ReactorInfo(reactor.TypeInfo, reactor.Level, this.stats) : 
				null;
		}

		private SensorInfo bestSensor()
		{
			var sensor = AComponentType.MakeBest(
				game.Statics.Sensors.Values,
				playersTechLevels
			);

			return sensor != null ? new SensorInfo(sensor.TypeInfo, sensor.Level) : null;
		}
		
		private ThrusterInfo bestThruster()
		{
			var thruster = AComponentType.MakeBest(
				game.Statics.Thrusters.Values,
				playersTechLevels
			);

			return thruster != null ? new ThrusterInfo(thruster.TypeInfo, thruster.Level) : null;
		}
		
		private Var shipBaseVars()
		{
			return PlayerProcessor.DesignBaseVars(
				new Component<HullType>(this.selectedHull.Type, this.selectedHull.Level),
				this.selectedMissionEquipment,
				this.selectedSpecialEquipment, 
				this.game.Statics);
		}
		
		private Var shipPoweredVars()
		{
			return PlayerProcessor.DesignPoweredVars(
				new Component<HullType>(this.selectedHull.Type, this.selectedHull.Level), 
				new Component<ReactorType>(this.reactorInfo.Type, this.reactorInfo.Level),
				this.selectedMissionEquipment,
				this.selectedSpecialEquipment, 
				this.game.Statics);
		}

		private void makeDesign()
		{
			this.design = new Design(
				this.game.States.MakeDesignId(),
				this.player,
				false,
				this.Name?.Trim() ?? "",
				this.ImageIndex,
				true,
				new Component<ArmorType>(this.armorInfo.Type, this.armorInfo.Level),
				new Component<HullType>(this.selectedHull.Type, this.selectedHull.Level),
				this.HasIsDrive ? new Component<IsDriveType>(this.availableIsDrive.Type, this.availableIsDrive.Level) : null,
				new Component<ReactorType>(this.reactorInfo.Type, this.reactorInfo.Level),
				new Component<SensorType>(this.sensorInfo.Type, this.sensorInfo.Level),
				new Component<ThrusterType>(this.thrusterInfo.Type, this.thrusterInfo.Level),
				this.Shield != null ? new Component<ShieldType>(this.Shield.Type, this.Shield.Level) : null,
				selectedMissionEquipment,
				selectedSpecialEquipment,
				this.game.Statics
			);

			this.stats = PlayerProcessor.StatsOf(this.design, this.game.Statics);
		}

		#region Component lists
		public IEnumerable<HullInfo> Hulls()
		{
			return game.Statics.Hulls.Values.Where(x => x.CanPick).Select(x => new HullInfo(x, x.HighestLevel(playersTechLevels)));
		}
		
		public ArmorInfo Armor
		{
			get { return this.armorInfo; }
		}
		
		public IsDriveInfo AvailableIsDrive
		{
			get { return this.availableIsDrive; }
		}

		public ReactorInfo Reactor
		{
			get { return this.reactorInfo; }
		}

		public SensorInfo Sensor
		{
			get { return this.sensorInfo; }
		}
		
		public IEnumerable<ShieldInfo> Shields()
		{
			return this.game.Statics.Shields.Values.
				Where(x => x.IsAvailable(playersTechLevels) && x.CanPick).
				Select(x => new ShieldInfo(x, x.HighestLevel(playersTechLevels), this.stats.ShieldSize));
		}

		public IEnumerable<MissionEquipInfo> MissionEquipment()
		{
			return this.game.Statics.MissionEquipment.Values.
				Where(x => x.IsAvailable(playersTechLevels) && x.CanPick).
				Select(x => new MissionEquipInfo(x, x.HighestLevel(playersTechLevels)));
		}
		
		public IEnumerable<SpecialEquipInfo> SpecialEquipment()
		{
			return this.game.Statics.SpecialEquipment.Values.
				Where(x => x.IsAvailable(playersTechLevels) && x.CanPick).
				Select(x => new SpecialEquipInfo(x, x.HighestLevel(playersTechLevels), this.selectedHull));
		}
		
		public ThrusterInfo Thrusters
		{
			get { return this.thrusterInfo; }
		}
		#endregion
		
		#region Selected components
		private HullInfo selectedHull = null;

		//TODO(v0.9) use types/components instead of infos
		private readonly ArmorInfo armorInfo = null;
		private IsDriveInfo availableIsDrive = null;
		private ReactorInfo reactorInfo = null;
		private readonly SensorInfo sensorInfo = null;
		private readonly ThrusterInfo thrusterInfo = null;

		private bool hasIsDrive;
		private ShieldInfo shield;
		private readonly List<Component<MissionEquipmentType>> selectedMissionEquipment = new List<Component<MissionEquipmentType>>();
		private readonly List<Component<SpecialEquipmentType>> selectedSpecialEquipment = new List<Component<SpecialEquipmentType>>();

		private Design design;
		private DesignStats stats;

		private void onHullChange()
		{
			if (this.ImageIndex < 0 || this.ImageIndex >= this.selectedHull.ImagePaths.Count)
				this.ImageIndex = 0;

			this.reactorInfo = bestReactor();
			this.availableIsDrive = bestIsDrive();
			this.HasIsDrive &= availableIsDrive != null;
			this.makeDesign();
		}

		#endregion

		#region Combat info
		public double Cloaking
		{
			get 
			{
				return game.Statics.ShipFormulas.Cloaking.Evaluate(
					this.shipPoweredVars().
					And("shieldCloak", Shield != null ? Shield.Cloaking : 0).Get
				);
			}
		}
		
		public double CombatSpeed
		{
			get 
			{
				return game.Statics.ShipFormulas.CombatSpeed.Evaluate(
					this.shipBaseVars().
					And("thrust", this.thrusterInfo.Evasion).Get
				);
			}
		}
		
		public double Detection
		{
			get 
			{
				return game.Statics.ShipFormulas.Detection.Evaluate(
					this.shipBaseVars().
					And("sensor", this.thrusterInfo.Evasion).Get
				);
			}
		}
		
		public double Evasion
		{
			get 
			{
				return game.Statics.ShipFormulas.Evasion.Evaluate(
					this.shipBaseVars().
					And("baseEvasion", this.thrusterInfo.Evasion).Get
				);
			}
		}
		
		public double HitPoints
		{
			get 
			{
				return game.Statics.ShipFormulas.HitPoints.Evaluate(
					this.shipBaseVars().
					And("armorFactor", this.armorInfo.ArmorFactor).Get
				);
			}
		}
		
		public double Jamming
		{
			get 
			{
				return game.Statics.ShipFormulas.Jamming.Evaluate(
					this.shipPoweredVars().
					And("shieldJamming", Shield != null ? Shield.Jamming : 0).Get
				);
			}
		}
		#endregion

		#region Design info
		public double Cost => this.stats.Cost;
		
		public double PowerUsed
		{
			get { return (this.Shield != null) ? this.Shield.PowerUsage : 0; }
		}
		
		public double SpaceTotal
		{
			get { return this.selectedHull.Space; }
		}
		
		public double SpaceUsed
		{
			get 
			{
				var shipVars = PlayerProcessor.DesignBaseVars(
					new Component<HullType>(this.selectedHull.Type, this.selectedHull.Level), 
					this.selectedMissionEquipment, this.selectedSpecialEquipment, game.Statics);
				var specEquipVars = new Var(HullType.SizeKey, this.selectedHull.Size);

				return (this.HasIsDrive ? this.stats.IsDriveSize : 0) + 
					(this.Shield != null ? this.stats.ShieldSize : 0) +
					this.selectedMissionEquipment.Sum(x => x.TypeInfo.Size.Evaluate(new Var(AComponentType.LevelKey, x.Level).Get) * x.Quantity) + 
					this.selectedSpecialEquipment.Sum(x => x.TypeInfo.Size.Evaluate(specEquipVars.Set(AComponentType.LevelKey, x.Level).Get) * x.Quantity);
			} 
		}
		#endregion
		
		#region Designer actions
		public string Name { get; set; } 
		public int ImageIndex { get; set; }

		public void AddMissionEquip(MissionEquipInfo equipInfo)
		{
			if (equipInfo == null)
				throw new ArgumentNullException(nameof(equipInfo));

			this.selectedMissionEquipment.Add(new Component<MissionEquipmentType>(equipInfo.Type, equipInfo.Level, 1));
			this.makeDesign();
		}

		public HullInfo Hull 
		{ 
			get { return this.selectedHull; }
			set
			{
				this.selectedHull = value;
				this.onHullChange();
			}
		}

		public bool HasIsDrive 
		{
			get => this.hasIsDrive;
			set
			{
				this.hasIsDrive = value;
				this.makeDesign();
			}
		}

		public bool HasSpecialEquip(SpecialEquipInfo equipInfo)
		{
			return this.selectedSpecialEquipment.Any(x => x.Quantity > 0 && x.TypeInfo == equipInfo.Type);
		}
		
		public int MissionEquipCount(int index)
		{
			return this.selectedMissionEquipment[index].Quantity;
		}

		public void MissionEquipSetAmount(int index, int amount)
		{
			if (index < 0 || index >= this.selectedMissionEquipment.Count || amount < 0)
				return;
			
			if (amount == 0)
				this.selectedMissionEquipment.RemoveAt(index);
			else
			{
				var oldEquip = this.selectedMissionEquipment[index];
				this.selectedMissionEquipment[index] = new Component<MissionEquipmentType>(oldEquip.TypeInfo, oldEquip.Level, amount);
			}
			this.makeDesign();
		}
		
		public ShieldInfo Shield 
		{
			get => this.shield;
			set
			{
				this.shield = value;
				this.makeDesign();
			}
		}

		public int SpecialEquipCount(SpecialEquipInfo equipInfo)
		{
			return this.selectedSpecialEquipment.
				Where(x => x.TypeInfo == equipInfo.Type).
				Aggregate(0, (sum, x) => x.Quantity);
		}
				
		public void SpecialEquipSetAmount(SpecialEquipInfo equipInfo, int amount)
		{
			if (equipInfo == null)
				throw new ArgumentNullException(nameof(equipInfo));

			int i = this.selectedSpecialEquipment.FindIndex(x => x.TypeInfo == equipInfo.Type);

			if (i < 0)
			{
				i = this.selectedSpecialEquipment.Count;
				this.selectedSpecialEquipment.Add(new Component<SpecialEquipmentType>(equipInfo.Type, equipInfo.Level, 0));
			}

			if (amount <= 0)
				this.selectedSpecialEquipment.RemoveAt(i);
			else if (amount <= equipInfo.MaxCount)
				this.selectedSpecialEquipment[i] = new Component<SpecialEquipmentType>(equipInfo.Type, equipInfo.Level, amount);

			this.onHullChange();
		}

		//TODO(later) consider returning a reason for invalidity 
		public bool IsDesignValid
		{
			get
			{

				return this.selectedHull != null && this.ImageIndex >= 0 && this.ImageIndex < this.selectedHull.ImagePaths.Count &&
					!string.IsNullOrWhiteSpace(this.Name) && game.States.Designs.All(x => x.Name != this.Name.Trim()) &&
					(this.availableIsDrive != null || !this.HasIsDrive) &&
					this.SpaceUsed <= this.SpaceTotal;
			}
		}
		
		public void Commit()
		{
			if (!IsDesignValid)
				return;

			this.makeDesign();
			
			if (this.game.States.Designs.Contains(this.design))
				return; //TODO(v0.8) move the check to IsDesignValid
			
			game.States.Designs.Add(design); //TODO(v0.8) add to changes DB and propagate to states during turn processing
			game.Derivates.Players.Of[this.player].Analyze(design, this.game.Statics);
		}
		#endregion
	}
}
