using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockSharp.BusinessEntities;

namespace SciTrader.Services
{
	public class MarketDataService
	{
		private readonly ConnectorService _connectorService;

		public MarketDataService(ConnectorService connectorService)
		{
			_connectorService = connectorService;

			// Subscribe to Market Data Events
			var connector = _connectorService.Connector;

			connector.SecurityReceived += (s, sec) => Console.WriteLine($"Security Received: {sec.Code}");
			connector.TickTradeReceived += (s, t) => Console.WriteLine($"Trade: {t.SecurityId} - {t.Price}");
			connector.Level1Received += (s, l) => Console.WriteLine($"Level1 Data: {l.SecurityId}");
		}

		public void SubscribeToSecurity(string symbol)
		{
			//var security = new Security { Code = symbol, Board = ExchangeBoards.Nasdaq };
			//_connectorService.TradingConnector.RegisterMarketDepth(security);
		}
	}

}
