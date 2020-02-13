﻿using Stareater.GameLogic.Planning;
using Stareater.Utils.StateEngine;

namespace Stareater.Players.Reports
{
	[StateTypeAttribute(saveTag: SaveTag)]
	class ResearchReport : IReport
	{
		[StatePropertyAttribute]
		public ResearchResult TechProgress { get; private set; }
		
		internal ResearchReport(ResearchResult techProgress)
		{
			this.TechProgress = techProgress;
		}

		private ResearchReport()
		{ }

		public Player Owner {
			get {
				return this.TechProgress.Item.Owner;
			}
		}
		
		public void Accept(IReportVisitor visitor)
		{
			visitor.Visit(this);
		}
		
		public const string SaveTag = "ResearchReport";
	}
}
