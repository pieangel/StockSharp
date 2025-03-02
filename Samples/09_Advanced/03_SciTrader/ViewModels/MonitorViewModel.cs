using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SciTrader.ViewModels
{

	public class MonitorViewModel : PanelWorkspaceViewModel
	{
		protected override string WorkspaceName => "BottomHost";

		public MonitorViewModel()
		{
			DisplayName = "Monitor";
			Glyph = Images.Toolbox;
		}
		
	}
}
