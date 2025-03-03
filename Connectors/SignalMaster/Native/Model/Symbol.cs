namespace StockSharp.SignalMaster.Native.Model;

using System;
using System.Reflection;
using Newtonsoft.Json;

/// <summary>
/// The symbol.
/// </summary>
[Obfuscation(Feature = "renaming", ApplyToMembers = true)]
public class Symbol
{
	/// <summary>
	/// Gets or sets the type.
	/// </summary>
	[JsonProperty("type")]
	public string Type { get; set; }

	/// <summary>
	/// Gets or sets the English name.
	/// </summary>
	[JsonProperty("name_en")]
	public string Name { get; set; }

	/// <summary>
	/// Gets or sets the korean name.
	/// </summary>
	[JsonProperty("name_kr")]
	public string NameKr { get; set; }

	/// <summary>
	/// Gets or sets the symbol code.
	/// </summary>
	[JsonProperty("symbol_code")]
	public string SymbolCode { get; set; }

	/// <summary>
	/// Gets or sets the market name.
	/// </summary>
	[JsonProperty("market_name")]
	public string MarketName { get; set; }

	/// <summary>
	/// Gets or sets the product code.
	/// </summary>
	[JsonProperty("product_code")]
	public string ProductCode { get; set; }

	/// <summary>
	/// Gets or sets the decimal places.
	/// </summary>
	[JsonProperty("decimal_places")]
	public int DecimalPlaces { get; set; } = 2;

	/// <summary>
	/// Gets or sets the min amount.
	/// </summary>
	[JsonProperty("min_amount")]
	public decimal MinAmount { get; set; }  // Changed to decimal

	/// <summary>
	/// Gets or sets the contract size.
	/// </summary>
	[JsonProperty("contract_size")]
	public decimal ContractSize { get; set; } = 0.05m;  // Changed to decimal

	/// <summary>
	/// Gets or sets the tick value.
	/// </summary>
	[JsonProperty("tick_value")]
	public decimal TickValue { get; set; } = 12500m;  // Changed to decimal

	/// <summary>
	/// Gets or sets the tick size.
	/// </summary>
	[JsonProperty("tick_size")]
	public decimal TickSize { get; set; } = 0.05m;  // Changed to decimal

	/// <summary>
	/// Gets or sets the expire date.
	/// </summary>
	[JsonProperty("expire_date")]
	public string ExpireDate { get; set; }

	/// <summary>
	/// Gets or sets the total volume.
	/// </summary>
	[JsonProperty("total_volume")]
	public int TotalVolume { get; set; } = 0;

	/// <summary>
	/// Gets or sets the pre day volume.
	/// </summary>
	[JsonProperty("pre_day_volume")]
	public int PreDayVolume { get; set; } = 0;

	/// <summary>
	/// Gets or sets the deposit.
	/// </summary>
	[JsonProperty("deposit")]
	public string Deposit { get; set; }

	/// <summary>
	/// Gets or sets the currency.
	/// </summary>
	[JsonProperty("currency")]
	public string Currency { get; set; }

	/// <summary>
	/// Gets or sets the exchange.
	/// </summary>
	[JsonProperty("exchange")]
	public string Exchange { get; set; }

	/// <summary>
	/// Gets or sets the pre day rate.
	/// </summary>
	[JsonProperty("pre_day_rate")]
	public string PreDayRate { get; set; }

	/// <summary>
	/// Gets or sets the full code.
	/// </summary>
	[JsonProperty("full_code")]
	public string FullCode { get; set; }

	/// <summary>
	/// Gets or sets the remain days.
	/// </summary>
	[JsonProperty("remain_days")]
	public int RemainDays { get; set; } = 0;

	/// <summary>
	/// Gets or sets the last trade day.
	/// </summary>
	[JsonProperty("last_trade_day")]
	public string LastTradeDay { get; set; }

	/// <summary>
	/// Gets or sets the high limit price.
	/// </summary>
	[JsonProperty("high_limit_price")]
	public string HighLimitPrice { get; set; }

	/// <summary>
	/// Gets or sets the low limit price.
	/// </summary>
	[JsonProperty("low_limit_price")]
	public string LowLimitPrice { get; set; }

	/// <summary>
	/// Gets or sets the pre day close.
	/// </summary>
	[JsonProperty("pre_day_close")]
	public string PreDayClose { get; set; }

	/// <summary>
	/// Gets or sets the standard price.
	/// </summary>
	[JsonProperty("standard_price")]
	public string StandardPrice { get; set; }

	/// <summary>
	/// Gets or sets the strike.
	/// </summary>
	[JsonProperty("strike")]
	public string Strike { get; set; }

	/// <summary>
	/// Gets or sets the atm type.
	/// </summary>
	[JsonProperty("atm_type")]
	public int AtmType { get; set; } = 0;

	/// <summary>
	/// Gets or sets the recent month.
	/// </summary>
	[JsonProperty("recent_month")]
	public int RecentMonth { get; set; } = 1;

	//[JsonProperty("symbol_type")]
	//public SymbolType SymbolType { get; set; } = SymbolType.None;
}
