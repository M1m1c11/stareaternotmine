﻿using System;
using Stareater.GameData.Ships;

namespace Stareater.GameLogic
{
	class AbilityStats
	{
		public AAbilityType Type { get; private set; }
		public int Level { get; private set; }
		public int Quantity { get; private set; }
		
		public int Range { get; private set; }
		public bool IsInstantDamage { get; private set; }
		
		public double FirePower { get; private set; }
		public double Accuracy { get; private set; }
		public double EnergyCost { get; private set; }
		public double ArmorEfficiency { get; private set; }
		public double ShieldEfficiency { get; private set; }
		
		public AbilityStats(AAbilityType type, int level, int quantity, 
		                    int range, bool isInstantDamage, 
		                    double firePower, double accuracy, double energyCost, double armorEfficiency, double shieldEfficiency)
		{
			this.Type = type;
			this.Level = level;
			this.Quantity = quantity;
			
			this.Range = range;
			this.IsInstantDamage = isInstantDamage;
			
			this.FirePower = firePower;
			this.Accuracy = accuracy;
			this.EnergyCost = energyCost;
			this.ArmorEfficiency = armorEfficiency;
			this.ShieldEfficiency = shieldEfficiency;
		}
	}
}
