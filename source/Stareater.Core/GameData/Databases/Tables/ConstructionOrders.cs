﻿ 

using Ikadn.Ikon.Types;
using Stareater.Utils.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Stareater.GameData.Databases.Tables 
{
	partial class ConstructionOrders 
	{
		public double SpendingRatio { get; set; }
		public List<Constructable> Queue { get; private set; }

		public ConstructionOrders(double spendingRatio) 
		{
			this.SpendingRatio = spendingRatio;
			this.Queue = new List<Constructable>();
 
		} 

		private ConstructionOrders(ConstructionOrders original) 
		{
			this.SpendingRatio = original.SpendingRatio;
			this.Queue = new List<Constructable>();
			foreach(var item in original.Queue)
				this.Queue.Add(item);
 
		}

		private  ConstructionOrders(IkonComposite rawData, ObjectDeindexer deindexer) 
		{
			var spendingRatioSave = rawData[SpendingRatioKey];
			this.SpendingRatio = spendingRatioSave.To<double>();

			var queueSave = rawData[QueueKey];
			this.Queue = new List<Constructable>();
			foreach(var item in queueSave.To<IkonArray>())
				this.Queue.Add(deindexer.Get<Constructable>(item.To<string>()));
 
		}

		internal ConstructionOrders Copy() 
		{
			return new ConstructionOrders(this);
 
		} 
 

		#region Saving
		public IkonComposite Save(ObjectIndexer indexer) 
		{
			var data = new IkonComposite(TableTag);
			data.Add(SpendingRatioKey, new IkonFloat(this.SpendingRatio));

			var queueData = new IkonArray();
			foreach(var item in this.Queue)
				queueData.Add(new IkonText(item.IdCode));
			data.Add(QueueKey, queueData);
			return data;
 
		}

		public static ConstructionOrders Load(IkonComposite rawData, ObjectDeindexer deindexer)
		{
			var loadedData = new ConstructionOrders(rawData, deindexer);
			deindexer.Add(loadedData);
			return loadedData;
		}
 

		private const string TableTag = "ConstructionOrders";
		private const string SpendingRatioKey = "spendingRatio";
		private const string QueueKey = "queue";
 
		#endregion
	}
}
