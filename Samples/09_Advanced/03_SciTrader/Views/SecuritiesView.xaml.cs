using System;
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

using Ecng.Collections;
using Ecng.Common;
using Ecng.Xaml;
using Ecng.Serialization;
using Ecng.ComponentModel;

using StockSharp.Algo;
using StockSharp.BusinessEntities;
using StockSharp.Xaml;
using StockSharp.Localization;
using StockSharp.Messages;

using SelectionChangedEventArgs = System.Windows.Controls.SelectionChangedEventArgs;
using System.ComponentModel;
using SciTrader.ViewModels;

namespace SciTrader.Views
{
	public partial class SecuritiesView : UserControl
	{
		public SecuritiesView()
		{
			InitializeComponent();
			DataContext = new SecuritiesViewModel(); // Ensure this is set
		}
	}
}