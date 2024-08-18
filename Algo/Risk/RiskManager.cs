namespace StockSharp.Algo.Risk;

using System.Collections.Generic;
using System.Linq;

using Ecng.Collections;
using Ecng.Serialization;

using StockSharp.Logging;
using StockSharp.Messages;

/// <summary>
/// The risks control manager.
/// </summary>
public class RiskManager : BaseLogReceiver, IRiskManager
{
	/// <summary>
	/// Initializes a new instance of the <see cref="RiskManager"/>.
	/// </summary>
	public RiskManager()
	{
		_rules.Added += r => r.Parent = this;
		_rules.Removed += r => r.Parent = null;
		_rules.Inserted += (i, r) => r.Parent = this;
		_rules.Clearing += () =>
		{
			_rules.Cache.ForEach(r => r.Parent = null);
			return true;
		};
	}

	private readonly CachedSynchronizedList<IRiskRule> _rules = new();

	/// <inheritdoc />
	public INotifyList<IRiskRule> Rules => _rules;

	/// <inheritdoc />
	public virtual void Reset()
	{
		_rules.Cache.ForEach(r => r.Reset());
	}

	/// <inheritdoc />
	public IEnumerable<IRiskRule> ProcessRules(Message message)
	{
		if (message.Type == MessageTypes.Reset)
		{
			Reset();
			return Enumerable.Empty<IRiskRule>();
		}

		return _rules.Cache.Where(r => r.ProcessMessage(message)).ToArray();
	}

	/// <inheritdoc />
	public override void Load(SettingsStorage storage)
	{
		Rules.Clear();
		Rules.AddRange(storage.GetValue<SettingsStorage[]>(nameof(Rules)).Select(s => s.LoadEntire<IRiskRule>()));

		base.Load(storage);
	}

	/// <inheritdoc />
	public override void Save(SettingsStorage storage)
	{
		storage.SetValue(nameof(Rules), Rules.Select(r => r.SaveEntire(false)).ToArray());

		base.Save(storage);
	}
}