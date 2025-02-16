namespace StockSharp.Samples.Basic.ConnectAndDownloadInstruments;

using System.Windows;
using System.IO;

using Ecng.Serialization;
using Ecng.Configuration;

using StockSharp.Configuration;
using StockSharp.Messages;
using StockSharp.Algo;
using StockSharp.BusinessEntities;
using StockSharp.Xaml;
using StockSharp.SignalMaster;
using System.Security;
using System.Threading;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
	private readonly Connector _connector = new();
	private const string _connectorFile = "ConnectorFile.json";

	private SignalMasterMessageAdapter slMessageAdapter;

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
		slMessageAdapter = new SignalMasterMessageAdapter(new SciLeanIdGenerator());
		var apiKey = ToSecureString("angelpie"); // Replace with your actual API key
		var apiSecret = ToSecureString("orion"); // Replace with your actual API secret

		slMessageAdapter.Key = apiKey;
		slMessageAdapter.Secret = apiSecret;

		// Add the Coinbase adapter to the connector
		_connector.Adapter.InnerAdapters.Add(slMessageAdapter);
	}

	public MainWindow()
	{
		InitializeComponent();

		InitSignalMasterMessageAdapter();

		// registering all connectors
		ConfigManager.RegisterService<IMessageAdapterProvider>(new InMemoryMessageAdapterProvider(_connector.Adapter.InnerAdapters));

		if (File.Exists(_connectorFile))
		{
			_connector.Load(_connectorFile.Deserialize<SettingsStorage>());
		}
	}

	private void Setting_Click(object sender, RoutedEventArgs e)
	{
		if (_connector.Configure(this))
		{
			_connector.Save().Serialize(_connectorFile);
		}
	}

	private void Connect_Click(object sender, RoutedEventArgs e)
	{
		SecurityPicker.SecurityProvider = _connector;
		SecurityPicker.MarketDataProvider = _connector;
		_connector.Connected += Connector_Connected;
		_connector.Connect();
	}

	private void Connector_Connected()
	{
		// try lookup all securities
		_connector.LookupSecurities(StockSharp.Messages.Extensions.LookupAllCriteriaMessage);
	}

	private void SecurityPicker_SecuritySelected(Security security)
	{
		if (security == null) return;
		_connector.SubscribeLevel1(security);
	}
}