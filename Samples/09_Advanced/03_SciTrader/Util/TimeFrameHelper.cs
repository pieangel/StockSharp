using System;
using StockSharp.Algo.Candles;
using StockSharp.Messages;
public static class TimeFrameHelper
{
	/// <summary>
	/// Converts a StockSharp TimeFrame to a .NET TimeSpan.
	/// </summary>
	/*
	public static TimeSpan ToTimeSpan(this TimeFrame timeFrame)
	{
		return timeFrame switch
		{
			TimeFrame.Min1 => TimeSpan.FromMinutes(1),
			TimeFrame.Min5 => TimeSpan.FromMinutes(5),
			TimeFrame.Min10 => TimeSpan.FromMinutes(10),
			TimeFrame.Min15 => TimeSpan.FromMinutes(15),
			TimeFrame.Min30 => TimeSpan.FromMinutes(30),
			TimeFrame.Hour1 => TimeSpan.FromHours(1),
			TimeFrame.Hour4 => TimeSpan.FromHours(4),
			TimeFrame.Day1 => TimeSpan.FromDays(1),
			TimeFrame.Week => TimeSpan.FromDays(7),
			TimeFrame.Month => TimeSpan.FromDays(30), // Approximate for a month
			_ => throw new ArgumentOutOfRangeException(nameof(timeFrame), $"Unsupported TimeFrame: {timeFrame}")
		};
	}

	/// <summary>
	/// Converts a .NET TimeSpan to the closest StockSharp TimeFrame.
	/// </summary>
	public static TimeFrame ToTimeFrame(this TimeSpan timeSpan)
	{
		if (timeSpan.TotalMinutes == 1) return TimeFrame.Min1;
		if (timeSpan.TotalMinutes == 5) return TimeFrame.Min5;
		if (timeSpan.TotalMinutes == 10) return TimeFrame.Min10;
		if (timeSpan.TotalMinutes == 15) return TimeFrame.Min15;
		if (timeSpan.TotalMinutes == 30) return TimeFrame.Min30;
		if (timeSpan.TotalHours == 1) return TimeFrame.Hour1;
		if (timeSpan.TotalHours == 4) return TimeFrame.Hour4;
		if (timeSpan.TotalDays == 1) return TimeFrame.Day1;
		if (timeSpan.TotalDays == 7) return TimeFrame.Week;
		if (timeSpan.TotalDays >= 28 && timeSpan.TotalDays <= 31) return TimeFrame.Month;

		throw new ArgumentOutOfRangeException(nameof(timeSpan), $"Unsupported TimeSpan: {timeSpan}");
	}
	*/
}
