using DevExpress.Mvvm.POCO;
using System.Collections.Generic;
using SciTrader.Data;
using SciTrader.Model;

using StockSharp.BusinessEntities;
using StockSharp.Messages;
using System.Reactive.Linq;
using SciTrader.Services;
using System;
using StockSharp.Algo;
using System.Linq;

namespace SciTrader.ViewModels {
    public class HomeSymbolViewModel : PanelWorkspaceViewModel
    {
		private Connector _connector;
		private readonly ConnectorService _connectorService;
		private IDisposable _securitySubscription;

		public static HomeSymbolViewModel Create(List<OrderHistoryData> orders) {
            return ViewModelSource.Create(() => new HomeSymbolViewModel(orders));
        }

        readonly List<OrderHistoryData> ordersDataSource;

        public List<OrderHistoryData> OrdersDataSource { get { return ordersDataSource; } }

        public HomeSymbolViewModel(List<OrderHistoryData> orders) {
            ordersDataSource = orders;
        }

        public HomeSymbolViewModel()
        {
			_connectorService = ConnectorService.Instance;
			DisplayName = "Order History";
            Glyph = Images.Output;

			optionItemsDict = new Dictionary<string, OptionItem>();

			futureItemsDict = new Dictionary<string, FutureItem>();

			LoadData();

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

		private List<OptionItem> optionItems = new List<OptionItem>();
		private List<FutureItem> futureItems = new List<FutureItem>();
		private Dictionary<string, FutureItem> futureItemsDict;
		private Dictionary<string, OptionItem> optionItemsDict;

		public List<FutureItem> FutureItems { get { return futureItems; } }
		public List<OptionItem> OptionItems { get { return optionItems; } }

		private void LoadData()
		{
			AddOrUpdateOption(new OptionItem { Strike = "100", CallPrice = 1.2, PutPrice = 0.8, OptionSymbolCode = "1" });
			AddOrUpdateOption(new OptionItem { Strike = "110", CallPrice = 1.5, PutPrice = 1.1, OptionSymbolCode = "2" });

			AddOrUpdateFuture(new FutureItem { FutureSymbolCode = "3", ShortSymbolCode = "SC1", FutureName = "Future 1", Price = 100 });
			AddOrUpdateFuture(new FutureItem { FutureSymbolCode = "4", ShortSymbolCode = "SC2", FutureName = "Future 2", Price = 110 });

			//this.RaisePropertyChanged(nameof(FutureItems)); // Notify UI
			//this.RaisePropertyChanged(nameof(OptionItems)); // Notify UI
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

		protected override string WorkspaceName { get { return "Toolbox"; } }
    }
}
