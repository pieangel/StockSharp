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
using Ecng.Xaml;
using Ecng.Logging;
using Ecng.Common;

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
using SciTrader.Services;
using StockSharp.Localization;
using StockSharp.Xaml.GridControl;

namespace SciTrader
{
    public partial class MainWindow : ThemedWindow {
		private Connector _connector;
		public Connector Connector => _connector;
		private bool _isConnected;
		public static MainWindow Instance { get; private set; }

		private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
		{
			CreateConnector();
			InitConnector();
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

				//MessageBox.Show("Download completed successfully!", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
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

				EventBus.Instance.PublishMainWindow(this);
				
			}
			catch (Exception ex)
			{
				MessageBox.Show($"XAML error: {ex.Message}");
			}
		}

		private void CreateConnector()
		{
			string path = ConfigService.Instance.DefaultDataPath;
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

			ConnectorService.Instance.SetConnector(_connector);
			EventBus.Instance.PublishConnector(ConnectorService.Instance.GetConnector());
		}

		private void ChangeConnectStatus(bool isConnected)
		{
			_isConnected = isConnected;
			//ConnectBtn.Content = isConnected ? LocalizedStrings.Disconnect : LocalizedStrings.Connect;
		}

		private void InitConnector()
		{
			Connector.Connected += () =>
			{
				this.GuiAsync(() => ChangeConnectStatus(true));

				if (Connector.Adapter.IsMarketDataTypeSupported(DataType.News) && !Connector.Adapter.IsSecurityNewsOnly)
				{
					if (Connector.Subscriptions.All(s => s.DataType != DataType.News))
						Connector.SubscribeNews();
				}
			};

			// subscribe on connection error event
			Connector.ConnectionError += error => this.GuiAsync(() =>
			{
				ChangeConnectStatus(false);
				MessageBox.Show(this.GetWindow(), error.ToString(), LocalizedStrings.ErrorConnection);
			});

			Connector.Disconnected += () => this.GuiAsync(() => ChangeConnectStatus(false));

			Connector.ConnectionLost += a => Connector.AddErrorLog(LocalizedStrings.ConnectionLost); ;
			Connector.ConnectionRestored += a => Connector.AddInfoLog(LocalizedStrings.ConnectionRestored); ;

			// subscribe on error event
			//Connector.Error += error =>
			//	this.GuiAsync(() => MessageBox.Show(this, error.ToString(), LocalizedStrings.DataProcessError));

			// subscribe on error of market data subscription event
			Connector.SubscriptionFailed += (sub, error, isSubscribe) =>
				this.GuiAsync(() => MessageBox.Show(this.GetWindow(), error.ToString().Truncate(300), LocalizedStrings.ErrorSubDetails.Put(sub.DataType, sub.SecurityId)));

			//Connector.SecurityReceived += (s, sec) => _securitiesWindow.SecurityPicker.Securities.Add(sec);
			//Connector.TickTradeReceived += (s, t) => _tradesWindow.TradeGrid.Trades.Add(t);
			//Connector.OrderLogReceived += (s, ol) => _orderLogWindow.OrderLogGrid.LogItems.Add(ol);
			//Connector.Level1Received += (s, l) => _level1Window.Level1Grid.Messages.Add(l);

			Connector.NewOrder += Connector_OnNewOrder;
			Connector.OrderChanged += Connector_OnOrderChanged;
			Connector.OrderEdited += Connector_OnOrderEdited;

			//Connector.NewMyTrade += _myTradesWindow.TradeGrid.Trades.Add;

			//Connector.PositionReceived += (sub, p) => _portfoliosWindow.PortfolioGrid.Positions.TryAdd(p);

			// subscribe on error of order registration event
			Connector.OrderRegisterFailed += Connector_OnOrderRegisterFailed;
			// subscribe on error of order cancelling event
			Connector.OrderCancelFailed += Connector_OnOrderCancelFailed;
			// subscribe on error of order edition event
			Connector.OrderEditFailed += Connector_OnOrderEditFailed;

			// set market data provider
			//_securitiesWindow.SecurityPicker.MarketDataProvider = Connector;

			// set news provider
			//_newsWindow.NewsPanel.NewsProvider = Connector;

			Connector.LookupTimeFramesResult += (message, timeFrames, error) =>
			{
				//if (error == null)
				//	this.GuiAsync(() => _securitiesWindow.UpdateTimeFrames(timeFrames));
			};

			var nativeIdStorage = ServicesRegistry.TryNativeIdStorage;

			if (nativeIdStorage != null)
			{
				Connector.Adapter.NativeIdStorage = nativeIdStorage;

				try
				{
					nativeIdStorage.Init();
				}
				catch (Exception ex)
				{
					MessageBox.Show(this.GetWindow(), ex.ToString());
				}
			}

			if (Connector.StorageAdapter != null)
			{
				LoggingHelper.DoWithLog(ServicesRegistry.EntityRegistry.Init);
				LoggingHelper.DoWithLog(ServicesRegistry.ExchangeInfoProvider.Init);

				//Connector.Adapter.StorageSettings.DaysLoad = TimeSpan.FromDays(3);
				Connector.Adapter.StorageSettings.Mode = StorageModes.Snapshot;
				Connector.LookupAll();

				Connector.SnapshotRegistry.Init();
			}

			ConfigManager.RegisterService<IMessageAdapterProvider>(new InMemoryMessageAdapterProvider(Connector.Adapter.InnerAdapters));

			// for show mini chart in SecurityGrid
			//_securitiesWindow.SecurityPicker.PriceChartDataProvider = new PriceChartDataProvider(Connector);
			string settingsFile = ConfigService.Instance.SettingsFile;
			try
			{
				if (settingsFile.IsConfigExists())
				{
					var ctx = new ContinueOnExceptionContext();
					ctx.Error += ex => ex.LogError();

					using (ctx.ToScope())
						Connector.LoadIfNotNull(settingsFile.Deserialize<SettingsStorage>());
				}
			}
			catch
			{
				// ignore
			}
		}

		private void Connector_OnNewOrder(Order order)
		{
			//_ordersWindow.OrderGrid.Orders.Add(order);
			//_securitiesWindow.ProcessOrder(order);
		}

		private void Connector_OnOrderChanged(Order order)
		{
			//_securitiesWindow.ProcessOrder(order);
		}

		private void Connector_OnOrderEdited(long transactionId, Order order)
		{
			//_securitiesWindow.ProcessOrder(order);
		}

		private void Connector_OnOrderRegisterFailed(OrderFail fail)
		{
			//_ordersWindow.OrderGrid.AddRegistrationFail(fail);
			//_securitiesWindow.ProcessOrderFail(fail);
		}

		private void Connector_OnOrderEditFailed(long transactionId, OrderFail fail)
		{
			//_securitiesWindow.ProcessOrderFail(fail);
		}

		private void Connector_OnOrderCancelFailed(OrderFail fail)
		{
			//_securitiesWindow.ProcessOrderFail(fail);

			this.GuiAsync(() =>
			{
				MessageBox.Show(this.GetWindow(), fail.Error.ToString(), LocalizedStrings.OrderError);
			});
		}

	}
}
