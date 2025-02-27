using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockSharp.BusinessEntities;
using StockSharp.Algo.Positions;
using StockSharp.Algo.Risk;

namespace SciTrader.Services
{
	public class PortfolioService
	{
		private readonly ConnectorService _connectorService;

		public PortfolioService(ConnectorService connectorService)
		{
			_connectorService = connectorService;
			var connector = _connectorService.TradingConnector;

			connector.PositionReceived += (sub, p) => Console.WriteLine($"Position Received: {p.CurrentValue}");
		}
	}
}
