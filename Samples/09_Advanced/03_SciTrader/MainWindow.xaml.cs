namespace StockSharp.Samples.Advanced.MultiConnect;

using System.ComponentModel;
using System.IO;

using Ecng.Common;
using Ecng.Configuration;

using StockSharp.Messages;
using StockSharp.Algo;
using StockSharp.Algo.Storages;
using StockSharp.Algo.Storages.Csv;
using StockSharp.BusinessEntities;
using StockSharp.SignalMaster;
using System.Security;
using System.Threading;
using System;

public partial class MainWindow
{
	public static event Action OnInitSignalMasterMessageAdapter;  // Define event
	private Connector _connector;
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

	private void InitSignalMasterMessageAdapter()
	{
		signalMasterMessageAdapter = new SignalMasterMessageAdapter(new SciLeanIdGenerator());
		var apiKey = ToSecureString("angelpie"); // Replace with your actual API key
		var apiSecret = ToSecureString("orion"); // Replace with your actual API secret

		signalMasterMessageAdapter.Key = apiKey;
		signalMasterMessageAdapter.Secret = apiSecret;

		// Add the Coinbase adapter to the connector
		_connector.Adapter.InnerAdapters.Add(signalMasterMessageAdapter);
	}

	public static void TriggerInitSignalMasterMessageAdapter()
	{
		OnInitSignalMasterMessageAdapter?.Invoke();  // Fire the event
	}

	public MainWindow()
	{
		InitializeComponent();
		Instance = this;

		Title = Title.Put("Connections with storage");

		OnInitSignalMasterMessageAdapter += InitSignalMasterMessageAdapter;  // Subscribe event
	}

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

	protected override void OnClosing(CancelEventArgs e)
	{
		MainPanel.Close();

		ServicesRegistry.EntityRegistry.DelayAction.DefaultGroup.WaitFlush(true);

		base.OnClosing(e);
	}

	public static MainWindow Instance { get; private set; }

	private void HistoryPath_OnFolderChanged(string path)
	{
		var connector = MainPanel.Connector;

		if (connector == null)
			return;

		connector.Adapter.StorageSettings.Drive = new LocalMarketDataDrive(path.ToFullPath());
		connector.LookupAll();
	}
}