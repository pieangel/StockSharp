﻿using System;
using Ecng.Logging;
using StockSharp.Xaml;
using System.IO;
using Ecng.Common;
using System.Reactive.Subjects;
using ReactiveUI;
using StockSharp.Algo;

namespace SciTrader.Services
{
	public class LogService
	{
		private static readonly Lazy<LogService> _instance = new(() => new LogService());
		public static LogService Instance => _instance.Value;
		public LogManager LogManager { get; }

		private LogService()
		{
			LogManager = new LogManager();
			string defaultDataPath = ConfigService.Instance.DefaultDataPath;
			LogManager.Listeners.Add(new FileLogListener { LogDirectory = Path.Combine(defaultDataPath, "Logs") });

		}

		public void SetConnector(Connector connector)
		{
			AddLogSource(connector);
		}

		public void AddGuiListener(GuiLogListener listener)
		{
			LogManager.Listeners.Add(listener);
		}

		public void AddLogSource(ILogSource source)
		{
			LogManager.Sources.Add(source);
		}

		public void LogDebug(string message, Exception ex = null)
		{
			LogManager.Application.AddDebugLog(message, ex);
		}
	}

}
