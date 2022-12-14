using System;
using Stareater.AppData.Expressions;

namespace Stareater.GameData.Ships
{
	class StarShootAbility : AAbilityType
	{
		public Formula Range { get; private set; }
		public Formula EnergyCost { get; private set; }
		public Formula Ammo { get; private set; }

		public string AppliesTraitId { get; private set; }

		public StarShootAbility(string imagePath,
								Formula range, Formula energyCost, string appliesTraitId, Formula ammo)
			: base(imagePath)
		{
			this.Ammo = ammo;
			this.AppliesTraitId = appliesTraitId;
			this.EnergyCost = energyCost;
			this.Range = range;
		}

		public override void Accept(IAbilityVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}
