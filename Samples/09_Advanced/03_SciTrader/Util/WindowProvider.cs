using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SciTrader.Util
{
	public class WindowProvider : IWindowProvider
	{
		public Window GetMainWindow()
		{
			return Application.Current.MainWindow; // Return the main window
		}
	}
}
