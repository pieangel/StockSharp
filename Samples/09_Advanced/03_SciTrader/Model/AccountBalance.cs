using System;
using System.Reflection;
using Newtonsoft.Json;
namespace SciTrader.Model
{
	[Obfuscation(Feature = "renaming", ApplyToMembers = true)]
	internal class AccountBalance
	{
		[JsonProperty("id")]
		public Guid Id { get; set; }

		[JsonProperty("currency")]
		public string Currency { get; set; }

		[JsonProperty("balance")]
		public double Balance { get; set; }

		[JsonProperty("hold")]
		public double Hold { get; set; }

		[JsonProperty("available")]
		public double Available { get; set; }

		[JsonProperty("margin_enabled")]
		public bool MarginEnabled { get; set; }

		[JsonProperty("funded_amount")]
		public double FundedAmount { get; set; }

		[JsonProperty("default_amount")]
		public double DefaultAmount { get; set; }
	}
}