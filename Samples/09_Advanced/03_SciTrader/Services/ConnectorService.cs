using StockSharp.Algo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockSharp.BusinessEntities;
using System.Reactive.Subjects;

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
	}
}
