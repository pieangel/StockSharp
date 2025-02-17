using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SciTrader.Model
{
	public class FutureItem
	{
		[Browsable(false)]
		public string FutureSymbolCode { get; set; }
		public string FutureName { get; set; }
		public string ShortSymbolCode { get; set; }
		public double Price { get; set; }
	}
}
