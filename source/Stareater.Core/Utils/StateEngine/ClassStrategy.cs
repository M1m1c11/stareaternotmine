﻿using Ikadn.Ikon.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Stareater.Utils.StateEngine
{
	class ClassStrategy : ITypeStrategy
	{
		private readonly Func<object> constructor;
		private readonly List<PropertyStrategy> properties;
		private readonly string saveTag;
		
		public ClassStrategy(Type type, StateTypeAttribute attributes)
		{
			this.constructor = buildConstructor(type);
			this.properties = getProperties(type).
				Select(x => new PropertyStrategy(x)).
				ToList();

			this.saveTag = attributes.SaveTag ?? type.Name;
		}

        #region ITypeStrategy implementation
        public object Create(object originalValue)
        {
            return this.constructor();
        }

        public void FillCopy(object originalValue, object copyInstance, CopySession session)
        {
            foreach (var property in this.properties)
                property.Copy(originalValue, copyInstance, session);
        }

		public IEnumerable<object> Dependencies(object originalValue)
		{
			return this.properties.Select(x => x.Get(originalValue)).Where(x => x != null);
		}

		public IkonBaseObject Serialize(object originalValue, SaveSession session)
		{
			var data = new IkonComposite(this.saveTag);
			var reference = session.SaveReference(originalValue, data, this.saveTag);

			foreach (var property in this.properties.Where(x => x.Attribute.DoSave))
				if (property.Get(originalValue) != null)
					data.Add(property.Name, property.Serialize(originalValue, session));

			return reference;
		}

		public object Deserialize(IkonBaseObject rawData, LoadSession session)
		{
			var loadedValue = this.constructor();
			var saveData = rawData.To<IkonComposite>();

			foreach (var property in this.properties.Where(x => x.Attribute.DoSave))
				if (saveData.Keys.Contains(property.Name))
					property.Deserialize(loadedValue, saveData[property.Name].To<IkonBaseObject>(), session);
				else
					property.SetNull(loadedValue);

			return loadedValue;
		}
		#endregion

		private static IEnumerable<PropertyInfo> getProperties(Type type)
		{
			return type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).
				Where(StateManager.IsStateData);
		}
		
		private static Func<object> buildConstructor(Type type)
		{
            var ctorInfo = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[0], null);

			if (ctorInfo == null)
				throw new ArgumentException(type.FullName + " has no default constructor");

			var expr =
				Expression.Lambda<Func<object>>(
					Expression.New(ctorInfo)
				);

			return expr.Compile();
		}
	}
}
