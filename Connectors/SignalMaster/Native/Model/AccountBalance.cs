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
		public decimal Balance { get; set; }

		[JsonProperty("total")]
		public decimal Total { get; set; }

		[JsonProperty("hold")]
		public decimal Hold { get; set; }

		[JsonProperty("available")]
		public decimal Available { get; set; }

		[JsonProperty("margin_enabled")]
		public bool MarginEnabled { get; set; }

		[JsonProperty("funded_amount")]
		public decimal FundedAmount { get; set; }

		[JsonProperty("default_amount")]
		public decimal DefaultAmount { get; set; }
	}
}