using System;
using System.Collections.Generic;

using Ecng.ComponentModel;

using StockSharp.Algo.Indicators;
using StockSharp.Algo.Strategies;
using StockSharp.BusinessEntities;
using StockSharp.Messages;

namespace StockSharp.Samples.Strategies.HistoryBollingerBands
{
	public class BollingerStrategyUpBandStrategy : Strategy
	{
		private readonly StrategyParam<int> _bollingerLength;
		private readonly StrategyParam<decimal> _bollingerDeviation;
		private readonly StrategyParam<DataType> _candleTypeParam;

		private BollingerBands _bollingerBands;

		public int BollingerLength
		{
			get => _bollingerLength.Value;
			set => _bollingerLength.Value = value;
		}

		public decimal BollingerDeviation
		{
			get => _bollingerDeviation.Value;
			set => _bollingerDeviation.Value = value;
		}

		public DataType CandleType
		{
			get => _candleTypeParam.Value;
			set => _candleTypeParam.Value = value;
		}

		public BollingerStrategyUpBandStrategy()
		{
			_bollingerLength = Param(nameof(BollingerLength), 20)
							  .SetValidator(new IntGreaterThanZeroAttribute())
							  .SetDisplay("Bollinger Length", "Length of the Bollinger Bands indicator", "Indicators")
							  .SetCanOptimize(true)
							  .SetOptimize(10, 50, 5);

			_bollingerDeviation = Param(nameof(BollingerDeviation), 2.0m)
								 .SetDisplay("Bollinger Deviation", "Standard deviation multiplier for Bollinger Bands", "Indicators")
								 .SetCanOptimize(true)
								 .SetOptimize(1.0m, 3.0m, 0.5m);

			_candleTypeParam = Param(nameof(CandleType), DataType.TimeFrame(TimeSpan.FromMinutes(5)))
							  .SetDisplay("Candle Type", "Type of candles to use", "General");
		}

		public override IEnumerable<(Security sec, DataType dt)> GetWorkingSecurities()
		{
			return new[] { (Security, CandleType) };
		}

		protected override void OnStarted(DateTimeOffset time)
		{
			base.OnStarted(time);

			// Create the Bollinger Bands indicator
			_bollingerBands = new BollingerBands
			{
				Length = BollingerLength,
				Width = BollingerDeviation
			};

			// Create subscription and bind indicator
			var subscription = SubscribeCandles(CandleType);
			subscription
				.Bind(_bollingerBands, ProcessCandle)
				.Start();

			// Setup chart visualization if available
			var area = CreateChartArea();
			if (area != null)
			{
				DrawCandles(area, subscription);
				DrawIndicator(area, _bollingerBands, System.Drawing.Color.Purple);
				DrawOwnTrades(area);
			}
		}

		private void ProcessCandle(ICandleMessage candle, decimal middleBand, decimal upperBand, decimal lowerBand)
		{
			// Skip unfinished candles
			if (candle.State != CandleStates.Finished)
				return;

			// Check if strategy is ready to trade
			if (!IsFormedAndOnlineAndAllowTrading())
				return;

			// Trading logic:
			// Buy when price touches the upper band (only when no position)
			if (candle.ClosePrice >= upperBand && Position == 0)
			{
				BuyMarket(Volume);
			}
			// Sell to close position when price reaches the middle band (only when long)
			else if (candle.ClosePrice <= middleBand && Position > 0)
			{
				SellMarket(Math.Abs(Position));
			}
		}
	}
}