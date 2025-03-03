namespace StockSharp.SignalMaster.Native.Model;

[Obfuscation(Feature = "renaming", ApplyToMembers = true)]
internal class Account
{
	[JsonProperty("account_no")]
	public string AccountCode { get; set; }

	[JsonProperty("account_name")]
	public string AccountName { get; set; }

	[JsonProperty("account_type")]
	public string AccountType { get; set; }

	[JsonProperty("currency")]
	public string Currency { get; set; }

}