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

using Ecng.Common;
using Ecng.Configuration;
using Ecng.Serialization;
using Ecng.Collections;
using Ecng.Logging;

using Nito.AsyncEx;

using StockSharp.Configuration;
using StockSharp.Messages;
using StockSharp.Algo;
using StockSharp.BusinessEntities;
using StockSharp.Xaml;
using System.Threading;
using System.Security;
using SciTrader.Network;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using System.Reactive.Subjects;
using System.Reactive.Linq;
using StockSharp.Localization;
using StockSharp.Algo.Storages;


namespace SciTrader.ViewModels {
    public class MainViewModel
	{

		// ✅ Rx.NET Subject for event-driven communication
		private readonly Subject<EventData<object>> _eventSubject = new();
		public IObservable<EventData<object>> EventObservable => _eventSubject.AsObservable();

		public Connector Connector { get; private set; }
        private Connector _connector;
		private const string _connectorFile = "ConnectorFile.json";

		// ✅ Rx.NET Subject that the View will observe
		private readonly Subject<string> _viewRequestSubject = new();

		// ✅ Expose as an Observable (Only View can listen, but cannot push data)
		public IObservable<string> ViewRequests => _viewRequestSubject.AsObservable();

		private readonly List<Subscription> _subscriptions = new();
		//private SecurityId? _selectedSecurityId;

		/*
		private SciLeanMessageAdapter slMessageAdapter;

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
        */


		private bool _isConnected;
		private Window _mainWindow;

		public ObservableCollection<Security> Securities { get; } = new();
		public ObservableCollection<Order> Orders { get; } = new();
		public ObservableCollection<ITickTradeMessage> Trades { get; } = new();

		private string _connectButtonLabel = "Connect";

		public string ConnectButtonLabel
		{
			get => _connectButtonLabel;
			set
			{
				_connectButtonLabel = value;
				OnPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public const int Tick = 500;
        CommandViewModel errorList;
        PanelWorkspaceViewModel lastOpenedItem;
        CommandViewModel loadLayout;
        CommandViewModel newFile;
        CommandViewModel newProject;
        CommandViewModel openFile;
        CommandViewModel openProject;
        CommandViewModel output;
        CommandViewModel properties;
        CommandViewModel save;
        //CommandViewModel ConnectCommandViewModel;
        CommandViewModel saveAll;
        CommandViewModel saveLayout;
        CommandViewModel searchResults;
        CommandViewModel solutionExplorer;
		//CommandViewModel settingCommandViewModel;

		public CommandViewModel ConnectCommandViewModel { get; private set; }
		public CommandViewModel SettingsCommandViewModel { get; private set; }

		SolutionExplorerViewModel solutionExplorerViewModel;
        CommandViewModel toolbox;
        ObservableCollection<WorkspaceViewModel> workspaces;

        public MainViewModel() {
            ErrorListViewModel = CreatePanelWorkspaceViewModel<ErrorListViewModel>();
            OutputViewModel = CreatePanelWorkspaceViewModel<OutputViewModel>();
            PropertiesViewModel = CreatePanelWorkspaceViewModel<PropertiesViewModel>();
            SearchResultsViewModel = CreatePanelWorkspaceViewModel<SearchResultsViewModel>();
            ToolboxViewModel = CreatePanelWorkspaceViewModel<ToolboxViewModel>();
            OpenOrdersViewModel = CreatePanelWorkspaceViewModel<OpenOrdersViewModel>();
            HomeSymbolViewModel = CreatePanelWorkspaceViewModel<HomeSymbolViewModel>();
			DomesticSymbolViewModel = CreatePanelWorkspaceViewModel<DomesticSymbolViewModel>();
			ForeignSymbolViewModel = CreatePanelWorkspaceViewModel<ForeignSymbolViewModel>();
			SecuritiesViewModel = CreatePanelWorkspaceViewModel<SecuritiesViewModel>();
			Bars = new ReadOnlyCollection<BarModel>(CreateBars());
            InitDefaultLayout();
			//InitSciLeanMessageAdapter();
			//InitConnect();

			// ✅ Register Messenger to receive MainWindow instance
			Messenger.Default.Register<Window>(this, "MainWindowMessage", window =>
			{
				_mainWindow = window;
				_connector = new Connector(); // ✅ Now we can pass MainWindow
			});


			
		}

		private readonly string _defaultDataPath = "Data";
		private readonly string _settingsFile;

		private readonly Subject<bool> _connectionStatusSubject = new();
		public IObservable<bool> ConnectionStatusObservable => _connectionStatusSubject.AsObservable();

		private readonly Subject<SubscriptionErrorEvent> _subscriptionErrorSubject = new();
		public IObservable<SubscriptionErrorEvent> SubscriptionErrorObservable => _subscriptionErrorSubject.AsObservable();

		// ✅ Rx.NET Subject for Security Events
		private readonly Subject<Security> _securityReceivedSubject = new();

		public IObservable<Security> SecurityReceivedObservable => _securityReceivedSubject.AsObservable();

		// ✅ Rx.NET Subject for TimeFrames
		private readonly Subject<List<TimeSpan>> _timeFramesSubject = new();

		public IObservable<List<TimeSpan>> TimeFramesObservable => _timeFramesSubject.AsObservable();

		private void InitConnector()
        {
			Connector.Connected += () =>
			{
				_connectionStatusSubject.OnNext(true); // 🔴 Notify the View about connection status

				if (Connector.Adapter.IsMarketDataTypeSupported(DataType.News) && !Connector.Adapter.IsSecurityNewsOnly)
				{
					if (Connector.Subscriptions.All(s => s.DataType != DataType.News))
						Connector.SubscribeNews();
				}
			};

			Connector.ConnectionError += error =>
			{
				_connectionStatusSubject.OnNext(false); // 🔴 Notify View about error
			};

			Connector.Disconnected += () =>
			{
				_connectionStatusSubject.OnNext(false); // 🔴 Notify View about disconnection
			};

			Connector.ConnectionLost += a => Connector.AddErrorLog(LocalizedStrings.ConnectionLost); ;
            Connector.ConnectionRestored += a => Connector.AddInfoLog(LocalizedStrings.ConnectionRestored); ;

			// subscribe on error event
			//Connector.Error += error =>
			//	this.GuiAsync(() => MessageBox.Show(this, error.ToString(), LocalizedStrings.DataProcessError));

			// subscribe on error of market data subscription event
			Connector.SubscriptionFailed += (sub, error, isSubscribe) =>
			{
				// 🔴 Notify the View about the error instead of showing MessageBox
				_subscriptionErrorSubject.OnNext(new SubscriptionErrorEvent
				{
					DataType = sub.DataType,
					SecurityId = (SecurityId)sub.SecurityId,
					ErrorMessage = error.ToString().Truncate(300)
				});
			};



			/*
             * What Happens in Runtime?
               The Connector receives new security data from the market.
               The SecurityReceived event is triggered.
               The event handler (s, sec) => _securitiesWindow.SecurityPicker.Securities.Add(sec); executes.
               The security (sec) is added to the UI's SecurityPicker.
            */
			// ✅ Event Subscription
			Connector.SecurityReceived += (s, sec) => _securityReceivedSubject.OnNext(sec);
			/*
			Connector.SecurityReceived += (s, sec) => _securitiesWindow.SecurityPicker.Securities.Add(sec);
            Connector.TickTradeReceived += (s, t) => _tradesWindow.TradeGrid.Trades.Add(t);
            Connector.OrderLogReceived += (s, ol) => _orderLogWindow.OrderLogGrid.LogItems.Add(ol);
            Connector.Level1Received += (s, l) => _level1Window.Level1Grid.Messages.Add(l);

            Connector.NewOrder += Connector_OnNewOrder;
            Connector.OrderChanged += Connector_OnOrderChanged;
            Connector.OrderEdited += Connector_OnOrderEdited;

            Connector.NewMyTrade += _myTradesWindow.TradeGrid.Trades.Add;

            Connector.PositionReceived += (sub, p) => _portfoliosWindow.PortfolioGrid.Positions.TryAdd(p);
            */

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

			// ✅ Event Subscription in ViewModel
			Connector.LookupTimeFramesResult += (message, timeFrames, error) =>
			{
				if (error == null)
					_timeFramesSubject.OnNext(timeFrames.ToList()); // Emit time frames
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
                    MessageBox.Show(_mainWindow, ex.ToString());
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

			try
			{
				if (_settingsFile.IsConfigExists())
				{
					var ctx = new ContinueOnExceptionContext();
					ctx.Error += ex => ex.LogError();

					using (ctx.ToScope())
						Connector.LoadIfNotNull(_settingsFile.Deserialize<SettingsStorage>());
				}
			}
			catch
			{
				// ignore
			}
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

// 			this.GuiAsync(() =>
// 			{
// 				MessageBox.Show(this.GetWindow(), fail.Error.ToString(), LocalizedStrings.OrderError);
// 			});
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
        /*
		private void InitSciLeanMessageAdapter()
		{
			slMessageAdapter = new SciLeanMessageAdapter(new SciLeanIdGenerator());
			var apiKey = ToSecureString("angelpie"); // Replace with your actual API key
			var apiSecret = ToSecureString("orion"); // Replace with your actual API secret

			slMessageAdapter.Key = apiKey;
			slMessageAdapter.Secret = apiSecret;

			// Add the Coinbase adapter to the connector
			_connector.Adapter.InnerAdapters.Add(slMessageAdapter);
		}
        */

		public ReadOnlyCollection<BarModel> Bars { get; private set; }
        public ErrorListViewModel ErrorListViewModel { get; private set; }
        public OutputViewModel OutputViewModel { get; private set; }
        public PropertiesViewModel PropertiesViewModel { get; private set; }
        public SearchResultsViewModel SearchResultsViewModel { get; set; }

        public OpenOrdersViewModel OpenOrdersViewModel { get; private set; }
        public HomeSymbolViewModel HomeSymbolViewModel { get; private set; }

		public DomesticSymbolViewModel DomesticSymbolViewModel { get; private set; }

		public ForeignSymbolViewModel ForeignSymbolViewModel { get; private set; }

        public SecuritiesViewModel SecuritiesViewModel { get; private set; }

		public SolutionExplorerViewModel SolutionExplorerViewModel {
            get {
                if(solutionExplorerViewModel == null) {
                    solutionExplorerViewModel = CreatePanelWorkspaceViewModel<SolutionExplorerViewModel>();
                    solutionExplorerViewModel.ItemOpening += SolutionExplorerViewModel_ItemOpening;
                    solutionExplorerViewModel.Solution = Solution.Create();
                    OpenItem(solutionExplorerViewModel.Solution.LastOpenedItem.FilePath);
                }
                return solutionExplorerViewModel;
            }
        }
        public ToolboxViewModel ToolboxViewModel { get; private set; }
        public ObservableCollection<WorkspaceViewModel> Workspaces {
            get {
                if(workspaces == null) {
                    workspaces = new ObservableCollection<WorkspaceViewModel>();
                    workspaces.CollectionChanged += OnWorkspacesChanged;
                }
                return workspaces;
            }
        }
        protected virtual IDockingSerializationDialogService SaveLoadLayoutService { get { return null; } }

        protected virtual List<CommandViewModel> CreateAboutCommands() {
            var showAboutCommnad = new DelegateCommand(ShowAbout);
            return new List<CommandViewModel>() { new CommandViewModel("About", showAboutCommnad) { Glyph = Images.About } };
        }
        protected virtual List<CommandViewModel> CreateEditCommands() {
            var findCommand = new CommandViewModel("Find") { Glyph = Images.Find, KeyGesture = new KeyGesture(Key.F, ModifierKeys.Control) };
            var replaceCommand = new CommandViewModel("Replace") { Glyph = Images.Replace, KeyGesture = new KeyGesture(Key.H, ModifierKeys.Control) };
            var findInFilesCommand = new CommandViewModel("Find in Files") {
                Glyph = Images.FindInFiles,
                KeyGesture = new KeyGesture(Key.F, ModifierKeys.Control | ModifierKeys.Shift)
            };
            var list = new List<CommandViewModel>() { findCommand, replaceCommand, findInFilesCommand };
            CommandViewModel findReplaceDocument = new CommandViewModel("Find and Replace", list);
            findReplaceDocument.IsEnabled = true;
            findReplaceDocument.IsSubItem = true;
            return new List<CommandViewModel>() { findReplaceDocument };
        }
        protected virtual List<CommandViewModel> CreateLayoutCommands() {
            loadLayout = new CommandViewModel("Load Layout...", new DelegateCommand(OnLoadLayout)) { Glyph = Images.LoadLayout };
            saveLayout = new CommandViewModel("Save Layout...", new DelegateCommand(OnSaveLayout)) { Glyph = Images.SaveLayout };
            return new List<CommandViewModel>() { loadLayout, saveLayout };
        }
        protected T CreatePanelWorkspaceViewModel<T>() where T : PanelWorkspaceViewModel {
            return ViewModelSource<T>.Create();
        }
        protected virtual List<CommandViewModel> CreateViewCommands()
        {
            toolbox = GetShowCommand(ToolboxViewModel);
            solutionExplorer = GetShowCommand(SolutionExplorerViewModel);
            properties = GetShowCommand(PropertiesViewModel);
            errorList = GetShowCommand(ErrorListViewModel);
            output = GetShowCommand(OutputViewModel);
            searchResults = GetShowCommand(SearchResultsViewModel);
            return new List<CommandViewModel>() {
                toolbox,
                solutionExplorer,
                properties,
                errorList,
                output,
                searchResults,
            };
        }
        List<CommandViewModel> CreateThemesCommands() {
            var themesCommands = new List<CommandViewModel>();
            var converter = new ThemePaletteGlyphConverter();
            foreach (Theme theme in Theme.Themes.Where(x => x.Category == Theme.VisualStudioCategory && x.Name.StartsWith("VS2019"))) {
                var themeName = theme.Name;
                var paletteCommands = new List<CommandViewModel>();
                var defaultPalette = new CommandViewModel("Default", new DelegateCommand<Theme>(t => SetTheme(theme))) {
                    Glyph = (ImageSource)converter.Convert(themeName, null, null, CultureInfo.CurrentCulture)
                };
                paletteCommands.Add(defaultPalette);
                foreach (var palette in GetPalettes(theme)) {
                    var paletteTheme = Theme.Themes.FirstOrDefault(x => x.Name == string.Format("{0}{1}", palette.Name, themeName));
                    if (paletteTheme != null) {
                        var command = new CommandViewModel(palette.Name, new DelegateCommand<Theme>(t => SetTheme(paletteTheme))) {
                            Glyph = (ImageSource)converter.Convert(paletteTheme.Name, null, null, CultureInfo.CurrentCulture)
                        };
                        paletteCommands.Add(command);
                    }
                }
                themesCommands.Add(new CommandViewModel(theme.Name.Replace("VS2019", ""), paletteCommands) {
                    IsEnabled = true, IsSubItem = true, Glyph = (ImageSource)(new SvgImageSourceExtension() {Uri = theme.SvgGlyph}.ProvideValue(null))
                });
            }
            return themesCommands;
        }
        void SetTheme(Theme theme) {
            ApplicationThemeHelper.ApplicationThemeName = theme.Name;
        }
        IEnumerable<PredefinedThemePalette> GetPalettes(Theme theme) {
            switch (theme.Name) {
                case Theme.VS2019LightName:
                    return PredefinedThemePalettes.VS2019LightPalettes;
                case Theme.VS2019DarkName:
                    return PredefinedThemePalettes.VS2019DarkPalettes;
                default:
                    return PredefinedThemePalettes.VS2019BluePalettes;
            }
        }
        protected void OpenOrCloseWorkspace(PanelWorkspaceViewModel workspace, bool activateOnOpen = true) {
            if (Workspaces.Contains(workspace)) {
                workspace.IsClosed = !workspace.IsClosed;
            }
            else {
                Workspaces.Add(workspace);
                workspace.IsClosed = false;
            }
            if (activateOnOpen && workspace.IsOpened)
                SetActiveWorkspace(workspace);
        }
        bool ActivateDocument(string path) {
            var document = GetDocument(path);
            bool isFound = document != null;
            if (isFound) document.IsActive = true;
            return isFound;
        }
        List<BarModel> CreateBars() {
            return new List<BarModel>() {
                new BarModel("Main") { IsMainMenu = true, Commands = CreateCommands() },
                new BarModel("Standard") { Commands = CreateToolbarCommands() }
            };
        }
        List<CommandViewModel> CreateCommands() {
            return new List<CommandViewModel> {
                new CommandViewModel("File", CreateFileCommands()),
                new CommandViewModel("Edit", CreateEditCommands()),
                new CommandViewModel("Layouts", CreateLayoutCommands()),
                new CommandViewModel("View", CreateViewCommands()),
                new CommandViewModel("Help", CreateAboutCommands()),
                new CommandViewModel("Themes", CreateThemesCommands())
            };
        }
        DocumentViewModel CreateDocumentViewModel() {
            return CreatePanelWorkspaceViewModel<DocumentViewModel>();
        }
        List<CommandViewModel> CreateFileCommands() {
            var fileExecutedCommand = new DelegateCommand<object>(OnNewFileExecuted);
            var fileOpenCommand = new DelegateCommand<object>(OnFileOpenExecuted);
            var fileSaveCommand = new DelegateCommand<object>(OnSave);
            //var connectCommand = new DelegateCommand<object>(OnConnect);
            //var settingCommand = new DelegateCommand<object>(Setting_Click);

			CommandViewModel newCommand = new CommandViewModel("New") { IsSubItem = true };
            newProject = new CommandViewModel("Project...", fileExecutedCommand) { Glyph = Images.NewProject, KeyGesture = new KeyGesture(Key.N, ModifierKeys.Control | ModifierKeys.Shift), IsEnabled = false };
            newFile = new CommandViewModel("New File", fileExecutedCommand) { Glyph = Images.File, KeyGesture = new KeyGesture(Key.N, ModifierKeys.Control) };
            newCommand.Commands = new List<CommandViewModel>() { newProject, newFile };

            CommandViewModel openCommand = new CommandViewModel("Open") { IsSubItem = true, };
            openProject = new CommandViewModel("Project/Solution...") {
                Glyph = Images.OpenSolution,
                IsEnabled = false,
                KeyGesture = new KeyGesture(Key.O, ModifierKeys.Control | ModifierKeys.Shift),
            };
            openFile = new CommandViewModel("Open File", fileOpenCommand) { Glyph = Images.OpenFile, KeyGesture = new KeyGesture(Key.O, ModifierKeys.Control) };
            openCommand.Commands = new List<CommandViewModel>() { openProject, openFile };

            CommandViewModel closeFile = new CommandViewModel("Close");
            CommandViewModel closeSolution = new CommandViewModel("Close Solution") { Glyph = Images.CloseSolution };
            save = new CommandViewModel("Save", fileSaveCommand) { Glyph = Images.Save, KeyGesture = new KeyGesture(Key.S, ModifierKeys.Control) };
            saveAll = new CommandViewModel("Save All") { Glyph = Images.SaveAll, KeyGesture = new KeyGesture(Key.S, ModifierKeys.Control | ModifierKeys.Shift) };
			//ConnectCommandViewModel = new CommandViewModel("Connect", connectCommand) { Glyph = Images.Connect, KeyGesture = new KeyGesture(Key.K, ModifierKeys.Control), DisplayMode = BarItemDisplayMode.ContentAndGlyph };
			//SettingsCommandViewModel = new CommandViewModel("Settings", settingCommand) { Glyph = Images.Settings, KeyGesture = new KeyGesture(Key.W, ModifierKeys.Control), DisplayMode = BarItemDisplayMode.ContentAndGlyph };

			// ✅ Initialize Commands with Rx.NET Integration
			ConnectCommandViewModel = new CommandViewModel("Connect", new DelegateCommand<object>(OnConnect))
			{
				Glyph = Images.Connect,
				KeyGesture = new KeyGesture(Key.K, ModifierKeys.Control),
				DisplayMode = BarItemDisplayMode.ContentAndGlyph
			};

			SettingsCommandViewModel = new CommandViewModel("Settings", new DelegateCommand<object>(OnSettings))
			{
				Glyph = Images.Settings,
				KeyGesture = new KeyGesture(Key.W, ModifierKeys.Control),
				DisplayMode = BarItemDisplayMode.ContentAndGlyph
			};

			return new List<CommandViewModel>() { newCommand, openCommand, GetSeparator(), closeFile, closeSolution, GetSeparator(), save, saveAll, ConnectCommandViewModel, SettingsCommandViewModel };
        }
        List<CommandViewModel> CreateToolbarCommands() {
            CommandViewModel start = new CommandViewModel("Start") {
                Glyph = Images.Run,
                KeyGesture = new KeyGesture(Key.F5, ModifierKeys.None),
                DisplayMode = BarItemDisplayMode.ContentAndGlyph
            };
            CommandViewModel combo = new CommandViewModel("Configuration") { IsSubItem = true, IsComboBox = true };
            combo.Commands = new List<CommandViewModel>() { new CommandViewModel("Debug"), new CommandViewModel("Release") };

            CommandViewModel cut = new CommandViewModel("Cut") { Glyph = Images.Cut, KeyGesture = new KeyGesture(Key.X, ModifierKeys.Control) };
            CommandViewModel copy = new CommandViewModel("Copy") { Glyph = Images.Copy, KeyGesture = new KeyGesture(Key.C, ModifierKeys.Control) };
            CommandViewModel paste = new CommandViewModel("Paste") { Glyph = Images.Paste, KeyGesture = new KeyGesture(Key.V, ModifierKeys.Control) };

            CommandViewModel undo = new CommandViewModel("Undo") { Glyph = Images.Undo, KeyGesture = new KeyGesture(Key.Z, ModifierKeys.Control) };
            CommandViewModel redo = new CommandViewModel("Redo") { Glyph = Images.Redo, KeyGesture = new KeyGesture(Key.Y, ModifierKeys.Control) };

            return new List<CommandViewModel>() {
				SettingsCommandViewModel, ConnectCommandViewModel, newProject, newFile, openFile, save, saveAll, GetSeparator(), combo, start,
                GetSeparator(), cut, copy, paste, GetSeparator(), undo, redo, GetSeparator(),
                toolbox, solutionExplorer, properties, errorList, output, searchResults,
                GetSeparator(), loadLayout, saveLayout
            };
        }
        DocumentViewModel GetDocument(string filePath) {
            return Workspaces.OfType<DocumentViewModel>().FirstOrDefault(x => x.FilePath == filePath);
        }
        CommandViewModel GetSeparator() {
            return new CommandViewModel() { IsSeparator = true };
        }
        CommandViewModel GetShowCommand(PanelWorkspaceViewModel viewModel) {
            return new CommandViewModel(viewModel, new DelegateCommand(() => OpenOrCloseWorkspace(viewModel)));
        }
        void InitDefaultLayout() {
            var panels = new List<PanelWorkspaceViewModel> 
            { 
                ToolboxViewModel, 
                OpenOrdersViewModel, 
                HomeSymbolViewModel, 
                DomesticSymbolViewModel, 
                SecuritiesViewModel,
                ForeignSymbolViewModel, 
                SolutionExplorerViewModel, 
                PropertiesViewModel, 
                ErrorListViewModel 
            };
            foreach(var panel in panels) {
                OpenOrCloseWorkspace(panel, false);
            }
        }
        void OnFileOpenExecuted(object param) {
            var document = CreateDocumentViewModel();
            if(ActivateDocument(document.FilePath)) {
                document.Dispose();
                return;
            }
            OpenOrCloseWorkspace(document);
        }
        void OnLoadLayout() {
            SaveLoadLayoutService.LoadLayout();
        }
        void OnNewFileExecuted(object param) {
            string newItemName = solutionExplorerViewModel.Solution.AddNewItemToRoot();
            OpenItem(newItemName);
        }

		void OnSave(object param)
		{
			string newItemName = solutionExplorerViewModel.Solution.AddNewItemToRoot();
			OpenItem(newItemName);
		}




		void OnSettings(object param)
		{
			// ✅ Push an event for UI updates (e.g., Show Settings Popup)
			_eventSubject.OnNext(new EventData<object> { Type = "ShowSettingsPopup", Data = null });

			if (_mainWindow != null && _connector.Configure(_mainWindow))
			{
				_connector.Save().Serialize("ConnectorSettings.json");
			}
		}


		// ✅ Set the MainWindow instance from outside (instead of Messenger)
		public void SetMainWindow(Window window)
		{
			_mainWindow = window;
		}


		private void ChangeConnectStatus(bool isConnected)
		{
			_isConnected = isConnected;
			ConnectButtonLabel = _isConnected ? "Disconnect" : "Connect";
		}


		void OnConnect(object param)
        {
			_connector.Connected += Connector_Connected;
			//_connector.Connect();

			if (_isConnected)
			{
				_connector.Disconnect();
			}
			else
			{
				_connector.Connect();
				//_connector.LookupSecurities(StockSharp.Messages.Extensions.LookupAllCriteriaMessage);
			}
		}

		private void Connector_Connected()
		{
			ConnectCommandViewModel.Glyph = Images.Disconnect;
			// try lookup all securities
			_connector.LookupSecurities(StockSharp.Messages.Extensions.LookupAllCriteriaMessage);
		}

		void OnSaveLayout() {
            SaveLoadLayoutService.SaveLayout();
        }
        void OnWorkspaceRequestClose(object sender, EventArgs e) {
            var workspace = sender as PanelWorkspaceViewModel;
            if(workspace != null) {
                workspace.IsClosed = true;
                if(workspace is DocumentViewModel) {
                    workspace.Dispose();
                    Workspaces.Remove(workspace);
                }
            }
        }
        void OnWorkspacesChanged(object sender, NotifyCollectionChangedEventArgs e) {
            if(e.NewItems != null && e.NewItems.Count != 0)
                foreach(WorkspaceViewModel workspace in e.NewItems)
                    workspace.RequestClose += OnWorkspaceRequestClose;
            if(e.OldItems != null && e.OldItems.Count != 0)
                foreach(WorkspaceViewModel workspace in e.OldItems)
                    workspace.RequestClose -= OnWorkspaceRequestClose;
        }
        void OpenItem(string filePath) {
            if(ActivateDocument(filePath)) return;
            lastOpenedItem = CreateDocumentViewModel();
            lastOpenedItem.OpenItemByPath(filePath);
            OpenOrCloseWorkspace(lastOpenedItem);
        }
        void SetActiveWorkspace(WorkspaceViewModel workspace) {
            workspace.IsActive = true;
        }
        void ShowAbout() {
            About.ShowAbout(ProductKind.DXperienceWPF);
        }
        void SolutionExplorerViewModel_ItemOpening(object sender, SolutionItemOpeningEventArgs e) {
            OpenItem(e.SolutionItem.FilePath);
        }
    }
    
    

	#region Tool Panels

    

    
    #endregion

    #region Bars
    

    
    #endregion

    #region Images
    
    
    
    #endregion
}
