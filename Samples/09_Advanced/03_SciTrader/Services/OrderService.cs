using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockSharp.BusinessEntities;
using StockSharp.Messages;

namespace SciTrader.Services
{
	public class OrderService
	{
		private readonly ConnectorService _connectorService;

		public OrderService(ConnectorService connectorService)
		{
			_connectorService = connectorService;
			var connector = _connectorService.Connector;

			// Order Events
			connector.NewOrder += Connector_OnNewOrder;
			connector.OrderChanged += Connector_OnOrderChanged;
			//connector.OrdersRegisterFailed += Connector_OnOrderRegisterFailed;
		}

		public void PlaceLimitOrder(string symbol, decimal price, int volume, Sides side)
		{
			var order = new Order
			{
				Security = new Security { Code = symbol, Board = ExchangeBoard.Nasdaq },
				Portfolio = new Portfolio { Name = "MyPortfolio" },
				Volume = volume,
				Side = side,
				Price = price,
				Type = OrderTypes.Limit
			};

			_connectorService.Connector.RegisterOrder(order);
		}

		private void Connector_OnNewOrder(Order order) => Console.WriteLine($"New Order: {order.Security.Code}");
		private void Connector_OnOrderChanged(Order order) => Console.WriteLine($"Order Changed: {order.Security.Code}");
		private void Connector_OnOrderRegisterFailed(Order order, Exception error) => Console.WriteLine($"Order Failed: {error.Message}");
	}

}
