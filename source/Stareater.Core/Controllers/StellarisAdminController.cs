﻿using System;
using System.Collections.Generic;
using System.Linq;
using Stareater.AppData.Expressions;
using Stareater.Controllers.Data;
using Stareater.Galaxy;
using Stareater.GameData;
using Stareater.GameLogic;

namespace Stareater.Controllers
{
	public class StellarisAdminController : AConstructionSiteController
	{
		internal StellarisAdminController(Game game, StellarisAdmin stellaris, bool readOnly): base(stellaris, readOnly, game)
		{ }

		internal override AConstructionSiteProcessor Processor
		{
			get { return Game.Derivates.Of((StellarisAdmin)Site); }
		}
		
		#region Buildings
		protected override void RecalculateSpending()
		{
			Game.Derivates.Stellarises.At(Location).CalculateSpending(
				Game.Derivates.Of(Site.Owner),
				Game.Derivates.Colonies.At(Location)
			);
		}
		
		public override IEnumerable<ConstructableItem> ConstructableItems 
		{
			get 
			{ 
				foreach(var item in base.ConstructableItems)
					yield return item;
				
				//TODO(v0.5): put ID code
				foreach(var design in Game.States.Designs.OwnedBy(Game.CurrentPlayer))
					yield return new ConstructableItem(
						new Constructable(design.Name, "", true, design.ImagePath, 
						                  "", new Prerequisite[0], SiteType.StarSystem,
						                  new Formula(true), new Formula(design.Cost), new Formula(double.PositiveInfinity),
						                  new IConstructionEffect[] { new ConstructionAddShip() }),
						Game.Derivates.Players.Of(Game.CurrentPlayer)
					);
			}
		}
		#endregion
		
		protected StarData Location 
		{
			get 
			{ 
				return (Site as StellarisAdmin).Location;
			}
		}
		
		#region Colonies
		public double OrganisationAverage 
		{
			get 
			{ 
				var workplaces = Game.Derivates.Colonies.
					At(Location).
					Where(x => x.Owner == Site.Owner).
					Sum(x => x.Organization * x.Colony.Population);
				
				return workplaces / PopulationTotal; //FIXME(later): possible div by 0
			}
		}
		public double PopulationTotal
		{
			get 
			{ 
				return Game.States.Colonies.
					AtStar(Location).
					Where(x => x.Owner == Site.Owner).
					Sum(x => x.Population);
			}
		}
		#endregion
		
		#region Output
		public double IndustryTotal 
		{
			get 
			{ 
				return Game.Derivates.Stellarises.
					Of(Site as StellarisAdmin).
					SpendingPlan.Sum(x => x.InvestedPoints);
			}
		}
		
		public double DevelopmentTotal 
		{
			get 
			{ 
				return Game.Derivates.Colonies.
					At(Location).
					Where(x => x.Owner == Site.Owner).
					Sum(x => x.Development);
			}
		}
		
		public double Research 
		{
			get 
			{ 
				return 0; //TODO
			}
		}
		#endregion
	}
}
