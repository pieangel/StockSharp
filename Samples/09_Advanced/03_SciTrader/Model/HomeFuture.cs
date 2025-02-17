using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SciTrader.Model
{
	public class HomeFuture
	{
		public bool Registered { get; set; }
		public string FutureName { get; set; }
		public string ProductCode { get; set; }
		public Product Product { get; set; }
	}
}
