using RestSharp;
using StockSharp.DarkHorse.Native.Model;

namespace StockSharp.DarkHorse;

partial class DarkHorseMessageAdapter
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
    protected override ValueTask OnLevel1SubscriptionAsync(MarketDataMessage mdMsg, CancellationToken cancellationToken)
    {
        SendSubscriptionReply(mdMsg.TransactionId);

        var currency = mdMsg.SecurityId.ToCurrency();

        if (mdMsg.IsSubscribe)
            return _wsClient.SubscribeLevel1(currency, cancellationToken);
        else
            return _wsClient.UnsubscribeLevel1(currency, cancellationToken);
    }

    /// <inheritdoc />
    protected override ValueTask OnMarketDepthSubscriptionAsync(MarketDataMessage mdMsg, CancellationToken cancellationToken)
    {
        SendSubscriptionReply(mdMsg.TransactionId);

        var currency = mdMsg.SecurityId.ToCurrency();

        if (mdMsg.IsSubscribe)
            return _wsClient.SubscribeOrderBook(currency, cancellationToken);
        else
            return _wsClient.UnsubscribeOrderBook(currency, cancellationToken);
    }

    /// <inheritdoc />
    protected override async ValueTask OnTicksSubscriptionAsync(MarketDataMessage mdMsg, CancellationToken cancellationToken)
    {
        SendSubscriptionReply(mdMsg.TransactionId);

        var currency = mdMsg.SecurityId.ToCurrency();

        if (mdMsg.IsSubscribe)
        {
            if (mdMsg.From is not null)
            {
                var startTime = mdMsg.From.Value.UtcDateTime;
                var endTime = mdMsg.To?.UtcDateTime ?? DateTime.UtcNow;
                var left = mdMsg.Count ?? long.MaxValue;

                while (startTime < endTime)
                {
                    var trades = await _restClient.GetMarketTrades(currency, startTime, endTime, cancellationToken);

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
                await _wsClient.SubscribeTradesChannel(currency, WsTradeChannelSubscriber.Trade, cancellationToken);

            SendSubscriptionResult(mdMsg);
        }
        else
        {
            await _wsClient.UnsubscribeTradesChannel(currency, WsTradeChannelSubscriber.Trade, cancellationToken);
        }
    }

    /// <inheritdoc />
    protected override async ValueTask OnTFCandlesSubscriptionAsync(MarketDataMessage mdMsg, CancellationToken cancellationToken)
    {
        SendSubscriptionReply(mdMsg.TransactionId);

        var currency = mdMsg.SecurityId.ToCurrency();

        if (mdMsg.IsSubscribe)
        {
            if (mdMsg.From is not null)
            {
                var startTime = mdMsg.From.Value.UtcDateTime;
                var endTime = mdMsg.To?.UtcDateTime ?? DateTime.UtcNow;
                var left = mdMsg.Count ?? long.MaxValue;

                var resolution = (TimeSpan)mdMsg.DataType2.Arg;

                while (startTime < endTime)
                {
                    var candles = await _restClient.GetMarketCandles(currency, resolution, startTime, endTime, cancellationToken);

                    var lastTime = startTime;

                    foreach (var candle in candles.OrderBy(t => t.OpenTime))
                    {
                        if (candle.OpenTime < startTime)
                            continue;

                        if (candle.OpenTime > endTime)
                            break;

                        SendOutMessage(new TimeFrameCandleMessage
                        {
                            OriginalTransactionId = mdMsg.TransactionId,
                            ClosePrice = candle.ClosePrice,
                            HighPrice = candle.HightPrice,
                            LowPrice = candle.LowPrice,
                            OpenPrice = candle.OpenPrice,
                            TotalVolume = candle.WindowVolume,
                            OpenTime = candle.OpenTime,
                            State = CandleStates.Finished,
                        });

                        if (--left <= 0)
                            break;

                        lastTime = candle.OpenTime;
                    }

                    if (candles.Count != _candlesPaginationLimit || --left <= 0)
                        break;

                    startTime = lastTime;
                }
            }

            if (!mdMsg.IsHistoryOnly())
                await _wsClient.SubscribeTradesChannel(currency, WsTradeChannelSubscriber.Candles, cancellationToken);

            SendSubscriptionResult(mdMsg);
        }
        else
        {
            await _wsClient.UnsubscribeTradesChannel(currency, WsTradeChannelSubscriber.Candles, cancellationToken);
        }
    }

    /// <inheritdoc />
    public override async ValueTask SecurityLookupAsync(SecurityLookupMessage lookupMsg, CancellationToken cancellationToken)
    {
        var secTypes = lookupMsg.GetSecurityTypes();
        var left = lookupMsg.Count ?? long.MaxValue;
       
        var symbols = await _restClient.GetSymbols(cancellationToken);

        foreach (var symbol in symbols)
        {
            var secMsg = new SecurityMessage
            {
                SecurityId = symbol.SymbolCode.ToStockSharpFromSymbol(),
                SecurityType = symbol.Type == "future" ? SecurityTypes.Future : SecurityTypes.CryptoCurrency,
                Decimals = int.Parse(symbol.Decimal),
                MinVolume = decimal.Parse(symbol.tickSize) * (decimal)Math.Pow(10, int.Parse(symbol.Decimal)),
                Name = symbol.Name,
                VolumeStep = decimal.Parse(symbol.tickSize) * (decimal)Math.Pow(10, int.Parse(symbol.Decimal)),
                OriginalTransactionId = lookupMsg.TransactionId,
                PriceStep = decimal.Parse(symbol.tickSize)
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