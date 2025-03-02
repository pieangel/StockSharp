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
		private static readonly Lazy<ConnectorService> _instance =
			new(() => new ConnectorService());

		public static ConnectorService Instance => _instance.Value;

		public void SetConnector(Connector connector)
		{
			_connector = connector;
		}

		private Connector? _connector;

		public Connector? GetConnector() => _connector;

		private ConnectorService() { }
	}
}
