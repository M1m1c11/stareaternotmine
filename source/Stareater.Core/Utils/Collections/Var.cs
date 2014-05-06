﻿using System;
using System.Collections.Generic;

namespace Stareater.Utils.Collections
{
	public class Var
	{
		Dictionary<string, double> variables = new Dictionary<string, double>();

		public Var()
		{ }

		public Var(string name, double value)
		{
			variables.Add(name, value);
		}

		public Var And(string name, double value)
		{
			variables.Add(name, value);
			return this;
		}

		public IDictionary<string, double> Get
		{
			get {
				return this.variables;
			}
		}
		
		public Var UnionWith(IEnumerable<KeyValuePair<string, double>> variables)
		{
			foreach (var pair in variables)
				this.variables.Add(pair.Key, pair.Value);
			
			return this;
		}
		
		public Var UnionWith(IEnumerable<KeyValuePair<string, int>> variables)
		{
			foreach (var pair in variables)
				this.variables.Add(pair.Key, pair.Value);
			
			return this;
		}
	}
}
