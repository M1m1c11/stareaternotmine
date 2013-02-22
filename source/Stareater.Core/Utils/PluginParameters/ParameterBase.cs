﻿using Stareater.AppData;

namespace Stareater.Utils.PluginParameters
{
	public abstract class ParameterBase
	{
		protected string contextKey { get; private set; }
		private string nameKey;

		public ParameterBase(string contextKey, string nameKey)
		{
			this.contextKey = contextKey;
			this.nameKey = nameKey;
		}

		public string Name
		{
			get { return Settings.Get.Language[contextKey][nameKey].Text(); }
		}
	}
}
