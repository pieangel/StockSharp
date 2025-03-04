using StockSharp.Messages;
using Ecng.Common;
namespace StockSharp.SignalMaster;

partial class SignalMasterMessageAdapter
{
	private class RequestIdGenerator : Ecng.Common.IdGenerator
	{
		private long _currentId;

		public RequestIdGenerator()
		{
			_currentId = 1;
		}

		public override long GetNextId()
		{
			return Interlocked.Increment(ref _currentId);
		}
	}
	private RequestIdGenerator _requestIdGenerator = new RequestIdGenerator();
	private readonly TimeSpan _orderHistoryInterval = TimeSpan.FromDays(7);
	private const int _fillsPaginationLimit = 100;

	private long? _portfolioLookupSubMessageTransactionID;
	private bool _isOrderSubscribed;



	private string PortfolioName => nameof(SignalMaster) + "_" + Key.ToId().To<string>();

	private void SessionOnNewFill(Fill fill)
	{
		SendOutMessage(new ExecutionMessage
		{
			DataTypeEx = DataType.Transactions,
			PortfolioName = GetPortfolioName(),
			TradeId = fill.TradeId,
			TradePrice = fill.Price,
			TradeVolume = fill.Size,
			SecurityId = fill.Market.ToStockSharp(),
			OrderId = fill.OrderId,
			Side = fill.Side.ToSide(),
			ServerTime = fill.Time
		});
	}

	private void SendProcessOrderStatusResult(Order order, OrderMessage message)
	{
		if (!long.TryParse(order.ClientId, out var transId))
			return;

		SendOutMessage(new ExecutionMessage
		{
			OriginalTransactionId = message.TransactionId,
			TransactionId = transId,
			OrderId = order.Id,
			DataTypeEx = DataType.Transactions,
			PortfolioName = GetPortfolioName(),
			HasOrderInfo = true,
			ServerTime = order.CreatedAt ?? CurrentTime.ConvertToUtc(),
			OrderState = order.Status.ToOrderState(),
			OrderType = order.Type.ToOrderType(),
			AveragePrice = order.AvgFillPrice,
			OrderPrice = order.Price ?? (order.AvgFillPrice ?? 0),
			SecurityId = order.Market.ToStockSharp(),
			Side = order.Side.ToSide(),
			OrderVolume = order.Size,
			Balance = order.Size - order.FilledSize,

		});
	}
	private void SessionOnNewOrder(Order order)
	{
		if (!long.TryParse(order.ClientId, out var transId))
			return;

		var message = new ExecutionMessage
		{
			OriginalTransactionId = transId,
			TransactionId = transId,
			DataTypeEx = DataType.Transactions,
			PortfolioName = GetPortfolioName(),
			HasOrderInfo = true,
			ServerTime = order.CreatedAt ?? CurrentTime.ConvertToUtc(),
			OrderId = order.Id,
			OrderState = order.Status.ToOrderState(),
			OrderType = order.Type.ToOrderType(),
			AveragePrice = order.AvgFillPrice,
			OrderPrice = order.Price ?? (order.AvgFillPrice ?? 0),
			SecurityId = order.Market.ToStockSharp(),
			Side = order.Side.ToSide(),
			OrderVolume = order.Size,
			Balance = order.Size - order.FilledSize,
		};

		SendOutMessage(message);
	}

	/// <inheritdoc />
	public override async ValueTask RegisterOrderAsync(OrderRegisterMessage regMsg, CancellationToken cancellationToken)
	{
		switch (regMsg.OrderType)
		{
			case OrderTypes.Limit:
			case OrderTypes.Market:
				break;
			default:
				throw new NotSupportedException(LocalizedStrings.OrderUnsupportedType.Put(regMsg.OrderType, regMsg.TransactionId));
		}

		var price = regMsg.OrderType == OrderTypes.Market ? (decimal?)null : regMsg.Price;

		var order = await _restClient.RegisterOrder(regMsg.SecurityId.ToSymbol(), regMsg.Side, price, regMsg.OrderType.Value, regMsg.Volume, regMsg.TransactionId.To<string>(), SubaccountName, cancellationToken)
			?? throw new InvalidOperationException(LocalizedStrings.OrderNoExchangeId.Put(regMsg.TransactionId));

		SendProcessOrderStatusResult(order, regMsg);
	}

	/// <inheritdoc />
	public override async ValueTask CancelOrderAsync(OrderCancelMessage cancelMsg, CancellationToken cancellationToken)
	{
		if (cancelMsg.OrderId == null)
			throw new InvalidOperationException(LocalizedStrings.OrderNoExchangeId.Put(cancelMsg.OriginalTransactionId));

		if (!await _restClient.CancelOrder(cancelMsg.OrderId.Value, SubaccountName, cancellationToken))
		{
			throw new InvalidOperationException(LocalizedStrings.OrderNoExchangeId.Put(cancelMsg.OriginalTransactionId));
		}
	}

	/// <inheritdoc />
	public override async ValueTask CancelOrderGroupAsync(OrderGroupCancelMessage cancelMsg, CancellationToken cancellationToken)
	{
		if (!await _restClient.CancelAllOrders(SubaccountName, cancellationToken))
		{
			throw new InvalidOperationException(LocalizedStrings.OrderNoExchangeId.Put(cancelMsg.OriginalTransactionId));
		}
	}

	/// <inheritdoc />
	public override async ValueTask OrderStatusAsync(OrderStatusMessage statusMsg, CancellationToken cancellationToken)
	{
		if (statusMsg != null)
		{
			SendSubscriptionReply(statusMsg.TransactionId);
			_isOrderSubscribed = statusMsg.IsSubscribe;
		}

		if (!_isOrderSubscribed)
		{
			return;
		}

		var orders = await _restClient.GetOpenOrders(SubaccountName, cancellationToken);

		if (statusMsg == null)
		{
			if (orders != null && orders.Count > 0)
			{
				foreach (var order in orders)
					SessionOnNewOrder(order);

				var start = orders.Where(x => x.CreatedAt != null).Min(x => x.CreatedAt.Value);
				var fills = await _restClient.GetFills(start, DateTime.UtcNow, SubaccountName, cancellationToken);

				foreach (var fill in fills)
					SessionOnNewFill(fill);
			}
		}
		else
		{
			if (!statusMsg.IsSubscribe)
			{
				return;
			}

			var fromTime = DateTime.UtcNow - _orderHistoryInterval;

			do
			{
				var prevFromTime = fromTime;

				var (histOrders, hasMoreData) = await _restClient.GetMarketOrderHistoryAndHasMoreOrders(SubaccountName, fromTime, cancellationToken);

				if (!histOrders.Any())
					break;

				foreach (var order in histOrders.Where(o => o.ClientId != null).OrderBy(o => o.CreatedAt))
				{
					if (order.CreatedAt > fromTime)
						fromTime = order.CreatedAt.Value;

					if (order.ClientId != null)
					SendProcessOrderStatusResult(order, statusMsg);
				}

				if (!hasMoreData || fromTime <= prevFromTime)
					break;
			}
			while (true);

			var now = DateTime.UtcNow;

			var startTime = now - _orderHistoryInterval;
			var endTime = (now - startTime) < _orderHistoryInterval ? (startTime + (now - startTime)) : startTime + _orderHistoryInterval;

			while (startTime < endTime)
			{
				var fills = await _restClient.GetFills(startTime, endTime, SubaccountName, cancellationToken);

				var lastTime = startTime;

				foreach (var fill in fills.OrderBy(f => f.Time))
				{
					if (fill.Time < startTime)
						continue;

					if (fill.Time > endTime)
						break;

					SessionOnNewFill(fill);

					lastTime = fill.Time;
				}

				if (fills.Count != _fillsPaginationLimit)
					break;

				startTime = lastTime;
			}

			SendSubscriptionResult(statusMsg);
		}
	}

	/// <inheritdoc />
	public override async ValueTask PortfolioLookupAsync(PortfolioLookupMessage lookupMsg, CancellationToken cancellationToken)
	{
		if (lookupMsg != null)
		{
			SendSubscriptionReply(lookupMsg.TransactionId);

			_portfolioLookupSubMessageTransactionID = lookupMsg.IsSubscribe ? lookupMsg.TransactionId : null;

			if (!lookupMsg.IsSubscribe)
			{
				return;
			}
		}

		if (_portfolioLookupSubMessageTransactionID == null)
		{
			return;
		}


		var accounts = await _restClient.GetAccounts(cancellationToken);
		foreach (var account in accounts)
		{
			string portfolioName = account.AccountCode;
			// ✅ Step 1: Send Portfolio Info
			SendOutMessage(new PortfolioMessage
			{
				PortfolioName = portfolioName,
				BoardCode = BoardCodes.SignalMaster,
				OriginalTransactionId = (long)_portfolioLookupSubMessageTransactionID,
			});

			// ✅ Step 2: Get account balance from the broker
			/* 나중에 이 부분을 풀고 반드시 구현할 것
			var balance = await _restClient.GetAccountBalance(_requestIdGenerator.GetNextId(), portfolioName, DateTime.Now, DateTime.Now, cancellationToken);
			if (balance != null)
			{
				var msg = this.CreatePositionChangeMessage(portfolioName, new SecurityId
				{
					SecurityCode = account.Currency,
					BoardCode = BoardCodes.Coinbase,
				});
				msg.TryAdd(PositionChangeTypes.CurrentValue, balance.Balance, true);
				msg.TryAdd(PositionChangeTypes.BlockedValue, balance.Total - balance.Balance, true);
				if (_portfolioLookupSubMessageTransactionID != null)
					msg.OriginalTransactionId = (long)_portfolioLookupSubMessageTransactionID;
				// ✅ Step 3: Send account balance to StockSharp
				SendOutMessage(msg);
			}

			var positions = await _restClient.GetPositions(portfolioName, cancellationToken);

			foreach (var position in positions)
			{
				var msg = this.CreatePositionChangeMessage(portfolioName, position.Name.ToStockSharp());
				msg.TryAdd(PositionChangeTypes.CurrentValue, position.Cost, true);
				msg.TryAdd(PositionChangeTypes.UnrealizedPnL, position.UnrealizedPnl, true);
				msg.TryAdd(PositionChangeTypes.AveragePrice, position.EntryPrice, true);
				if (_portfolioLookupSubMessageTransactionID != null) msg.OriginalTransactionId = (long)_portfolioLookupSubMessageTransactionID;
				// ✅ Step 4: Get open positions for this account
				SendOutMessage(msg);
			}
			*/
		}

		if (lookupMsg != null)
		{
			SendSubscriptionResult(lookupMsg);
		}
	}

	private string GetPortfolioName()
	{
		return SubaccountName.IsEmpty(PortfolioName);
	}
}
