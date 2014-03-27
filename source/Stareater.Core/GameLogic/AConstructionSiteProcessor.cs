﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Stareater.Galaxy;
using Stareater.GameData;
using Stareater.GameData.Databases;
using Stareater.Utils.Collections;

namespace Stareater.GameLogic
{
	abstract class AConstructionSiteProcessor
	{
		private const string BuidingCountPrefix = "_count";
		
		public double Production { get; protected set; }
		public double SpendingRatioEffective { get; protected set; }
		public IEnumerable<ConstructionResult> SpendingPlan { get; protected set; }

		protected AConstructionSiteProcessor()
		{
			this.SpendingPlan = new ConstructionResult[0];
		}
		
		protected AConstructionSiteProcessor(AConstructionSiteProcessor original)
		{
			this.Production = original.Production;
			this.SpendingPlan = new List<ConstructionResult>(original.SpendingPlan);
			this.SpendingRatioEffective = original.SpendingRatioEffective;
		}
		
		public virtual Var LocalEffects(StaticsDB statics)
		{
			var vars = new Var();
			
			foreach(var building in statics.Buildings)
				if (Site.Buildings.ContainsKey(building.Key))
					vars.And(building.Key.ToLower() + BuidingCountPrefix, Site.Buildings[building.Key]);
				else
					vars.And(building.Key.ToLower() + BuidingCountPrefix, 0);
			
			return vars;
		}

		public virtual void ProcessPrecombat()
		{
			foreach (var construction in SpendingPlan) 
				if (construction.DoneCount >= 1)
					foreach(var effect in construction.Item.Effects)
						effect.Apply(Site, construction.DoneCount);
		}
		
		protected abstract AConstructionSite Site { get; }
		
		protected static IEnumerable<ConstructionResult> SimulateSpending(
			AConstructionSite site, double industryPoints, 
			IEnumerable<Constructable> queue, IDictionary<string, double> vars)
		{
			var spendingPlan = new List<ConstructionResult>();

			foreach (var buildingItem in queue) {
				if (industryPoints <= 0 || buildingItem.Condition.Evaluate(vars) < 0) {
					spendingPlan.Add(new ConstructionResult(0, 0, buildingItem, 0));
					continue;
				}
				
				double cost = buildingItem.Cost.Evaluate(vars);
				double investment = industryPoints;

				if (site.Stockpile.ContainsKey(buildingItem))
					investment += site.Stockpile[buildingItem];

				double completed = Math.Floor(investment / cost); //FIXME(v0.5): possible division by zero
				double countLimit = buildingItem.TurnLimit.Evaluate(vars);

				if (completed > countLimit) {
					spendingPlan.Add(new ConstructionResult(
						countLimit,
						countLimit * cost,
						buildingItem,
						0
					));

					industryPoints -= countLimit * cost;
				}
				else {
					spendingPlan.Add(new ConstructionResult(
						completed,
						investment,
						buildingItem,
						investment - completed * cost
					));

					industryPoints = 0;
				}
			}

			return spendingPlan;
		}
	}
}
