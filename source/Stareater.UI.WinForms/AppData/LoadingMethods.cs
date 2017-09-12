﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Stareater.Galaxy;
using Stareater.Galaxy.Builders;
using Stareater.Localization;
using System.IO;
using Stareater.Players;
using Stareater.Utils;

namespace Stareater.AppData
{
	static class LoadingMethods
	{
		#region Localization
		private const string LanguagesFolder = "./languages/";
		private const string DefaultLangSufix = "(default)";
		private static string DefaultLangCode = null;

		public static void InitializeLocalization()
		{
			Language currentLanguage = null;
			Language defaultLanguage = null;
			var infos = new List<LanguageInfo>();

			foreach (var folder in new DirectoryInfo(rootFolder + LanguagesFolder).EnumerateDirectories())
			{
				string code = folder.Name;

				if (folder.Name.EndsWith(DefaultLangSufix, StringComparison.InvariantCultureIgnoreCase))
				{
					code = code.Remove(code.Length - DefaultLangSufix.Length);
					DefaultLangCode = code;
				}
				
				Language lang = LoadLanguage(code);

				if (code == DefaultLangCode)
					defaultLanguage = lang;
				if (code == SettingsWinforms.Get.LanguageId)
					currentLanguage = lang;

				
				infos.Add(new LanguageInfo(code, lang["General"]["LanguageName"].Text()));
			}

			LocalizationManifest.Initialize(infos, defaultLanguage, currentLanguage);

			if (SettingsWinforms.Get.LanguageId == null)
				SettingsWinforms.Get.ChangeLanguage(defaultLanguage.Code, defaultLanguage);
		}

		public static Language LoadLanguage(string langCode)
		{
			var folderSufix = langCode == DefaultLangCode ? DefaultLangSufix : "";

			return new Language(
				langCode,
				dataStreams(new DirectoryInfo(rootFolder + LanguagesFolder + langCode + folderSufix).EnumerateFiles())
			);
		}
		#endregion

		#region Player assets
		private const string AIsFolder = "./players/";
		private static readonly string[] OrganizationFiles = { "./data/organizations.txt" };
		private static readonly string[] PlayersColorFiles = { "./data/playerData.txt" };
		
		public static void LoadAis()
		{
			PlayerAssets.AILoader(loadFromDLLs<IOffscreenPlayerFactory>(rootFolder + AIsFolder));
		}
		
		public static void LoadOrganizations()
		{
			PlayerAssets.OrganizationsLoader(dataStreams(OrganizationFiles.Select(x => new FileInfo(rootFolder + x))));
		}
		
		public static void LoadPlayerColors()
		{
			PlayerAssets.ColorLoader(dataStreams(PlayersColorFiles.Select(x => new FileInfo(rootFolder + x))));
		}
		#endregion
		
		#region Map assets
		private static readonly string[] StartConditionsFiles = { "./data/startConditions.txt" };
		public const string MapsFolder = "./maps/";
		
		public static void LoadStarConnectors()
		{
			MapAssets.ConnectorsLoader(loadFromDLLs<IStarConnector>(rootFolder + MapsFolder));
		}
		
		public static void LoadStarPopulators()
		{
			MapAssets.PopulatorsLoader(loadFromDLLs<IStarPopulator>(rootFolder + MapsFolder));
		}
		
		public static void LoadStarPositioners()
		{
			MapAssets.PositionersLoader(loadFromDLLs<IStarPositioner>(rootFolder + MapsFolder));
		}
		
		public static void LoadStartConditions()
		{
			MapAssets.StartConditionsLoader(dataStreams(StartConditionsFiles.Select(x => new FileInfo(rootFolder + x))));
		}
		#endregion
		
		#region Game data
		private static readonly string StaticDataFolder = "./data/statics/";
		
		public static IEnumerable<TracableStream> GameDataSources()
		{
			return dataStreams(new DirectoryInfo(rootFolder + StaticDataFolder).EnumerateFiles());
		}
		#endregion
		
		private static IEnumerable<TracableStream> dataStreams(IEnumerable<FileInfo> files)
		{
			foreach (var file in files)
			{
				var stream = new StreamReader(file.FullName);
				yield return new TracableStream(stream, file.FullName);
				stream.Close();
			}
		}

		private static string rootFolder
		{
			get { return SettingsWinforms.Get.DataRootPath ?? ""; }
		}
		
		private static IEnumerable<T> loadFromDLLs<T>(string folderPath)
		{
			var dllFiles = new List<FileInfo>(new DirectoryInfo(folderPath).EnumerateFiles("*.dll"));
			Type targetType = typeof(T);
			
			foreach (var file in dllFiles)
				foreach (var type in Assembly.UnsafeLoadFrom(file.FullName).GetTypes()) //TODO(later) consider more secure approach
					if (targetType.IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface)
						yield return (T)Activator.CreateInstance(type);
		}
	}
}
