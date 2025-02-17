using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SciTrader.Model
{
	public class OptionItem : BindableBase
	{
		public double CallPrice { get; set; }
		public string Strike { get; set; }
		public double PutPrice { get; set; }
		[Browsable(false)]
		public string OptionSymbolCode { get; set; }
	}
}
