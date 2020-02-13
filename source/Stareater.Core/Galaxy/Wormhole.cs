﻿using Stareater.Utils;
using Stareater.Utils.StateEngine;

namespace Stareater.Galaxy
{
	class Wormhole
	{
		[StatePropertyAttribute]
		public Pair<StarData> Endpoints { get; private set; }

		public Wormhole(StarData fromStar, StarData toStar) 
		{
			this.Endpoints = new Pair<StarData>(fromStar, toStar);
 		} 

		private Wormhole() 
		{ }

		public override string ToString()
		{
			return this.Endpoints.First.ToString() + " - " + this.Endpoints.Second.ToString();
		}
	}
}
