using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SciTrader.ViewModels
{
	public class EventData<T>
	{
		public string Type { get; set; }  // ✅ Identifies data type (e.g., "Window", "Message")
		public T Data { get; set; }       // ✅ Generic Payload
	}
}
