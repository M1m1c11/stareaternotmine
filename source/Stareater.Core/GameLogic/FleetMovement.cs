﻿using System;
using NGenerics.DataStructures.Mathematical;
using Stareater.Galaxy;

namespace Stareater.GameLogic
{
	class FleetMovement
	{
		public Fleet OriginalFleet { get; private set; }
		public Fleet LocalFleet { get; private set; }
		public double ArrivalTime { get; private set; }
		public double DepartureTime { get; private set; }
		public Vector2D MovementDirection { get; private set; }
		public bool Remove { get; private set; }
		
		public FleetMovement(Fleet originalFleet, Fleet localFleet, double arrivalTime, double departureTime, Vector2D movementDirection, bool remove)
		{
			this.OriginalFleet = originalFleet;
			this.LocalFleet = localFleet;
			this.ArrivalTime = arrivalTime;
			this.DepartureTime = departureTime;
			this.MovementDirection = movementDirection;
			this.Remove = remove;
			
			if (movementDirection.Magnitude() > 0)
				movementDirection.Normalize();
		}
	}
}
