using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace SciTrader.ViewModels
{
// 	public class ToolboxItemViewModel
// 	{
// 		public string Name { get; set; }
// 		public object Data { get; set; }
// 	}

	public class ToolboxViewModel : PanelWorkspaceViewModel
	{
		// 		private ObservableCollection<ToolboxItemViewModel> _items;
		// 		private ToolboxItemViewModel _selectedItem;
		// 
		// 		public ToolboxViewModel()
		// 		{
		// 			DisplayName = "Toolbox";
		// 			Glyph = Images.Toolbox;
		// 
		// 			// Initialize Items with some sample data (replace this with actual data)
		// 			Items = new ObservableCollection<ToolboxItemViewModel>
		// 			{
		// 				new ToolboxItemViewModel { Name = "Item 1", Data = new object() },
		// 				new ToolboxItemViewModel { Name = "Item 2", Data = new object() }
		// 			};
		// 
		// 			SelectedItem = Items.Count > 0 ? Items[0] : null;
		// 		}
		// 
		// 		protected override string WorkspaceName => "Toolbox";
		// 
		// 		public ObservableCollection<ToolboxItemViewModel> Items
		// 		{
		// 			get => _items;
		// 			set
		// 			{
		// 				_items = value;
		// 			}
		// 		}
		// 
		// 		public ToolboxItemViewModel SelectedItem
		// 		{
		// 			get => _selectedItem;
		// 			set
		// 			{
		// 				_selectedItem = value;
		// 			}
		// 		}

		public ToolboxViewModel()
		{
			DisplayName = "Toolbox";
			Glyph = Images.Toolbox;

		}
		// If DisplayName is empty, the BindableName property will throw an exception.
		protected override string WorkspaceName => "Toolbox";
		public OpenOrdersViewModel OpenOrders { get; } = new OpenOrdersViewModel();
		public HomeSymbolViewModel OrderHistory { get; } = new HomeSymbolViewModel();
	}
}
