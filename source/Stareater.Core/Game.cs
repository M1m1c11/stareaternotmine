﻿using System;
using System.Collections.Generic;
using System.Linq;
using Stareater.Controllers.Data;
using Stareater.Galaxy;
using Stareater.Galaxy.Builders;
using Stareater.GameData;
using Stareater.GameData.Databases;
using Stareater.GameData.Databases.Tables;
using Stareater.GameLogic;
using Stareater.Players;
using Stareater.Utils;
using Ikadn.Ikon.Types;

namespace Stareater
{
	class Game
	{
		public Player[] Players { get; private set; }
		public int Turn { get; private set; }
		public int CurrentPlayerIndex { get; private set; } //FIXME(later): assumes single player, remove
		private IEnumerable<object> conflicts; //TODO(v0.5): make type

		public StaticsDB Statics { get; private set; }
		public StatesDB States { get; private set; }
		public TemporaryDB Derivates { get; private set; }
			
		public Game(Player[] players, StaticsDB statics, StatesDB states, TemporaryDB derivates)
		{
			this.Turn = 0;
			this.CurrentPlayerIndex = 0;
			
			this.Players = players;
			this.Statics = statics;
			this.States = states;
			this.Derivates = derivates;
		}

		private Game()
		{ }

		public Player CurrentPlayer
		{ 
			get { return this.Players[CurrentPlayerIndex]; }
		}
		
		public GameCopy ReadonlyCopy()
		{
			Game copy = new Game();

			GalaxyRemap galaxyRemap = this.States.CopyGalaxy();
			PlayersRemap playersRemap = this.States.CopyPlayers(
				this.Players.ToDictionary(x => x, x => x.Copy(galaxyRemap)),
				galaxyRemap);

			foreach (var playerPair in playersRemap.Players)
				playerPair.Value.Orders = playerPair.Key.Orders.Copy(playersRemap, galaxyRemap);

			copy.Players = this.Players.Select(p => playersRemap.Players[p]).ToArray();
			copy.Turn = this.Turn;
			copy.CurrentPlayerIndex = this.CurrentPlayerIndex;

			copy.Statics = this.Statics;
			copy.States = this.States.Copy(playersRemap, galaxyRemap);
			copy.Derivates = this.Derivates.Copy(playersRemap);

			return new GameCopy(copy, playersRemap, galaxyRemap);
		}
		
		public void CalculateBaseEffects()
		{
			foreach (var stellaris in this.Derivates.Stellarises)
				stellaris.CalculateBaseEffects();
			foreach(var colonyProc in this.Derivates.Colonies)
				colonyProc.CalculateBaseEffects(Statics, Derivates.Of(colonyProc.Owner));
		}
		
		public void CalculateSpendings()
		{
			foreach(var colonyProc in this.Derivates.Colonies)
				colonyProc.CalculateSpending(Statics, Derivates.Of(colonyProc.Owner));
			foreach (var stellaris in this.Derivates.Stellarises)
				stellaris.CalculateSpending(
					Derivates.Of(stellaris.Owner),
					this.Derivates.Colonies.At(stellaris.Location)
				);
		}
		
		public void CalculateDerivedEffects()
		{
			foreach(var colonyProc in this.Derivates.Colonies)
				colonyProc.CalculateDerivedEffects(Statics, Derivates.Of(colonyProc.Owner));
		}
		
		public void ProcessPrecombat()
		{
			CalculateBaseEffects();
			CalculateSpendings();
			CalculateDerivedEffects();
			
			foreach(var playerProc in this.Derivates.Players)
				playerProc.ProcessPrecombat(
					Statics,
					States,
					this.Derivates.Colonies.OwnedBy(playerProc.Player),
					this.Derivates.Stellarises.OwnedBy(playerProc.Player)
				);
			
			//TODO(v0.5): add new tech level message
			/*
			 * TODO(v0.5): Process ships
			 * - Move ships
			 * - Space combat
			 * - Ground combat
			 * - Bombardment
			 * - Colonise planets
			 */
		}
		
		public void ProcessPostcombat()
		{
			// TODO(v0.5): Process research
			foreach(var playerProc in this.Derivates.Players)
				playerProc.ProcessPostcombat(Statics, States, Derivates);
			
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

		#region Saving
		internal IkonComposite Save()
		{
			IkonComposite gameData = new IkonComposite(SaveGameTag);
			var playersData = new IkonArray();
			var ordersData = new IkonArray();

			gameData.Add(StatesKey, this.States.Save());

			//TODO(v0.5) implement
			/*foreach(var player in this.Players)
				playersData.Add(player.Save());
			gameData.Add(PlayersKey, playersData);
			
			foreach(var player in this.Players)
				ordersData.Add(player.Orders.Save());
			gameData.Add(OrdersKey, playersData);
			*/
			return gameData;
		}

		private const string SaveGameTag = "Game";
		private const string OrdersKey = "orders";
		private const string PlayersKey = "players";
		private const string StatesKey = "states";
		#endregion
	}
}
