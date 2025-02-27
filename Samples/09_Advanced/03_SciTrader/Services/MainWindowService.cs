using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SciTrader.Services
{
	public class MainWindowService
	{
		private readonly MainWindow _mainWindow;

		public MainWindowService(MainWindow mainWindow)
		{
			_mainWindow = mainWindow;
		}

		public void ShowMessage(string message, string title = "Notification")
		{
			_mainWindow.Dispatcher.Invoke(() =>
			{
				MessageBox.Show(_mainWindow, message, title);
			});
		}

		public void ChangeConnectionStatus(bool isConnected)
		{
			_mainWindow.Dispatcher.Invoke(() =>
			{
				_mainWindow.ChangeConnectStatus(isConnected);
			});
		}
	}

}
