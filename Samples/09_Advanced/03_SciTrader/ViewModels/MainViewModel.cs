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
using SciTrader.Services;
using StockSharp.SignalMaster;


namespace SciTrader.ViewModels {
    public class MainViewModel {

		private Connector _connector;
		private MainWindow _mainWindow;

		private const string _connectorFile = "ConnectorFile.json";





		private readonly List<Subscription> _subscriptions = new();
		//private SecurityId? _selectedSecurityId;

        
		private SignalMasterMessageAdapter signalMasterMessageAdapter;

		public class SciTraderIdGenerator : Ecng.Common.IdGenerator
		{
			private long _currentId;

			public SciTraderIdGenerator()
			{
				_currentId = 1;
			}

			public override long GetNextId()
			{
				return Interlocked.Increment(ref _currentId);
			}
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
            InitEventBus();
		}

        private void InitEventBus()
        {
			EventBus.Instance.MainWindowObservable
		        .Subscribe(window =>
		        {
			        _mainWindow = window as MainWindow;
		        });

			EventBus.Instance.ConnectorObservable
				.Subscribe(connector =>
				{
					_connector = connector;
					InitMessageAdapter();
					InitConnect();
				});
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
        
		private void InitMessageAdapter()
		{
			signalMasterMessageAdapter = new SignalMasterMessageAdapter(new SciTraderIdGenerator());
			var apiKey = ToSecureString("angelpie"); // Replace with your actual API key
			var apiSecret = ToSecureString("orion"); // Replace with your actual API secret

			signalMasterMessageAdapter.Key = apiKey;
			signalMasterMessageAdapter.Secret = apiSecret;

			// Add the Coinbase adapter to the connector
			_connector.Adapter.InnerAdapters.Add(signalMasterMessageAdapter);
		}
        

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
			if (_mainWindow != null && _connector.Configure(_mainWindow))
		    {
				_connector.Save().Serialize("ConnectorSettings.json");
			}
		}

		private void ChangeConnectStatus(bool isConnected)
		{
			//_isConnected = isConnected;
			//ConnectButtonLabel = _isConnected ? "Disconnect" : "Connect";
		}


		void OnConnect(object param)
        {
			_connector.Connected += Connector_Connected;
			_connector.Connect();
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
