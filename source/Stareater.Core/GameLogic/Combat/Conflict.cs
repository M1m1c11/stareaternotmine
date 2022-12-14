using System.Collections.Generic;
using Stareater.GameLogic.Planning;
using Stareater.Utils;

namespace Stareater.GameLogic.Combat
{
	class Conflict
	{
		public Vector2D Location { get; private set; }
		public IEnumerable<FleetMovement> Fleets { get; private set; }
		public double StartTime { get; private set; }
		
		public Conflict(Vector2D location, IEnumerable<FleetMovement> fleets, double startTime)
		{
			this.Location = location;
			this.Fleets = fleets;
			this.StartTime = startTime;
		}
	}
}
