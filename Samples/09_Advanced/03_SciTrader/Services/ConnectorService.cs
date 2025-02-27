using StockSharp.Algo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SciTrader.Services
{
	public class ConnectorService
	{
		public Connector TradingConnector { get; }
		private readonly MainWindowService _mainWindowService;
		private readonly LogService _logService;

		public ConnectorService(MainWindowService mainWindowService, LogService logService)
		{
			_mainWindowService = mainWindowService;
			_logService = logService;

			TradingConnector = new Connector();
			TradingConnector.Connected += OnConnected;
			TradingConnector.ConnectionError += OnConnectionError;
			TradingConnector.Disconnected += OnDisconnected;
		}

		private void OnConnected()
		{
			_logService.LogInfo("Connected to the exchange.");
			_mainWindowService.ChangeConnectionStatus(true);
		}

		private void OnConnectionError(Exception error)
		{
			_logService.LogError($"Connection error: {error.Message}");
			_mainWindowService.ShowMessage(error.ToString(), "Connection Error");
		}

		private void OnDisconnected()
		{
			_logService.LogInfo("Disconnected from exchange.");
			_mainWindowService.ChangeConnectionStatus(false);
		}

		public void Connect() => TradingConnector.Connect();
		public void Disconnect() => TradingConnector.Disconnect();
	}

}
