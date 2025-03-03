﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SciTrader.Services;
using StockSharp.Xaml;
using StockSharp.Xaml.GridControl;

namespace SciTrader.Views
{
    /// <summary>
    /// Interaction logic for MonitorView.xaml
    /// </summary>
    public partial class MonitorView : UserControl
    {
        public MonitorView()
        {
            InitializeComponent();
        }

		private void MonitorView_OnLoaded(object sender, RoutedEventArgs e)
		{

			// ✅ Add GUI Listener for UI logging
			LogService.Instance.AddGuiListener(new GuiLogListener(Monitor));

		}
	}
}
