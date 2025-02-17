using DevExpress.Xpf.Docking;
using DevExpress.Xpf.PropertyGrid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SciTrader.ViewModels
{
	public class PropertiesViewModel : PanelWorkspaceViewModel
	{
		public PropertiesViewModel()
		{
			DisplayName = "Properties";
			Glyph = Images.PropertiesWindow;
			SelectedItem = new PropertyItem(new PropertyGridControl());
			Items = new List<PropertyItem> {
				SelectedItem,
                //new PropertyItem(new AccordionControl()),
                new PropertyItem(new DocumentPanel()),
				new PropertyItem(new DocumentGroup()),
				new PropertyItem(new DevExpress.Xpf.Docking.LayoutPanel())
			};
		}

		public List<PropertyItem> Items { get; set; }
		public virtual PropertyItem SelectedItem { get; set; }
		protected override string WorkspaceName { get { return "RightHost"; } }
	}

	public class PropertyItem
	{
		public PropertyItem(object data)
		{
			Data = data;
			Name = Data.ToString();
		}

		public object Data { get; set; }
		public string Name { get; set; }
	}
}
