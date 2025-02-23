using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.Native;
using DevExpress.Mvvm.POCO;
using DevExpress.Utils;
using DevExpress.Utils.About;
using DevExpress.Xpf;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Bars.Native;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.DemoBase.Helpers;
using DevExpress.Xpf.DemoBase.Helpers.TextColorizer;
using DevExpress.Xpf.Docking;
using DevExpress.Xpf.PropertyGrid;

using Ecng.Serialization;
using Ecng.Configuration;

using StockSharp.Configuration;
using StockSharp.Messages;
using StockSharp.Algo;
using StockSharp.BusinessEntities;
using StockSharp.Xaml;
using System.Threading;
using System.Security;
using SciTrader.Network;
using System.Threading.Tasks;
using DevExpress.Xpf.DemoCenterBase;

using StockSharp.Algo.Storages;
using StockSharp.Algo.Storages.Csv;
using StockSharp.SignalMaster;
using SciTrader.ViewModels;
using System.Reactive.Linq;
using ReactiveUI;

namespace SciTrader
{
    public partial class MainWindow : ThemedWindow {

		private IDisposable _subscription;

		private Connector _connector = new Connector();
		public Connector Connector { get; private set; }
		private const string _connectorFile = "ConnectorFile.json";

		private readonly List<Subscription> _subscriptions = new();
		//private SecurityId? _selectedSecurityId;
		
		private SignalMasterMessageAdapter signalMasterMessageAdapter;

		public class SciLeanIdGenerator : Ecng.Common.IdGenerator
		{
			private long _currentId;

			public SciLeanIdGenerator()
			{
				_currentId = 1;
			}

			public override long GetNextId()
			{
				return Interlocked.Increment(ref _currentId);
			}
		}
		

		private static SecureString ToSecureString(string str)
		{
			var secureString = new SecureString();
			foreach (char c in str)
			{
				secureString.AppendChar(c);
			}
			secureString.MakeReadOnly();
			return secureString;
		}

		private void InitConnect()
		{
			// registering all connectors
			ConfigManager.RegisterService<IMessageAdapterProvider>(new InMemoryMessageAdapterProvider(_connector.Adapter.InnerAdapters));

			if (File.Exists(_connectorFile))
			{
				_connector.Load(_connectorFile.Deserialize<SettingsStorage>());
			}
		}
		
		private void InitSciLeanMessageAdapter()
		{
			signalMasterMessageAdapter = new SignalMasterMessageAdapter(new SciLeanIdGenerator());
			var apiKey = ToSecureString("angelpie"); // Replace with your actual API key
			var apiSecret = ToSecureString("orion"); // Replace with your actual API secret

			signalMasterMessageAdapter.Key = apiKey;
			signalMasterMessageAdapter.Secret = apiSecret;

			// Add the Coinbase adapter to the connector
			_connector.Adapter.InnerAdapters.Add(signalMasterMessageAdapter);
		}
		
		public static MainWindow Instance { get; private set; }
		private Connector MainPanel_OnCreateConnector(string path)
		{
			//HistoryPath.Folder = path;

			var entityRegistry = new CsvEntityRegistry(path);

			var exchangeInfoProvider = new StorageExchangeInfoProvider(entityRegistry, false);
			ConfigManager.RegisterService<IExchangeInfoProvider>(exchangeInfoProvider);
			ConfigManager.RegisterService<IBoardMessageProvider>(exchangeInfoProvider);

			var storageRegistry = new StorageRegistry(exchangeInfoProvider)
			{
				DefaultDrive = new LocalMarketDataDrive(path)
			};

			ConfigManager.RegisterService<IEntityRegistry>(entityRegistry);
			ConfigManager.RegisterService<IStorageRegistry>(storageRegistry);

			INativeIdStorage nativeIdStorage = new CsvNativeIdStorage(Path.Combine(path, "NativeId"))
			{
				DelayAction = entityRegistry.DelayAction
			};
			ConfigManager.RegisterService(nativeIdStorage);

			var snapshotRegistry = new SnapshotRegistry(Path.Combine(path, "Snapshots"));

			_connector = new Connector(entityRegistry.Securities, entityRegistry.PositionStorage, exchangeInfoProvider, storageRegistry, snapshotRegistry, new StorageBuffer());
			return _connector;
		}
		static void ShowSplashScreen() {
            var viewModel = new DXSplashScreenViewModel() {
                Title = "Visual Studio Inspired UI Demo",
                Subtitle = "Powered by DevExpress",
                Logo = new Uri(string.Format(@"pack://application:,,,/DevExpress.Xpf.DemoBase.v{0};component/DemoLauncher/Images/Logo.svg", AssemblyInfo.VersionShort))
            };
            //SplashScreenManager.Create(() => new DockingSplashScreenWindow(), viewModel).ShowOnStartup();
        }

		protected override void OnContentRendered(EventArgs e)
		{
			base.OnContentRendered(e);
			//MessageBox.Show("MainWindow Loaded!");
			DownloadButton_Click(this, null);
		}

		private async void DownloadButton_Click(object sender, RoutedEventArgs e)
		{
			string serverUrl = "http://localhost:9000";
			string saveDirectory = ".\\mst";

			FileDownloader downloader = new FileDownloader(serverUrl, saveDirectory);

			// Show loading message before starting
			LoadingMessage.Visibility = Visibility.Visible;

			await downloader.DownloadAllFilesAsync(async () =>
			{
				var symbolManager = ((App)Application.Current).SymbolManager;

				await Task.Run(() =>
				{
					symbolManager.ReadForeignMarketFile("mst", "MRKT.cod");
					symbolManager.ReadForeignSymbolFile("mst", "JMCODE.cod");
					symbolManager.ReadHomeProductFile("mst", "dm_product.cod");
					symbolManager.ReadHomeSymbolMasterFile("mst", "chocode.cod");
				});

				// Hide loading message after completion
				LoadingMessage.Visibility = Visibility.Collapsed;

				MessageBox.Show("Download completed successfully!", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
			});
		}


		public MainWindow() {
			Instance = this;
			try
			{
				ApplicationThemeHelper.ApplicationThemeName = Theme.Office2019Colorful.Name;
				ShowSplashScreen();
				//DemoRunner.SubscribeThemeChanging();
				Theme.CachePaletteThemes = true;
				Theme.RegisterPredefinedPaletteThemes();
				InitializeComponent();

				InitSciLeanMessageAdapter();
				InitConnect();
			}
			catch (Exception ex)
			{
				MessageBox.Show($"XAML error: {ex.Message}");
			}
			// ✅ Send MainWindow instance to ViewModel via Messenger
			Messenger.Default.Send(this, "MainWindowMessage");

			var viewModel = new MainViewModel();
			DataContext = viewModel;

			// ✅ Subscribe to ViewModel's observable
			_subscription = viewModel.ViewRequests
				.ObserveOn(RxApp.MainThreadScheduler) // ✅ FIX: Ensures UI updates on the main thread
				.Subscribe(OnViewRequestReceived);

		}

		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);
			_subscription?.Dispose(); // ✅ Dispose Rx.NET subscription to prevent memory leaks
		}

		// ✅ Handle received requests from ViewModel
		private void OnViewRequestReceived(string request)
		{
			if (request == "ShowPopup")
			{
				MessageBox.Show("This is a popup from ViewModel (Rx.NET)!");
			}
		}

		private void Setting_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (_connector.Configure(this))
				{
					_connector.Save().Serialize(_connectorFile);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Error: {ex.Message}");
			}
		}
	}
}
