using System;
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
		private IDisposable _connectorSubscription;
		public LogManager LogManager { get; }

		//private readonly string _defaultDataPath = "Data";
		//public string DefaultDataPath => _defaultDataPath;

		private LogService()
		{
			LogManager = new LogManager();
			string defaultDataPath = ConfigService.Instance.DefaultDataPath;
			LogManager.Listeners.Add(new FileLogListener { LogDirectory = Path.Combine(defaultDataPath, "Logs") });

			// ✅ Subscribe to Connector updates from ConnectorService
			//_connectorSubscription = ConnectorService.Instance.ConnectorStream
			//	.Subscribe(OnConnectorUpdated);
		}

		public void SetConnector(Connector connector)
		{
			AddLogSource(connector);
		}

// 		private void OnConnectorUpdated(Connector newConnector)
// 		{
// 			// ✅ Add Connector as a Log Source
// 			AddLogSource(newConnector);
// 		}

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
