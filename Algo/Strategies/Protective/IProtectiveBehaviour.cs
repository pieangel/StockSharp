﻿namespace StockSharp.Algo.Strategies.Protective;

using System;

using StockSharp.Messages;

/// <summary>
/// Position protection behaviour.
/// </summary>
public interface IProtectiveBehaviour
{
	/// <summary>
	/// Update position difference.
	/// </summary>
	/// <param name="price">Position difference price.</param>
	/// <param name="value">Position difference value.</param>
	/// <returns>Registration order info.</returns>
	(bool isTake, Sides side, decimal price, decimal volume, OrderCondition condition)?
		Update(decimal price, decimal value);

	/// <summary>
	/// Try activate protection.
	/// </summary>
	/// <param name="price">Current price.</param>
	/// <param name="time">Current time.</param>
	/// <returns>Registration order info.</returns>
	(bool isTake, Sides side, decimal price, decimal volume, OrderCondition condition)?
		TryActivate(decimal price, DateTimeOffset time);

	/// <summary>
	/// Clear state.
	/// </summary>
	void Clear();
}

/// <summary>
/// Base implementation of <see cref="IProtectiveBehaviour"/>.
/// </summary>
public abstract class BaseProtectiveBehaviour : IProtectiveBehaviour
{
	/// <summary>
	/// Initializes a new instance of the <see cref="BaseProtectiveBehaviour"/>.
	/// </summary>
	/// <param name="takeValue">Take offset.</param>
	/// <param name="stopValue">Stop offset.</param>
	/// <param name="isTakeTrailing">Whether to use a trailing technique.</param>
	/// <param name="isStopTrailing">Whether to use a trailing technique.</param>
	/// <param name="takeTimeout">Time limit. If protection has not worked by this time, the position will be closed on the market.</param>
	/// <param name="stopTimeout">Time limit. If protection has not worked by this time, the position will be closed on the market.</param>
	/// <param name="useMarketOrders">Whether to use market orders.</param>
	protected BaseProtectiveBehaviour(
		Unit takeValue, Unit stopValue,
		bool isTakeTrailing, bool isStopTrailing,
		TimeSpan takeTimeout, TimeSpan stopTimeout,
		bool useMarketOrders)
    {
		TakeValue = takeValue;
		StopValue = stopValue;
		IsTakeTrailing = isTakeTrailing;
		IsStopTrailing = isStopTrailing;
		TakeTimeout = takeTimeout;
		StopTimeout = stopTimeout;
		UseMarketOrders = useMarketOrders;
	}

	/// <summary>
	/// Take offset.
	/// </summary>
	protected Unit TakeValue { get; }

	/// <summary>
	/// Stop offset.
	/// </summary>
	protected Unit StopValue { get; }

	/// <summary>
	/// Whether to use a trailing technique.
	/// </summary>
	protected bool IsTakeTrailing { get; }

	/// <summary>
	/// Whether to use a trailing technique.
	/// </summary>
	protected bool IsStopTrailing { get; }

	/// <summary>
	/// Time limit. If protection has not worked by this time, the position will be closed on the market.
	/// </summary>
	protected TimeSpan TakeTimeout { get; }

	/// <summary>
	/// Time limit. If protection has not worked by this time, the position will be closed on the market.
	/// </summary>
	protected TimeSpan StopTimeout { get; }

	/// <summary>
	/// Whether to use market orders.
	/// </summary>
	protected bool UseMarketOrders { get; }

	/// <inheritdoc />
	public abstract (bool isTake, Sides side, decimal price, decimal volume, OrderCondition condition)? Update(decimal price, decimal value);

	/// <inheritdoc />
	public abstract (bool, Sides, decimal, decimal, OrderCondition)? TryActivate(decimal price, DateTimeOffset time);
	
	/// <inheritdoc />
	public abstract void Clear();
}