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
		public static MainWindowService Instance { get; } = new();
		private MainWindow _mainWindow;

		public void SetMainWindow(MainWindow window)
		{
			_mainWindow = window;
		}

		public MainWindow GetMainWindow() => _mainWindow;
	}

}
