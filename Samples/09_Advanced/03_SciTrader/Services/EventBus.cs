using StockSharp.Algo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Reactive.Linq;

namespace SciTrader.Services
{
	public class EventBus
	{
		public static EventBus Instance { get; } = new EventBus();

		private readonly Subject<Window> _mainWindowSubject = new();
		private readonly Subject<Connector> _connectorSubject = new();

		public IObservable<Window> MainWindowObservable => _mainWindowSubject.AsObservable();
		public IObservable<Connector> ConnectorObservable => _connectorSubject.AsObservable();

		public void PublishMainWindow(Window window) => _mainWindowSubject.OnNext(window);
		public void PublishConnector(Connector connector) => _connectorSubject.OnNext(connector);
	}
}
