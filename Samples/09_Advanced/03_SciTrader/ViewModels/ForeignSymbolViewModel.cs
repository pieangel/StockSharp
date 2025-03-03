using DevExpress.Utils;
using DevExpress.Xpf.DemoBase.Helpers.TextColorizer;
using DevExpress.Xpf.DemoBase.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using DevExpress.Mvvm;
using SciTrader.Data;
using SciTrader.Model;
using System.Windows.Threading;
using DevExpress.Mvvm.DataAnnotations;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive.Linq;
using SciTrader.Services;
using StockSharp.Algo;
using StockSharp.BusinessEntities;

namespace SciTrader.ViewModels
{
	public class ForeignSymbolViewModel : PanelWorkspaceViewModel
	{

		private Connector _connector;
		private readonly ConnectorService _connectorService;
		private IDisposable _securitySubscription;

		protected override string WorkspaceName => "Toolbox";

	

		private List<FutureItem> futureItems = new List<FutureItem>();

		public ForeignSymbolViewModel()
		{
			_connectorService = ConnectorService.Instance;

			DisplayName = "해외시장";
			Glyph = Images.Toolbox;

			//FutureItems = new List<FutureItem>();
			futureItemsDict = new Dictionary<string, FutureItem>();

			// Add sample data
			LoadData();

			// Set the first item as default when the app starts
			if (MarketOptions.Any())
				SelectedMarket = MarketOptions.First();

			if (MonthOptions.Any())
				SelectedMonth = MonthOptions.First();

			EventBus.Instance.ConnectorObservable
				.Subscribe(connector =>
				{
					_connector = connector;
					// ✅ Subscribe to security updates
					_securitySubscription = _connectorService.SecurityStream
						//.ObserveOn(RxApp.MainThreadScheduler) // Ensure updates happen on the UI thread
						.Subscribe(OnSecurityReceived);
				});
		}

		private void OnSecurityReceived(Security security)
		{
			if (!futureItemsDict.ContainsKey(security.Name))
			{
				AddOrUpdateFuture(new FutureItem
				{
					FutureSymbolCode = security.Code,
					ShortSymbolCode = security.Code,
					FutureName = security.Name,
					Price = 100
				});
			}
		}

		private void LoadData()
		{

			AddOrUpdateFuture(new FutureItem { FutureSymbolCode = "3", ShortSymbolCode = "SC1", FutureName = "Future 1", Price = 100 });
			AddOrUpdateFuture(new FutureItem { FutureSymbolCode = "4", ShortSymbolCode = "SC2", FutureName = "Future 2", Price = 110 });

			//this.RaisePropertyChanged(nameof(FutureItems)); // Notify UI
			//this.RaisePropertyChanged(nameof(OptionItems)); // Notify UI
		}

		public List<FutureItem> FutureItems { get { return futureItems; } }

		private Dictionary<string, FutureItem> futureItemsDict;



		public void AddOrUpdateFuture(FutureItem newItem)
		{
			if (futureItemsDict.TryGetValue(newItem.FutureSymbolCode, out var existingItem))
			{
				// Update existing item (UI will auto-refresh)
				existingItem.Price = newItem.Price;
			}
			else
			{
				// Add new item
				FutureItems.Add(newItem);
				futureItemsDict[newItem.FutureSymbolCode] = newItem;
			}
		}

		private string _selectedMarket;
		public string SelectedMarket
		{
			get => _selectedMarket;
			set
			{
				if (_selectedMarket != value)
				{
					_selectedMarket = value;
					//RaisePropertyChanged(nameof(SelectedMarket));
					OnMarketSelectionChanged();
				}
			}
		}

		private string _selectedMonth;
		public string SelectedMonth
		{
			get => _selectedMonth;
			set
			{
				if (_selectedMonth != value)
				{
					_selectedMonth = value;
					//RaisePropertyChanged(nameof(SelectedMonth));
					OnMonthSelectionChanged();
				}
			}
		}

		private void OnMarketSelectionChanged()
		{
			Debug.WriteLine($"Market selected: {SelectedMarket}");
			// Add logic for market selection change if needed
		}

		private void OnMonthSelectionChanged()
		{
			Debug.WriteLine($"Month selected: {SelectedMonth}");
			// Add logic for month selection change if needed
		}

		// Define available items for the ComboBoxes
		public ObservableCollection<string> MarketOptions { get; set; } = new ObservableCollection<string>
		{
			"지수", "통화", "금리", "농축산", "귀금속", "에너지", "비철금속"
		};

		public ObservableCollection<string> MonthOptions { get; set; } = new ObservableCollection<string>
		{
			"Option A", "Option B"
		};

		public void UpdateMonthOptions()
		{
			MonthOptions.Clear(); // Clear existing items

			// Add new items dynamically (this could be loaded from a database, API, etc.)
			MonthOptions.Add("Option X");
			MonthOptions.Add("Option Y");
			MonthOptions.Add("Option Z");

			Debug.WriteLine("MonthOptions updated dynamically.");
		}

	}
}
