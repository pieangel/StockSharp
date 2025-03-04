﻿namespace StockSharp.SignalMaster.Native;

static class Extensions
{
	/// <summary>
	/// Market side to S# side
	/// </summary>
	/// <param name="type">Side</param>
	/// <returns></returns>
	public static Sides ToSide(this string type)
	{
		return type == "buy" ? Sides.Buy : Sides.Sell;
	}

	/// <summary>
	/// Market order state to S# order state
	/// </summary>
	/// <param name="status">State</param>
	/// <returns></returns>
	public static OrderStates ToOrderState(this string status)
	{
		if (status == "new" || status == "open") return OrderStates.Active;
		return OrderStates.Done;
	}

	/// <summary>
	/// Market order type to S# order type
	/// </summary>
	/// <param name="type">Type</param>
	/// <returns></returns>
	public static OrderTypes ToOrderType(this string type)
	{
		return type == "market" ? OrderTypes.Market : OrderTypes.Limit;
	}

	/// <summary>
	/// Security ID to currency string
	/// </summary>
	/// <param name="securityId">ID</param>
	/// <returns></returns>
	public static string ToCurrency(this SecurityId securityId)
	{
		return securityId.SecurityCode;
	}

	/// <summary>
	/// Currency string to S# Security Id
	/// </summary>
	/// <param name="currency"></param>
	/// <returns></returns>
	public static SecurityId ToStockSharp(this string currency)
	{
		return new SecurityId
		{
			SecurityCode = currency,
			BoardCode = BoardCodes.SignalMaster,
		};
	}

	public static string ToNative(this Sides side)
	{
		return side switch
		{
			Sides.Buy => "buy",
			Sides.Sell => "sell",
			_ => throw new ArgumentOutOfRangeException(nameof(side), side, LocalizedStrings.InvalidValue),
		};
	}

	public static Sides ToSide2(this string side)
		=> side?.ToLowerInvariant() switch
		{
			"buy" or "bid" => Sides.Buy,
			"sell" or "ask" or "offer" => Sides.Sell,
			_ => throw new ArgumentOutOfRangeException(nameof(side), side, LocalizedStrings.InvalidValue),
		};

	public static string ToNative(this OrderTypes? type)
	{
		return type switch
		{
			null => null,
			OrderTypes.Limit => "limit",
			OrderTypes.Market => "market",
			OrderTypes.Conditional => "stop",
			_ => throw new ArgumentOutOfRangeException(nameof(type), type, LocalizedStrings.InvalidValue),
		};
	}

	public static OrderTypes ToOrderType2(this string type)
		=> type?.ToLowerInvariant() switch
		{
			"limit" => OrderTypes.Limit,
			"market" => OrderTypes.Market,
			"stop" or "stop limit" => OrderTypes.Conditional,
			_ => throw new ArgumentOutOfRangeException(nameof(type), type, LocalizedStrings.InvalidValue),
		};

	public static OrderStates ToOrderState2(this string status)
		=> status?.ToLowerInvariant() switch
		{
			"pending" or "received" => OrderStates.Pending,
			"open" or "active" => OrderStates.Active,
			"filled" or "done" or "canceled" or "cancelled" or "expired" => OrderStates.Done,
			"rejected" or "rejected" => OrderStates.Failed,
			_ => throw new ArgumentOutOfRangeException(nameof(status), status, LocalizedStrings.InvalidValue),
		};

	public static string ToNative(this TimeInForce? tif, DateTimeOffset? tillDate)
	{
		return tif switch
		{
			null => null,
			TimeInForce.PutInQueue => tillDate == null ? "GTC" : "GTT",
			TimeInForce.CancelBalance => "IOC",
			TimeInForce.MatchOrCancel => "FOK",
			_ => throw new ArgumentOutOfRangeException(nameof(tif), tif, LocalizedStrings.InvalidValue),
		};
	}

	public static TimeInForce? ToTimeInForce(this string tif)
	{
		return tif switch
		{
			null => null,
			"GTC" or "GTT" => (TimeInForce?)TimeInForce.PutInQueue,
			"IOC" => (TimeInForce?)TimeInForce.CancelBalance,
			"FOK" => (TimeInForce?)TimeInForce.MatchOrCancel,
			_ => throw new ArgumentOutOfRangeException(nameof(tif), tif, LocalizedStrings.InvalidValue),
		};
	}

	public static string ToSymbol(this SecurityId securityId)
	{
		return securityId.SecurityCode.ToUpperInvariant();
	}

	public static SecurityId ToStockSharp2(this string symbol)
	{
		return new SecurityId
		{
			SecurityCode = symbol.ToUpperInvariant(),
			BoardCode = BoardCodes.Krx,
		};
	}

	public static SecurityTypes? ToSecurityType(this string secType)
		=> secType?.ToLowerInvariant() switch
		{
			"spot" => SecurityTypes.CryptoCurrency,
			"futures" => SecurityTypes.Future,
			_ => null,
		};

	public static readonly PairSet<TimeSpan, string> TimeFrames = new()
	{
		{ TimeSpan.FromMinutes(1), "ONE_MINUTE" },
		{ TimeSpan.FromMinutes(5), "FIVE_MINUTE" },
		{ TimeSpan.FromMinutes(15), "FIFTEEN_MINUTE" },
		{ TimeSpan.FromMinutes(30), "THIRTY_MINUTE" },
		{ TimeSpan.FromHours(1), "ONE_HOUR" },
		{ TimeSpan.FromHours(2), "TWO_HOUR" },
		{ TimeSpan.FromHours(6), "SIX_HOUR" },
		{ TimeSpan.FromDays(1), "ONE_DAY" },
	};

	public static string ToNative(this TimeSpan timeFrame)
		=> TimeFrames.TryGetValue(timeFrame) ?? throw new ArgumentOutOfRangeException(nameof(timeFrame), timeFrame, LocalizedStrings.InvalidValue);

	public static TimeSpan ToTimeFrame(this string name)
		=> TimeFrames.TryGetKey2(name) ?? throw new ArgumentOutOfRangeException(nameof(name), name, LocalizedStrings.InvalidValue);
}