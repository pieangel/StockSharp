using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SciTrader.Model
{
	public class HomeOption
	{
		public bool Registered { get; set; }
		public string ProductCode { get; set; }
		public string OptionName { get; set; }
		public Product CallProduct { get; set; }
		public Product PutProduct { get; set; }
	}
}
