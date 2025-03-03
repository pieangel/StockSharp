﻿using StockSharp.Algo;
using StockSharp.Algo.Strategies;
using StockSharp.Messages;

using Ecng.Logging;

using System;

namespace StockSharp.Samples.Strategies.HistoryMarketRule
{
	public class SimpleRulesUntilStrategy : Strategy
	{
		protected override void OnStarted(DateTimeOffset time)
		{
			var tickSub = this.SubscribeTrades(Security);

			var mdSub = this.SubscribeMarketDepth(Security);

			var i = 0;
			mdSub.WhenOrderBookReceived(this).Do(depth =>
			{
				i++;
				this.AddInfoLog($"The rule WhenOrderBookReceived BestBid={depth.GetBestBid()}, BestAsk={depth.GetBestAsk()}");
				this.AddInfoLog($"The rule WhenOrderBookReceived i={i}");
			})
			.Until(() => i >= 10)
			.Apply(this);

			base.OnStarted(time);
		}
	}
}