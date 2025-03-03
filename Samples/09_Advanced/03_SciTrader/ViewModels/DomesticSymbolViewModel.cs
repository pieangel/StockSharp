using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Mvvm;
using SciTrader.Data;
using SciTrader.Model;

using DevExpress.Data;
using DevExpress.Mvvm.DataAnnotations;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Threading;
using DevExpress.Mvvm.POCO;
using System.Reactive.Linq;
using SciTrader.Services;
using StockSharp.Algo;
using StockSharp.BusinessEntities;

namespace SciTrader.ViewModels
{
	public enum RadioButtonSelection
	{
		MarketPrice,
		ExpectedPrice,
		Balance
	}

	public class DomesticSymbolViewModel : PanelWorkspaceViewModel
	{

		private Connector _connector;
		private readonly ConnectorService _connectorService;
		private IDisposable _securitySubscription;


		private RadioButtonSelection _selectedRadioButton;
		public RadioButtonSelection SelectedRadioButton
		{
			get { return _selectedRadioButton; }
			set
			{
				if (_selectedRadioButton != value)
				{
					_selectedRadioButton = value;
					//RaisePropertyChanged(nameof(SelectedRadioButton));
					OnRadioButtonSelectionChanged();
				}
			}
		}


		private void OnRadioButtonSelectionChanged()
		{
			// Handle the radio button selection change here
			switch (SelectedRadioButton)
			{
				case RadioButtonSelection.MarketPrice:
					// Handle Market Price selection
					break;
				case RadioButtonSelection.ExpectedPrice:
					// Handle Expected Price selection
					break;
				case RadioButtonSelection.Balance:
					// Handle Balance selection
					break;
			}
		}
		public static DomesticSymbolViewModel Create(List<OptionItem> optionItems)
		{
			return ViewModelSource.Create(() => new DomesticSymbolViewModel(optionItems));
		}
		protected override string WorkspaceName => "Toolbox";

		private Dictionary<string, OptionItem> optionItemsDict;

		public DomesticSymbolViewModel(List<OptionItem> a_optionItems)
		{
			// Initialize the selected radio button
			SelectedRadioButton = RadioButtonSelection.MarketPrice;

			DisplayName = "국내시장";
			Glyph = Images.Toolbox;

			optionItems = a_optionItems;
			optionItemsDict = new Dictionary<string, OptionItem>();

			//futureItems = new List<FutureItem>();
			futureItemsDict = new Dictionary<string, FutureItem>();

			// Add sample data
			LoadData();
		}

		private List<OptionItem> optionItems = new List<OptionItem>();
		private List<FutureItem> futureItems = new List<FutureItem>();

		public DomesticSymbolViewModel()
		{
			_connectorService = ConnectorService.Instance;
			DisplayName = "국내시장";
			Glyph = Images.Toolbox;

			//OptionItems = new List<OptionItem>();
			optionItemsDict = new Dictionary<string, OptionItem>();

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
			AddOrUpdateOption(new OptionItem { Strike = "100", CallPrice = 1.2, PutPrice = 0.8, OptionSymbolCode = "1" });
			AddOrUpdateOption(new OptionItem { Strike = "110", CallPrice = 1.5, PutPrice = 1.1, OptionSymbolCode = "2" });

			AddOrUpdateFuture(new FutureItem { FutureSymbolCode = "3", ShortSymbolCode = "SC1", FutureName = "Future 1", Price = 100 });
			AddOrUpdateFuture(new FutureItem { FutureSymbolCode = "4", ShortSymbolCode = "SC2", FutureName = "Future 2", Price = 110 });

			//this.RaisePropertyChanged(nameof(FutureItems)); // Notify UI
			//this.RaisePropertyChanged(nameof(OptionItems)); // Notify UI
		}

		public void AddOrUpdateOption(OptionItem newItem)
		{
			if (optionItemsDict.TryGetValue(newItem.OptionSymbolCode, out var existingItem))
			{
				// Update existing item (UI will auto-refresh)
				existingItem.CallPrice = newItem.CallPrice;
				existingItem.PutPrice = newItem.PutPrice;
			}
			else
			{
				// Add new item
				optionItems.Add(newItem);
				optionItemsDict[newItem.OptionSymbolCode] = newItem;
			}
		}

		public List<FutureItem> FutureItems { get { return futureItems; } }
		public List<OptionItem> OptionItems { get { return optionItems; } }

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
			"Kospi", "Mini Kospi", "Kospi Weekly M", "Kospi Weekly T", "Kosdaq"
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
