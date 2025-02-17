using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SciTrader.ViewModels
{
	public class BarModel : ViewModel
	{
		public BarModel(string displayName)
		{
			DisplayName = displayName;
		}
		public List<CommandViewModel> Commands { get; set; }
		public bool IsMainMenu { get; set; }
	}
}
