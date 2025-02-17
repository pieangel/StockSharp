namespace SciTrader;

using System.Windows;
using System.Windows.Threading;
using SciTrader.Model;

public partial class App
{
	public SymbolManager SymbolManager { get; } = new SymbolManager();
	private void ApplicationDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
	{
		MessageBox.Show(MainWindow, e.Exception.ToString());
		e.Handled = true;
	}
}