using SciTrader.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Reactive.Linq;
using StockSharp.Algo;
using System.Reactive.Subjects;
using ReactiveUI;
using SciTrader.Services;
using StockSharp.BusinessEntities;
using StockSharp.Algo.Indicators;

namespace SciTrader.Views
{
    /// <summary>
    /// Interaction logic for SecurityView.xaml
    /// </summary>
    public partial class SecurityView : UserControl
    {
		private SecuritiesWindow _securitiesWindow;
		private IDisposable _connectorSubscription;
		private IDisposable _securitySubscription;
		public event Action<Security> SecuritySelected; // ✅ Custom event to notify parent components
		public SecurityView()
        {
			InitializeComponent();

			// Create an instance of the StockSharp window (e.g., SecuritiesWindow)
			_securitiesWindow = new SecuritiesWindow();

			// Extract the content from the StockSharp window
			var windowContent = _securitiesWindow.Content as UIElement;

			if (windowContent != null)
			{
				// Remove the window content and assign it to the UserControl
				_securitiesWindow.Content = null;
				RootGrid.Children.Add(windowContent);
			}

			// Close the original window since we are reusing its content
			_securitiesWindow.Close();

			DataContext = new SecurityViewModel(); // Ensure this is set

			

			// ✅ Subscribe to Connector updates from ConnectorService
			_connectorSubscription = ConnectorService.Instance.ConnectorStream
				.ObserveOn(RxApp.MainThreadScheduler) // Ensure UI updates on main thread
				.Subscribe(OnConnectorUpdated);

			EventBus.Instance.ConnectorObservable
				.Subscribe(connector =>
				{
					// ✅ Subscribe to security updates
					_securitySubscription = ConnectorService.Instance.SecurityStream
						//.ObserveOn(RxApp.MainThreadScheduler) // Ensure updates happen on the UI thread
						.Subscribe(OnSecurityReceived);

					//_securityWindow.
				});
		}

		private void OnConnectorUpdated(Connector newConnector)
		{
			if (_securitiesWindow.SecurityPicker != null) // ✅ Ensure SecurityPicker is available
			{
				_securitiesWindow.SecurityPicker.MarketDataProvider = newConnector;

				_securitiesWindow.SecurityPicker.SecuritySelected += OnSecuritySelected; // ✅ Subscribe to the event
			}
		}

		private void OnSecuritySelected(Security security)
		{
			SecuritySelected?.Invoke(security); // ✅ Raise the event
			System.Diagnostics.Debug.WriteLine($"Security Selected: {security.Code}");
		}

		private void OnSecurityReceived(Security security)
		{
			if (_securitiesWindow.SecurityPicker != null)
				_securitiesWindow.SecurityPicker.Securities.Add(security);
		}

		public void Dispose()
		{
			_connectorSubscription?.Dispose(); // ✅ Prevent memory leaks
		}
	}
}
