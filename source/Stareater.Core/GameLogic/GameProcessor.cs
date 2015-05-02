﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Stareater.Ships.Missions;
using Stareater.Galaxy;

namespace Stareater.GameLogic
{
	class GameProcessor
	{
		private Game game;

		public GameProcessor(Game game)
		{
			this.game = game;
		}

		public void ProcessPrecombat()
		{
			this.CalculateBaseEffects();
			this.CalculateSpendings();
			this.CalculateDerivedEffects();
			this.commitFleetOrders();

			this.game.States.Reports.Clear();
			foreach (var playerProc in this.game.Derivates.Players)
				playerProc.ProcessPrecombat(
					this.game.Statics,
					this.game.States,
					this.game.Derivates.Colonies.OwnedBy(playerProc.Player),
					this.game.Derivates.Stellarises.OwnedBy(playerProc.Player)
				);

			this.moveShips();
			/*
			 * TODO(v0.5): Process ships
			 * - Space combat
			 * - Ground combat
			 * - Bombardment
			 * - Colonise planets
			 */
		}

		public void ProcessPostcombat()
		{
			// TODO(v0.5): Process research
			foreach (var playerProc in this.game.Derivates.Players)
				playerProc.ProcessPostcombat(this.game.Statics, this.game.States, this.game.Derivates);

			// TODO(v0.5): Update ship designs

			// TODO(v0.5): Upgrade and repair ships

			/*
			 * TODO(v0.5): Colonies, 2nd pass
			 * - Apply normal effect buildings
			 * - Check construction queue
			 * - Recalculate colony effects
			 */

			CalculateBaseEffects();
			CalculateSpendings();
			CalculateDerivedEffects();
		}

		public void CalculateBaseEffects()
		{
			foreach (var stellaris in this.game.Derivates.Stellarises)
				stellaris.CalculateBaseEffects();
			foreach (var colonyProc in this.game.Derivates.Colonies)
				colonyProc.CalculateBaseEffects(this.game.Statics, this.game.Derivates.Of(colonyProc.Owner));
		}

		public void CalculateSpendings()
		{
			foreach (var colonyProc in this.game.Derivates.Colonies)
				colonyProc.CalculateSpending(
					this.game.Statics,
					this.game.Derivates.Of(colonyProc.Owner)
				);

			foreach (var stellaris in this.game.Derivates.Stellarises)
				stellaris.CalculateSpending(
					this.game.Derivates.Of(stellaris.Owner),
					this.game.Derivates.Colonies.At(stellaris.Location)
				);

			foreach (var player in this.game.Derivates.Players) {
				player.CalculateDevelopment(
					this.game.Statics,
					this.game.States,
					this.game.Derivates.Colonies.OwnedBy(player.Player)
				);
				player.CalculateResearch(
					this.game.Statics,
					this.game.States,
					this.game.Derivates.Colonies.OwnedBy(player.Player)
				);
			}
		}

		public void CalculateDerivedEffects()
		{
			foreach (var colonyProc in this.game.Derivates.Colonies)
				colonyProc.CalculateDerivedEffects(this.game.Statics, this.game.Derivates.Of(colonyProc.Owner));
		}

		private void commitFleetOrders()
		{
			foreach (var player in this.game.Players) {
				foreach (var order in player.Orders.ShipOrders) {
					foreach (var fleet in this.game.States.Fleets.At(order.Key).Where(x => x.Owner == player))
						this.game.States.Fleets.PendRemove(fleet);

					this.game.States.Fleets.ApplyPending();
					foreach (var fleet in order.Value)
						this.game.States.Fleets.Add(fleet);
				}

				player.Orders.ShipOrders.Clear();
			}
		}

		private void moveShips()
		{
			foreach (var fleet in this.game.States.Fleets)
				if (fleet.Mission.Type == MissionType.Move) {
					this.game.States.Fleets.PendRemove(fleet);
					var mission = fleet.Mission as MoveMission;
					//TODO(v0.5) calculate speed from ships
					double speed = 1;
					var waypoints = mission.Waypoints.Skip(1).ToArray();
					var distance = (waypoints[0] - fleet.Position).Magnitude();

					//TODO(v0.5) loop through all waypoints
					//TODO(v0.5) detect conflicts
					if (distance <= speed) {
						var newFleet = new Fleet(
							fleet.Owner,
							waypoints[0],
							new StationaryMission(this.game.States.Stars.At(waypoints[0]))
						);
						newFleet.Ships.Add(fleet.Ships);
						this.game.States.Fleets.PendAdd(newFleet);
					}
					else {
						var direction = (waypoints[0] - fleet.Position);
						direction.Normalize();

						var newFleet = new Fleet(
							fleet.Owner,
							fleet.Position + direction * speed,
							fleet.Mission
						);
						newFleet.Ships.Add(fleet.Ships);
						this.game.States.Fleets.PendAdd(newFleet);
					}
				}

			this.game.States.Fleets.ApplyPending();
		}
	}
}
