namespace StockSharp.SignalMaster;

partial class SignalMasterMessageAdapter
{
	private const int _tickPaginationLimit = 5000;
	private const int _candlesPaginationLimit = 1501;

	private void SessionOnNewTrade(string pair, List<Trade> trades)
	{
		foreach (var trade in trades)
		{
			SendOutMessage(new ExecutionMessage
			{
				DataTypeEx = DataType.Ticks,
				SecurityId = pair.ToStockSharp(),
				TradeId = trade.Id,
				TradePrice = trade.Price,
				TradeVolume = trade.Size,
				ServerTime = trade.Time,
				OriginSide = trade.Side.ToSide(),
			});
		}
	}

	private void SessionOnNewOrderBook(string pair, OrderBook book, QuoteChangeStates state)
	{
		SendOutMessage(new QuoteChangeMessage
		{
			State = state,
			SecurityId = pair.ToStockSharp(),
			Bids = book.Bids.Select(e => new QuoteChange(e.Price, e.Size)).ToArray(),
			Asks = book.Asks.Select(e => new QuoteChange(e.Price, e.Size)).ToArray(),
			ServerTime = book.Time,
		});
	}

	private void SessionOnNewLevel1(string pair, Level1 level1)
	{
		SendOutMessage(new Level1ChangeMessage()
		{
			SecurityId = pair.ToStockSharp(),
			ServerTime = level1.Time
		}
		.TryAdd(Level1Fields.BestBidPrice, level1.Bid)
		.TryAdd(Level1Fields.BestAskPrice, level1.Ask)
		.TryAdd(Level1Fields.BestBidVolume, level1.BidSize)
		.TryAdd(Level1Fields.BestAskVolume, level1.AskSize)
		);
	}

	/// <inheritdoc />
	protected override async ValueTask OnLevel1SubscriptionAsync(MarketDataMessage mdMsg, CancellationToken cancellationToken)
	{
		SendSubscriptionReply(mdMsg.TransactionId);

		var symbolCode = mdMsg.SecurityId.ToSymbol();

		if (mdMsg.IsSubscribe)
		{
			await _wsClient.SubscribeLevel1(mdMsg.TransactionId, symbolCode, cancellationToken);

			SendSubscriptionResult(mdMsg);
		}
		else
			await _wsClient.UnsubscribeLevel1(mdMsg.OriginalTransactionId, symbolCode, cancellationToken);
	}

	/// <inheritdoc />
	protected override async ValueTask OnMarketDepthSubscriptionAsync(MarketDataMessage mdMsg, CancellationToken cancellationToken)
	{
		SendSubscriptionReply(mdMsg.TransactionId);

		var symbolCode = mdMsg.SecurityId.ToSymbol();

		if (mdMsg.IsSubscribe)
		{
			await _wsClient.SubscribeOrderBook(mdMsg.TransactionId, symbolCode, cancellationToken);

			SendSubscriptionResult(mdMsg);
		}
		else
			await _wsClient.UnsubscribeOrderBook(mdMsg.OriginalTransactionId, symbolCode, cancellationToken);
	}

	/// <inheritdoc />
	protected override async ValueTask OnTicksSubscriptionAsync(MarketDataMessage mdMsg, CancellationToken cancellationToken)
	{
		SendSubscriptionReply(mdMsg.TransactionId);

		var symbolCode = mdMsg.SecurityId.ToSymbol();

		if (mdMsg.IsSubscribe)
		{
			if (mdMsg.From is not null)
			{
				var startTime = mdMsg.From.Value.UtcDateTime;
				var endTime = mdMsg.To?.UtcDateTime ?? DateTime.UtcNow;
				var left = mdMsg.Count ?? long.MaxValue;
				double request_id = _requestIdGenerator.GetNextId();

				while (startTime < endTime)
				{
					var trades = await _restClient.GetMarketTrades(symbolCode, startTime, endTime, cancellationToken);

					var lastTime = startTime;

					foreach (var trade in trades.OrderBy(t => t.Time))
					{
						if (trade.Time < startTime)
							continue;

						if (trade.Time > endTime)
							break;

						SendOutMessage(new ExecutionMessage
						{
							DataTypeEx = DataType.Ticks,
							SecurityId = mdMsg.SecurityId,
							TradeId = trade.Id,
							TradePrice = trade.Price,
							TradeVolume = trade.Size,
							ServerTime = trade.Time,
							OriginSide = trade.Side.ToSide(),
							OriginalTransactionId = mdMsg.TransactionId
						});

						if (--left <= 0)
							break;

						lastTime = trade.Time;
					}

					if (trades.Count != _tickPaginationLimit || --left <= 0)
						break;

					startTime = lastTime;
				}
			}

			if (!mdMsg.IsHistoryOnly())
				await _wsClient.SubscribeTradesChannel(mdMsg.TransactionId, symbolCode, WsTradeChannelSubscriber.Trade, cancellationToken);

			SendSubscriptionResult(mdMsg);
		}
		else
		{
			await _wsClient.UnsubscribeTradesChannel(mdMsg.OriginalTransactionId, symbolCode, WsTradeChannelSubscriber.Trade, cancellationToken);
		}
	}

	/// <inheritdoc />
	protected override async ValueTask OnTFCandlesSubscriptionAsync(MarketDataMessage mdMsg, CancellationToken cancellationToken)
	{
		SendSubscriptionReply(mdMsg.TransactionId);

		var secId = mdMsg.SecurityId;
		var symbol = secId.ToSymbol();

		var tf = mdMsg.GetTimeFrame();
		var tfName = tf.ToNative();

		var request_id = _requestIdGenerator.GetNextId();

		DateTimeOffset now = DateTimeOffset.UtcNow;
		string startDate = mdMsg.From?.ToString("MM/dd/yyyy HH:mm:ss") ?? string.Empty;
		string endDate = mdMsg.To?.ToString("MM/dd/yyyy HH:mm:ss") ?? string.Empty;
		if (startDate.IsEmpty())
		{
			startDate = now.ToString("MM/dd/yyyy HH:mm:ss");
		}
		if (endDate.IsEmpty()) {
			endDate = now.AddHours(-2).ToString("MM/dd/yyyy HH:mm:ss");
		}

		if (mdMsg.IsSubscribe)
		{
			if (mdMsg.From is not null)
			{
				var candles = await _restClient.GetMarketCandles(request_id.ToString(), symbol, tfName, startDate, endDate, cancellationToken);
				var left = mdMsg.Count ?? long.MaxValue;

				foreach (var candle in candles)
				{
					ProcessCandle(candle, secId, tf, mdMsg.TransactionId);

					if (--left <= 0)
						break;
				}
			}

			if (!mdMsg.IsHistoryOnly())
				await _wsClient.SubscribeTradesChannel(mdMsg.TransactionId, symbol, WsTradeChannelSubscriber.Candles, cancellationToken);

			SendSubscriptionResult(mdMsg);
		}
		else
			await _wsClient.UnsubscribeTradesChannel(mdMsg.TransactionId, symbol, WsTradeChannelSubscriber.Candles, cancellationToken);
	}

	private void ProcessCandle(Candle candle, SecurityId securityId, TimeSpan timeFrame, long originTransId)
	{
		SendOutMessage(new TimeFrameCandleMessage
		{
			SecurityId = securityId,
			TypedArg = timeFrame,
			OpenPrice = (decimal)candle.OpenPrice,
			ClosePrice = (decimal)candle.ClosePrice,
			HighPrice = (decimal)candle.HightPrice,
			LowPrice = (decimal)candle.LowPrice,
			TotalVolume = (decimal)candle.WindowVolume,
			OpenTime = candle.OpenTime,
			State = CandleStates.Finished,
			OriginalTransactionId = originTransId,
		});
	}

	/// <inheritdoc />
	public override async ValueTask SecurityLookupAsync(SecurityLookupMessage lookupMsg, CancellationToken cancellationToken)
	{
		var secTypes = lookupMsg.GetSecurityTypes();
		var left = lookupMsg.Count ?? long.MaxValue;

		var symbols = await _restClient.GetSymbols(cancellationToken);

		foreach (var info in symbols)
		{
			var secMsg = new SecurityMessage
			{
				SecurityId = info.SymbolCode.ToStockSharp(),
				SecurityType = info.Type == "future" ? SecurityTypes.Future : SecurityTypes.CryptoCurrency,
				MinVolume = info.MinAmount,
				Name = info.Name,
				VolumeStep = info.ContractSize,
				OriginalTransactionId = lookupMsg.TransactionId,
				PriceStep = info.TickSize
			};

			if (!secMsg.IsMatch(lookupMsg, secTypes))
				continue;

			SendOutMessage(secMsg);

			if (--left <= 0)
				break;
		}

		SendSubscriptionResult(lookupMsg);
	}
}