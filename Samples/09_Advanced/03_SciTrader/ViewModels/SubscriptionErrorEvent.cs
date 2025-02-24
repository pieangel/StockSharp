using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockSharp.Messages;

namespace SciTrader.ViewModels
{
	// ✅ Define a data structure for the event
	public class SubscriptionErrorEvent
	{
		public DataType DataType { get; set; }
		public SecurityId SecurityId { get; set; }
		public string ErrorMessage { get; set; }
	}
}
