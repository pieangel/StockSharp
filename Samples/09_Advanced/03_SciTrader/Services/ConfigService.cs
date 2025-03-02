using Ecng.Common;
using StockSharp.Configuration;
using System;
using System.IO;

namespace SciTrader.Services
{
	public class ConfigService
    {
		private static readonly Lazy<ConfigService> _instance = new(() => new ConfigService());
		public static ConfigService Instance => _instance.Value;

		private readonly string _defaultDataPath = "Data";
		public string DefaultDataPath => _defaultDataPath;
		public string SettingsFile => _settingsFile;
		private readonly string _settingsFile;
		private ConfigService()
		{
			_defaultDataPath = _defaultDataPath.ToFullPath();
			_settingsFile = Path.Combine(_defaultDataPath, $"connection{Paths.DefaultSettingsExt}");
		}
	}
}
