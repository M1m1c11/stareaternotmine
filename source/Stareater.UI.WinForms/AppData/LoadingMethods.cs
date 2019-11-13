﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Stareater.Galaxy;
using Stareater.Galaxy.Builders;
using Stareater.Localization;
using System.IO;
using Stareater.Players;
using Ikadn.Utilities;

namespace Stareater.AppData
{
	static class LoadingMethods
	{
		#region Localization
		private const string LanguagesFolder = "./languages/";
		private const string DefaultLangSufix = "(default)";

		private static string defaultLangCode;

		public static void InitializeLocalization()
		{
			Language currentLanguage = null;
			Language defaultLanguage = null;
			var infos = new List<LanguageInfo>();

			foreach (var folder in new DirectoryInfo(dataFolder + LanguagesFolder).EnumerateDirectories())
			{
				string code = folder.Name;

				if (folder.Name.EndsWith(DefaultLangSufix, StringComparison.InvariantCultureIgnoreCase))
				{
					code = code.Remove(code.Length - DefaultLangSufix.Length);
					defaultLangCode = code;
				}
				
				Language lang = LoadLanguage(code);

				if (code == defaultLangCode)
					defaultLanguage = lang;
				if (code == SettingsWinforms.Get.LanguageId)
					currentLanguage = lang;

				
				infos.Add(new LanguageInfo(code, lang["General"]["LanguageName"].Text()));
			}

			if (defaultLanguage == null)
				throw new FileNotFoundException("No default language found (language folder with " + DefaultLangSufix + " sufix)");

			LocalizationManifest.Initialize(infos, defaultLanguage, currentLanguage);

			if (SettingsWinforms.Get.LanguageId == null)
				SettingsWinforms.Get.ChangeLanguage(defaultLanguage.Code, defaultLanguage);
		}

		public static Language LoadLanguage(string langCode)
		{
			var folderSufix = langCode == defaultLangCode ? DefaultLangSufix : "";

			return new Language(
				langCode,
				dataStreams(new DirectoryInfo(dataFolder + LanguagesFolder + langCode + folderSufix).EnumerateFiles())
			);
		}
		#endregion

		#region Player assets
		private const string AIsFolder = "./players/";
		private static readonly string[] OrganizationFiles = { "./data/organizations.txt" };
		private static readonly string[] PlayersColorFiles = { "./data/playerData.txt" };
		
		public static void LoadAis()
		{
			PlayerAssets.AILoader(loadFromDLLs<IOffscreenPlayerFactory>(pluginFolder + AIsFolder));
		}
		
		public static void LoadOrganizations()
		{
			PlayerAssets.OrganizationsLoader(dataStreams(OrganizationFiles.Select(x => new FileInfo(dataFolder + x))));
		}
		
		public static void LoadPlayerColors()
		{
			PlayerAssets.ColorLoader(dataStreams(PlayersColorFiles.Select(x => new FileInfo(dataFolder + x))));
		}
		#endregion
		
		#region Map assets
		private static readonly string[] StartConditionsFiles = { "./data/startConditions.txt" };
		public const string MapsFolder = "./maps/";
		
		public static void LoadStarConnectors()
		{
			MapAssets.ConnectorsLoader(loadFromDLLs<IStarConnector>(
				pluginFolder + MapsFolder, 
				x => x.Initialize(dataFolder + MapsFolder)
			));
		}
		
		public static void LoadStarPopulators()
		{
			MapAssets.PopulatorsLoader(loadFromDLLs<IStarPopulator>(
				pluginFolder + MapsFolder, 
				x => x.Initialize(dataFolder + MapsFolder)
			));
		}
		
		public static void LoadStarPositioners()
		{
			MapAssets.PositionersLoader(loadFromDLLs<IStarPositioner>(
				pluginFolder + MapsFolder, 
				x => x.Initialize(dataFolder + MapsFolder)
			));
		}
		
		public static void LoadStartConditions()
		{
			MapAssets.StartConditionsLoader(dataStreams(StartConditionsFiles.Select(x => new FileInfo(dataFolder + x))));
		}
		#endregion
		
		#region Game data
		private const string StaticDataFolder = "./data/statics/";
		
		public static IEnumerable<NamedStream> GameDataSources()
		{
			return dataStreams(new DirectoryInfo(dataFolder + StaticDataFolder).EnumerateFiles());
		}
		#endregion
		
		private static IEnumerable<NamedStream> dataStreams(IEnumerable<FileInfo> files)
		{
			foreach (var file in files)
			{
				var stream = new StreamReader(file.FullName);
				yield return new NamedStream(stream, file.FullName);
				stream.Close();
			}
		}

		private static string dataFolder
		{
			get { return SettingsWinforms.Get.DataRootPath ?? ""; }
		}

		private static string pluginFolder
		{
			get { return SettingsWinforms.Get.PluginRootPath ?? ""; }
		}

		private static IEnumerable<T> loadFromDLLs<T>(string folderPath, Action<T> initFunction = null)
		{
			var dllFiles = new List<FileInfo>(new DirectoryInfo(folderPath).EnumerateFiles("*.dll"));
			Type targetType = typeof(T);

			//TODO(later) consider more secure approach
			return dllFiles.
				SelectMany(file => Assembly.UnsafeLoadFrom(file.FullName).GetTypes()).
				Where(type => targetType.IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface).
				Select(type =>
				{
					var instance = (T)Activator.CreateInstance(type);
					initFunction?.Invoke(instance);

					return instance;
				});
		}
	}
}
