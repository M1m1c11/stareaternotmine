﻿using System.Collections.Generic;
using System.Linq;
using Stareater.Controllers.Data;
using Stareater.Galaxy;
using Stareater.GameData.Databases;
using Stareater.GameLogic;
using Stareater.Utils;

namespace Stareater.Controllers.Views.Ships
{
	public class FleetInfo
	{
		public PlayerInfo Owner { get; private set; }
		public MissionsInfo Missions { get; private set; }
		public Vector2D Position { get; private set; }
		
		internal Fleet FleetData { get; private set; }
		
		private readonly PlayerProcessor playerProc;
		private readonly StaticsDB statics;
		
		internal FleetInfo(Fleet fleet, PlayerProcessor playerProc, StaticsDB statics)
		{
			this.FleetData = fleet;
			this.playerProc = playerProc;
			this.statics = statics;

			this.Missions = MissionInfoFactory.Create(fleet);
			this.Owner = new PlayerInfo(fleet.Owner);
			this.Position = fleet.Position;
		}
		
		public bool IsMoving 
		{ 
			get { return this.Missions.Waypoints.Count > 0; }
		}
		
		public IEnumerable<ShipGroupInfo> Ships
		{
			get
			{
				return this.FleetData.Ships.Select(x => new ShipGroupInfo(x, this.playerProc.DesignStats[x.Design], this.statics));
			}
		}

		public double PopulationCapacity
		{
			get { return this.FleetData.Ships.Sum(x => this.playerProc.DesignStats[x.Design].ColonizerPopulation * x.Quantity); }
		}

		#region Equals and GetHashCode implementation
		public override bool Equals(object obj)
		{
			var other = obj as FleetInfo;
			return other != null && object.Equals(this.FleetData, other.FleetData);
		}
		
		public override int GetHashCode()
		{
			int hashCode = 0;
			unchecked {
				if (this.FleetData != null)
					hashCode += 1000000007 * this.FleetData.GetHashCode();
			}
			return hashCode;
		}
		
		public static bool operator ==(FleetInfo lhs, FleetInfo rhs)
		{
			if (ReferenceEquals(lhs, rhs))
				return true;
			if (lhs is null || rhs is null)
				return false;
			return lhs.Equals(rhs);
		}
		
		public static bool operator !=(FleetInfo lhs, FleetInfo rhs)
		{
			return !(lhs == rhs);
		}
		#endregion
	}
}
