using Stareater.AppData.Expressions;
using Stareater.Galaxy;
using Stareater.Utils.Collections;

namespace Stareater.GameData.Construction
{
	class ConstructionAddBuilding : IConstructionEffect
	{
		private readonly string buildingCode;
		private readonly Formula quantity;
		
		public ConstructionAddBuilding(string buildingCode, Formula quantity)
		{
			this.buildingCode = buildingCode;
			this.quantity = quantity;
		}

		public void Apply(MainGame game, AConstructionSite site, long quantity)
		{
			//TODO(v0.8) report new building construction
			var vars = new Var("quantity", quantity);
			quantity = (long)this.quantity.Evaluate(vars.Get);
			
			if (!site.Buildings.ContainsKey(buildingCode))
				site.Buildings.Add(buildingCode, quantity);
			else
				site.Buildings[buildingCode] += quantity;
		}
	}
}
