using System.Reflection;
using Newtonsoft.Json;
namespace SciTrader.Model
{
	[Obfuscation(Feature = "renaming", ApplyToMembers = true)]
	internal class SciLeanRestResponse<T>
	where T : class
	{
		[JsonProperty("success")]
		public bool Success { get; set; }

		[JsonProperty("result")]
		public T Result { get; set; }
	}

	[Obfuscation(Feature = "renaming", ApplyToMembers = true)]
	internal class SciLeanRestResponseHasMoreData<T> : SciLeanRestResponse<T>
		where T : class
	{
		[JsonProperty("hasMoreData")]
		public bool HasMoreData { get; set; }
	}
}