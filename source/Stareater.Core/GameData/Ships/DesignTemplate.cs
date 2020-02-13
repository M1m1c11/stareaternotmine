﻿using System.Collections.Generic;
using Stareater.GameData.Databases;
using Stareater.Utils.StateEngine;

namespace Stareater.GameData.Ships
{
	[StateTypeAttribute(true)]
	class DesignTemplate : IIdentifiable
	{
		public string Name { get; private set; }
		public string IdCode { get; private set; }

		public string HullCode { get; private set; }
		public int HullImageIndex { get; private set; }
		
		public bool HasIsDrive { get; private set; }
		public string ShieldCode { get; private set; }
		public List<KeyValuePair<string, int>> MissionEquipment { get; private set; }
		public Dictionary<string, int> SpecialEquipment { get; private set; }

		public DesignTemplate(string name, string idCode, string hullCode, int hullImageIndex, bool hasIsDrive, string shieldCode, 
		                        List<KeyValuePair<string, int>> missionEquipment, Dictionary<string, int> specialEquipment)
		{
			this.Name = name;
			this.IdCode = idCode;
			
			this.HasIsDrive = hasIsDrive;
			this.HullCode = hullCode;
			this.HullImageIndex = hullImageIndex;
			this.ShieldCode = shieldCode;
			this.MissionEquipment = missionEquipment;
			this.SpecialEquipment = specialEquipment;
		}
		
		public IEnumerable<Prerequisite> Prerequisites(StaticsDB statics)
		{
			return statics.Hulls[HullCode].Prerequisites;
		}
	}
}
