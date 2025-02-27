namespace SciTrader;

using System.Windows;
using System.Windows.Threading;
using SciTrader.Model;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SciTrader.Services;
using SciTrader.ViewModels;
using System;
using DevExpress.Xpf.Core;

public partial class App
{
	public static IHost? AppHost { get; private set; }
	public SymbolManager SymbolManager { get; } = new SymbolManager();
	private void ApplicationDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
	{
		MessageBox.Show(MainWindow, e.Exception.ToString());
		e.Handled = true;
	}

	public App()
	{
		AppHost = Host.CreateDefaultBuilder()
			.ConfigureServices((hostContext, services) =>
			{
				// Register Services
				services.AddSingleton<ConnectorService>();
				services.AddTransient<MainViewModel>();

				// Register MainWindow as Transient or Singleton
				services.AddSingleton<MainWindow>();
			})
			.Build();
	}

	protected override void OnStartup(StartupEventArgs e)
	{
		base.OnStartup(e);

		// Resolve MainWindow from DI container
		var mainWindow = AppHost.Services.GetRequiredService<MainWindow>();
		mainWindow.Show();
	}

	protected override void OnExit(ExitEventArgs e)
	{
		AppHost?.Dispose();
		base.OnExit(e);
	}
}

