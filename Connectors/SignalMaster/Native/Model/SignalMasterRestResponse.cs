namespace StockSharp.SignalMaster.Native.Model;

[Obfuscation(Feature = "renaming", ApplyToMembers = true)]
internal class SignalMasterRestResponse<T>
	where T : class
{
	[JsonProperty("success")]
	public bool Success { get; set; }

	[JsonProperty("result")]
	public T Result { get; set; }
}

[Obfuscation(Feature = "renaming", ApplyToMembers = true)]
internal class SignalMasterRestResponseHasMoreData<T> : SignalMasterRestResponse<T>
	where T : class
{
	[JsonProperty("hasMoreData")]
	public bool HasMoreData { get; set; }
}