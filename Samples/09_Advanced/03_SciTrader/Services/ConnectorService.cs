using StockSharp.Algo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockSharp.BusinessEntities;
using System.Reactive.Subjects;
using StockSharp.Messages;


namespace SciTrader.Services
{
	public class ConnectorService
	{
		private static readonly Lazy<ConnectorService> _instance =
			new(() => new ConnectorService());

		public static ConnectorService Instance => _instance.Value;

		private Connector _connector;
		private readonly Subject<Security> _securitySubject = new();

		private readonly Subject<Connector> _connectorSubject = new(); // ✅ Notify subscribers

		public void SetConnector(Connector connector)
		{
			if (_connector != null)
			{
				_connector.SecurityReceived -= OnSecurityReceived; // Unsubscribe old connector
			}

			_connector = connector;

			if (_connector != null)
			{
				_connector.SecurityReceived += OnSecurityReceived; // Subscribe to new connector
				_connectorSubject.OnNext(_connector); // ✅ Notify subscribers
			}
		}

		// ✅ Observable stream for Connector updates
		public IObservable<Connector> ConnectorStream => _connectorSubject;

		public Connector GetConnector() => _connector;

		// ✅ Observable stream for Security updates
		public IObservable<Security> SecurityStream => _securitySubject;

		private ConnectorService() { }

		/// <summary>
		/// Handles SecurityReceived event and pushes it to Rx stream
		/// </summary>
		private void OnSecurityReceived(Subscription subscription, Security security)
		{
			if (security != null)
			{
				_securitySubject.OnNext(security); // ✅ Push security updates to subscribers
			}
		}

		public void SubscribeToTickCandles(Security security, int tickCycle = 200)
		{
			if (_connector == null)
				throw new InvalidOperationException("Connector is not set.");

			var mdMsg = new MarketDataMessage
			{
				IsSubscribe = true,
				DataType2 = DataType.Create(typeof(TickCandleMessage), tickCycle), // ✅ Tick-based candles with 200-tick cycle
				From = DateTime.UtcNow.AddHours(-2), // ✅ Historical data
				To = DateTime.UtcNow,
				BuildMode = MarketDataBuildModes.LoadAndBuild, // ✅ Load history + Build new candles
				Skip = 0,
				Count = 3200,
				IsFinishedOnly = true
			};

			Subscription _subscription = _connector.SubscribeMarketData(mdMsg);
		}

		// ✅ Subscribe to candle updates
		public void SubscribeToCandles(Security security, TimeSpan timespan)
		{
			if (_connector == null)
				throw new InvalidOperationException("Connector is not set.");

			Console.WriteLine($"Security: {security.Id}, Board: {security.Board?.Code ?? "NULL"}");

			var mdMsg = new MarketDataMessage
			{
				BoardCode = security.Board.Code,
				IsSubscribe = true,
				// 이부분 너무 중요함. 반드시 이 부분을 명심할 것. 
				DataType2 = DataType.Create(typeof(TimeFrameCandleMessage), timespan), // ✅ Correctly setting the TimeFrame
				From = DateTime.UtcNow.AddHours(-2),
				To = DateTime.UtcNow,
				BuildMode = MarketDataBuildModes.LoadAndBuild, // ✅ Load history + Build new candles
				Skip = 0,
				Count = 3200,
				IsFinishedOnly = true
			};
			// 이 부분 너무 중요함. security 정보를 MargetDataMessage에 넣어줘야 함.
			security.ToMessage().CopyTo(mdMsg);
			var tf = mdMsg.GetTimeFrame();

			Subscription _subscription = _connector.SubscribeMarketData(mdMsg);
		}
	}
}
