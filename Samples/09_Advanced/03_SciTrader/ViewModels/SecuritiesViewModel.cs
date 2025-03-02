using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using StockSharp.Algo;
using StockSharp.BusinessEntities;
using StockSharp.Messages;
using StockSharp.Xaml;
using SciTrader.Helpers;
using Ecng.IO.Fossil;
using SciTrader.Model;

using DevExpress.Mvvm;

using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;
using SciTrader.Services;

namespace SciTrader.ViewModels
{
	public class SecuritiesViewModel : PanelWorkspaceViewModel
	{
		private Connector _connector;
		private Security _selectedSecurity;
		protected override string WorkspaceName { get { return "Toolbox"; } }
		public SecuritiesViewModel()
		{
			//_connector = MainWindow.Instance.Connector;

			Securities = new ObservableCollection<Security>();
			TimeFrames = new ObservableCollection<TimeSpan>();

			// Initialize Commands
			FindCommand = new DelegateCommand(FindSecurities);
			SubscribeLevel1Command = new DelegateCommand(SubscribeLevel1, CanSubscribe);
			SubscribeTicksCommand = new DelegateCommand(SubscribeTicks, CanSubscribe);
			SubscribeOrderLogCommand = new DelegateCommand(SubscribeOrderLog, CanSubscribe);
			SubscribeCandlesCommand = new DelegateCommand(SubscribeCandles, CanSubscribe);

			SecuritySelectedCommand = new DelegateCommand<Security>(OnSecuritySelected, CanSelectSecurity);

			TimeFrames = new ObservableCollection<TimeSpan>
			{
				TimeSpan.FromMinutes(1),
				TimeSpan.FromMinutes(5),
				TimeSpan.FromMinutes(15),
				TimeSpan.FromHours(1)
			};

			// Set default selected value
			SelectedTimeFrame = TimeFrames.FirstOrDefault();

			EventBus.Instance.ConnectorObservable
				.Subscribe(connector =>
				{
					_connector = connector;
				});
		}

		public ObservableCollection<Security> Securities { get; }
		public ObservableCollection<TimeSpan> TimeFrames { get; }

		public Security SelectedSecurity
		{
			get => _selectedSecurity;
			set
			{
				_selectedSecurity = value;
				OnPropertyChanged();
				UpdateCommandStates();
			}
		}
		public ICommand SecuritySelectedCommand { get; }

		private void OnSecuritySelected(Security security)
		{
			if (security != null)
			{
				SelectedSecurity = security;
				System.Diagnostics.Debug.WriteLine($"Security Selected: {security.Code}");
			}
		}

		private TimeSpan _selectedTimeFrame;

		public TimeSpan SelectedTimeFrame
		{
			get => _selectedTimeFrame;
			set
			{
				_selectedTimeFrame = value;
				OnPropertyChanged();
			}
		}

		public ICommand FindCommand { get; }
		public ICommand SubscribeLevel1Command { get; }
		public ICommand SubscribeTicksCommand { get; }
		public ICommand SubscribeOrderLogCommand { get; }
		public ICommand SubscribeCandlesCommand { get; }

		private void FindSecurities()
		{
			var criteria = new Security { Code = "EUR" };
			_connector.LookupSecurities(criteria);

			// Add a callback to populate the Securities list when results are received
			_connector.SecurityReceived += (s, security) =>
			{
				if (!Securities.Contains(security))
				{
					App.Current.Dispatcher.Invoke(() => Securities.Add(security));
				}
			};
		}

		private void SubscribeLevel1()
		{
			if (_connector.FindSubscriptions(SelectedSecurity, DataType.Level1).Any(s => s.State.IsActive()))
				_connector.UnSubscribe(_connector.FindSubscriptions(SelectedSecurity, DataType.Level1).First());
			else
				_connector.SubscribeLevel1(SelectedSecurity);
		}

		private void SubscribeTicks()
		{
			if (_connector.FindSubscriptions(SelectedSecurity, DataType.Ticks).Any(s => s.State.IsActive()))
				_connector.UnSubscribe(_connector.FindSubscriptions(SelectedSecurity, DataType.Ticks).First());
			else
				_connector.SubscribeTrades(SelectedSecurity);
		}

		private void SubscribeOrderLog()
		{
			if (_connector.FindSubscriptions(SelectedSecurity, DataType.OrderLog).Any(s => s.State.IsActive()))
				_connector.UnSubscribe(_connector.FindSubscriptions(SelectedSecurity, DataType.OrderLog).First());
			else
				_connector.SubscribeOrderLog(SelectedSecurity);
		}

		private void SubscribeCandles()
		{
			var selectedTimeFrame = TimeFrames.FirstOrDefault();
			if (selectedTimeFrame == default) return;

			var msg = new MarketDataMessage
			{
				IsSubscribe = true,
				DataType2 = selectedTimeFrame.TimeFrame(),
				From = DateTime.Today.AddDays(-30),
				BuildMode = MarketDataBuildModes.LoadAndBuild
			};

			//_connector.SubscribeCandles(SelectedSecurity, msg);
		}

		private bool CanSubscribe() => SelectedSecurity != null;

		private void UpdateCommandStates()
		{
			(FindCommand as DelegateCommand)?.RaiseCanExecuteChanged();
			(SubscribeLevel1Command as DelegateCommand)?.RaiseCanExecuteChanged();
			(SubscribeTicksCommand as DelegateCommand)?.RaiseCanExecuteChanged();
			(SubscribeOrderLogCommand as DelegateCommand)?.RaiseCanExecuteChanged();
			(SubscribeCandlesCommand as DelegateCommand)?.RaiseCanExecuteChanged();
		}

		public event PropertyChangedEventHandler PropertyChanged;
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private bool CanSelectSecurity(Security security)
		{
			return security != null; // Enables the command only when a valid security is selected.
		}
	}
}


