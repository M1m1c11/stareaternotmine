﻿ 

using Ikadn.Ikon.Types;
using Stareater.Utils.Collections;
using System;
using System.Collections.Generic;
using Stareater.GameData;
using Stareater.GameData.Databases;
using Stareater.GameData.Ships;
using Stareater.Players;
using Stareater.Utils;

namespace Stareater.Ships 
{
	partial class Design 
	{
		public string IdCode { get; private set; }
		public Player Owner { get; private set; }
		public bool IsVirtual { get; private set; }
		public string Name { get; private set; }
		private int imageIndex;
		public Component<ArmorType> Armor { get; private set; }
		public Component<HullType> Hull { get; private set; }
		public Component<IsDriveType> IsDrive { get; private set; }
		public Component<ReactorType> Reactor { get; private set; }
		public Component<SensorType> Sensors { get; private set; }
		public Dictionary<Component<SpecialEquipmentType>, int> SpecialEquipment { get; private set; }
		public Component<ThrusterType> Thrusters { get; private set; }
		private BitHash hash;
		public double Cost { get; private set; }

		public Design(string idCode, Player owner, bool isVirtual, string name, int imageIndex, Component<ArmorType> armor, Component<HullType> hull, Component<IsDriveType> isDrive, Component<ReactorType> reactor, Component<SensorType> sensors, Dictionary<Component<SpecialEquipmentType>, int> specialEquipment, Component<ThrusterType> thrusters) 
		{
			this.IdCode = idCode;
			this.Owner = owner;
			this.IsVirtual = isVirtual;
			this.Name = name;
			this.imageIndex = imageIndex;
			this.Armor = armor;
			this.Hull = hull;
			this.IsDrive = isDrive;
			this.Reactor = reactor;
			this.Sensors = sensors;
			initSpecials(specialEquipment);
			this.Thrusters = thrusters;
			
			this.Cost = initCost();
 
			 
		} 

		private Design(Design original, Player owner) 
		{
			this.IdCode = original.IdCode;
			this.Owner = owner;
			this.IsVirtual = original.IsVirtual;
			this.Name = original.Name;
			this.imageIndex = original.imageIndex;
			this.Armor = original.Armor;
			this.Hull = original.Hull;
			this.IsDrive = original.IsDrive;
			this.Reactor = original.Reactor;
			this.Sensors = original.Sensors;
			this.SpecialEquipment = new Dictionary<Component<SpecialEquipmentType>, int>();
			foreach(var item in original.SpecialEquipment)
				this.SpecialEquipment.Add(item.Key, item.Value);
			this.Thrusters = original.Thrusters;
			this.hash = original.hash;
			this.Cost = original.Cost;
 
			 
		}

		private Design(IkonComposite rawData, ObjectDeindexer deindexer) 
		{
			var idCodeSave = rawData[IdCodeKey];
			this.IdCode = idCodeSave.To<string>();

			var ownerSave = rawData[OwnerKey];
			this.Owner = deindexer.Get<Player>(ownerSave.To<int>());

			var isVirtualSave = rawData[IsVirtualKey];
			this.IsVirtual = isVirtualSave.To<int>() >= 0;

			var nameSave = rawData[NameKey];
			this.Name = nameSave.To<string>();

			var imageIndexSave = rawData[ImageIndexKey];
			this.imageIndex = imageIndexSave.To<int>();

			var armorSave = rawData[ArmorKey];
			this.Armor = Component<ArmorType>.Load(armorSave.To<IkonArray>(), deindexer);

			var hullSave = rawData[HullKey];
			this.Hull = Component<HullType>.Load(hullSave.To<IkonArray>(), deindexer);

			if (rawData.Keys.Contains(IsDriveKey))
			{
				var isDriveSave = rawData[IsDriveKey];
				this.IsDrive = Component<IsDriveType>.Load(isDriveSave.To<IkonArray>(), deindexer);
			}

			var reactorSave = rawData[ReactorKey];
			this.Reactor = Component<ReactorType>.Load(reactorSave.To<IkonArray>(), deindexer);

			var sensorsSave = rawData[SensorsKey];
			this.Sensors = Component<SensorType>.Load(sensorsSave.To<IkonArray>(), deindexer);

			var specialEquipmentSave = rawData[SpecialsKey];
			this.SpecialEquipment = new Dictionary<Component<SpecialEquipmentType>, int>();
			foreach(var item in specialEquipmentSave.To<IEnumerable<IkonComposite>>()) {
				var itemKey = item[SpecialKey];
				var itemValue = item[SpecialAmountKey];
				this.SpecialEquipment.Add(
					Component<SpecialEquipmentType>.Load(itemKey.To<IkonArray>(), deindexer),
					itemValue.To<int>()
				);
			}

			var thrustersSave = rawData[ThrustersKey];
			this.Thrusters = Component<ThrusterType>.Load(thrustersSave.To<IkonArray>(), deindexer);

			this.Cost = initCost();
 
			 
		}

		internal Design Copy(PlayersRemap playersRemap) 
		{
			return new Design(this, playersRemap.Players[this.Owner]);
 
		} 
 

		#region Saving
		public IkonComposite Save(ObjectIndexer indexer) 
		{
			var data = new IkonComposite(TableTag);
			data.Add(IdCodeKey, new IkonText(this.IdCode));

			data.Add(OwnerKey, new IkonInteger(indexer.IndexOf(this.Owner)));

			data.Add(IsVirtualKey, new IkonInteger(this.IsVirtual ? 1 : -1));

			data.Add(NameKey, new IkonText(this.Name));

			data.Add(ImageIndexKey, new IkonInteger(this.imageIndex));

			data.Add(ArmorKey, this.Armor.Save());

			data.Add(HullKey, this.Hull.Save());

			if (this.IsDrive != null)
				data.Add(IsDriveKey, this.IsDrive.Save());

			data.Add(ReactorKey, this.Reactor.Save());

			data.Add(SensorsKey, this.Sensors.Save());

			var specialEquipmentData = new IkonArray();
			foreach(var item in this.SpecialEquipment) {
				var itemData = new IkonComposite(EquipmentTag);
				itemData.Add(SpecialKey, item.Key.Save());
				itemData.Add(SpecialAmountKey, new IkonInteger(item.Value));
				specialEquipmentData.Add(itemData);
			}
			data.Add(SpecialsKey, specialEquipmentData);

			data.Add(ThrustersKey, this.Thrusters.Save());
			return data;
 
		}

		public static Design Load(IkonComposite rawData, ObjectDeindexer deindexer)
		{
			var loadedData = new Design(rawData, deindexer);
			deindexer.Add(loadedData);
			return loadedData;
		}
 

		private const string TableTag = "Design";
		private const string IdCodeKey = "idCode";
		private const string OwnerKey = "owner";
		private const string IsVirtualKey = "isVirtual";
		private const string NameKey = "name";
		private const string ImageIndexKey = "imageIndex";
		private const string ArmorKey = "armor";
		private const string HullKey = "hull";
		private const string IsDriveKey = "isDrive";
		private const string ReactorKey = "reactor";
		private const string SensorsKey = "sensors";
		private const string SpecialsKey = "specials";
		private const string EquipmentTag = "Equipment";
		private const string SpecialKey = "Type";
		private const string SpecialAmountKey = "amount";
		private const string ThrustersKey = "thrusters";
		private const string HashKey = "hash";
		private const string CostKey = "cost";
 
		#endregion

 
	}
}
