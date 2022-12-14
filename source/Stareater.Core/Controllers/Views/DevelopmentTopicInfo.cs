using System.Collections.Generic;
using Stareater.GameData;
using Stareater.Localization;
using Stareater.Utils.Collections;
using Stareater.GameLogic.Planning;

namespace Stareater.Controllers.Views
{
	public class DevelopmentTopicInfo
	{
		internal const string LangContext = "Technologies";
		
		private readonly DevelopmentTopic topic;
		private IDictionary<string, double> textVars;
		
		public double Cost { get; private set; }
		public double InvestedPoints { get; private set; }
		public double Investment { get; private set; }
		public int Level { get; private set; }
		public int NextLevel { get; private set; }

		internal DevelopmentTopicInfo(DevelopmentProgress tech)
		{
			this.topic = tech.Topic;
			this.textVars = new Var(DevelopmentTopic.LevelKey, tech.NextLevel).
				And(DevelopmentTopic.PriorityKey, tech.Priority).Get;
				
			this.Cost = tech.Topic.Cost.Evaluate(textVars);
			this.InvestedPoints = tech.InvestedPoints;
			this.Investment = 0;
			this.Level = tech.Level;
			this.NextLevel = tech.NextLevel;
		}
		
		internal DevelopmentTopicInfo(DevelopmentProgress tech, DevelopmentResult investmentResult)
		{
			this.topic = tech.Topic;
			this.textVars = new Var(DevelopmentTopic.LevelKey, tech.NextLevel).
				And(DevelopmentTopic.PriorityKey, tech.Priority).Get;
				
			this.Cost = tech.Topic.Cost.Evaluate(textVars);
			this.InvestedPoints = tech.InvestedPoints;
			this.Investment = investmentResult.InvestedPoints;
			this.Level = tech.Level;
			this.NextLevel = investmentResult.CompletedCount > 1 ? tech.Level + (int)investmentResult.CompletedCount : tech.NextLevel;
		}
		
		public string Name 
		{
			get 
			{
				return LocalizationManifest.Get.CurrentLanguage[LangContext].Name(topic.LanguageCode).Text(textVars);
			}
		}
		
		public string Description 
		{ 
			get 
			{
				return LocalizationManifest.Get.CurrentLanguage[LangContext].Description(topic.LanguageCode).Text(textVars);
			}
		}
		
		public string ImagePath 
		{
			get
			{
				return topic.ImagePath;
			}
		}
		
		public int MaxLevel 
		{ 
			get 
			{
				return topic.MaxLevel;
			}
		}
		
		public string IdCode 
		{
			get 
			{
				return topic.IdCode;
			}
		}
		
		#region Equals and GetHashCode implementation
		public override bool Equals(object obj)
		{
			var other = obj as DevelopmentTopicInfo;
			return other != null && object.Equals(this.topic, other.topic);
		}

		public override int GetHashCode()
		{
			return topic != null ? topic.GetHashCode() : 0;
		}

		public static bool operator ==(DevelopmentTopicInfo lhs, DevelopmentTopicInfo rhs) {
			if (ReferenceEquals(lhs, rhs))
				return true;
			if (ReferenceEquals(lhs, null) || ReferenceEquals(rhs, null))
				return false;
			return lhs.Equals(rhs);
		}

		public static bool operator !=(DevelopmentTopicInfo lhs, DevelopmentTopicInfo rhs) {
			return !(lhs == rhs);
		}
		#endregion
	}
}
