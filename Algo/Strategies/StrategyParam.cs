namespace StockSharp.Algo.Strategies;

/// <summary>
/// The strategy parameter.
/// </summary>
public interface IStrategyParam : IPersistable, INotifyPropertyChanged, IAttributesEntity
{
	/// <summary>
	/// Parameter identifier.
	/// </summary>
	string Id { get; }

	/// <summary>
	/// The type of the parameter value.
	/// </summary>
	Type Type { get; }

	/// <summary>
	/// The parameter value.
	/// </summary>
	object Value { get; set; }

	/// <summary>
	/// Check can optimize parameter.
	/// </summary>
	bool CanOptimize { get; set; }

	/// <summary>
	/// The From value at optimization.
	/// </summary>
	object OptimizeFrom { get; set; }

	/// <summary>
	/// The To value at optimization.
	/// </summary>
	object OptimizeTo { get; set; }

	/// <summary>
	/// The Increment value at optimization.
	/// </summary>
	object OptimizeStep { get; set; }
}

/// <summary>
/// Wrapper for typified access to the strategy parameter.
/// </summary>
/// <typeparam name="T">The type of the parameter value.</typeparam>
public class StrategyParam<T> : NotifiableObject, IStrategyParam
{
	private readonly IEqualityComparer<T> _comparer;

	/// <summary>
	/// Initializes a new instance of the <see cref="StrategyParam{T}"/>.
	/// </summary>
	/// <param name="id">Parameter identifier.</param>
	/// <param name="initialValue">The initial value.</param>
	public StrategyParam(string id, T initialValue = default)
	{
		if (id.IsEmpty())
			throw new ArgumentNullException(nameof(id));

		Id = id;
		_value = initialValue;

		CanOptimize = typeof(T).CanOptimize();

		_comparer = EqualityComparer<T>.Default;
	}

	/// <inheritdoc />
	public string Id { get; private set; }

	private T _value;

	/// <inheritdoc />
	public T Value
	{
		get => _value;
		set
		{
			if (_comparer.Equals(_value, value))
				return;

			if (!this.IsValid(value))
				throw new ArgumentOutOfRangeException(nameof(value), value, LocalizedStrings.InvalidValue);

			_value = value;
			NotifyChanged();
		}
	}

	Type IStrategyParam.Type => typeof(T);

	object IStrategyParam.Value
	{
		get => Value;
		set => Value = (T)value;
	}

	/// <inheritdoc />
	public bool CanOptimize { get; set; }

	/// <inheritdoc />
	public object OptimizeFrom { get; set; }

	/// <inheritdoc />
	public object OptimizeTo { get; set; }

	/// <inheritdoc />
	public object OptimizeStep { get; set; }

	/// <inheritdoc />
	public IList<Attribute> Attributes { get; } = [];

	/// <summary>
	/// Fill optimization parameters.
	/// </summary>
	/// <param name="optimizeFrom">The From value at optimization.</param>
	/// <param name="optimizeTo">The To value at optimization.</param>
	/// <param name="optimizeStep">The Increment value at optimization.</param>
	/// <returns>The strategy parameter.</returns>
	public StrategyParam<T> SetOptimize(T optimizeFrom = default, T optimizeTo = default, T optimizeStep = default)
	{
		OptimizeFrom = optimizeFrom;
		OptimizeTo = optimizeTo;
		OptimizeStep = optimizeStep;

		return this;
	}

	/// <summary>
	/// Set <see cref="StrategyParam{T}.CanOptimize"/> value.
	/// </summary>
	/// <param name="canOptimize">The value of <see cref="StrategyParam{T}.CanOptimize"/>.</param>
	/// <returns>The strategy parameter.</returns>
	public StrategyParam<T> SetCanOptimize(bool canOptimize)
	{
		CanOptimize = canOptimize;
		return this;
	}

	/// <summary>
	/// Set display settings.
	/// </summary>
	/// <param name="displayName">The display name.</param>
	/// <param name="description">The description of the diagram element parameter.</param>
	/// <param name="category">The category of the diagram element parameter.</param>
	/// <returns><see cref="StrategyParam{T}"/></returns>
	public StrategyParam<T> SetDisplay(string displayName, string description, string category)
		=> ModifyAttributes(true, () => new DisplayAttribute
		{
			Name = displayName,
			Description = description,
			GroupName = category,
		});

	/// <summary>
	/// Set <see cref="BrowsableAttribute"/>.
	/// </summary>
	/// <param name="hidden">Is the parameter hidden in the editor.</param>
	/// <returns><see cref="StrategyParam{T}"/></returns>
	public StrategyParam<T> SetHidden(bool hidden = true)
		=> ModifyAttributes(hidden, () => new BrowsableAttribute(false));

	/// <summary>
	/// Set <see cref="BasicSettingAttribute"/>.
	/// </summary>
	/// <param name="basic">Value.</param>
	/// <returns><see cref="StrategyParam{T}"/></returns>
	public StrategyParam<T> SetBasic(bool basic = true)
		=> ModifyAttributes(basic, () => new BasicSettingAttribute());

	/// <summary>
	/// Set <see cref="ReadOnlyAttribute"/>.
	/// </summary>
	/// <param name="value">Value.</param>
	/// <returns><see cref="StrategyParam{T}"/></returns>
	public StrategyParam<T> SetReadOnly(bool value = true)
		=> ModifyAttributes(value, () => new ReadOnlyAttribute(true));

	private StrategyParam<T> ModifyAttributes<TAttr>(bool add, Func<TAttr> create)
		where TAttr : Attribute
	{
		Attributes.RemoveWhere(a => a is TAttr);

		if (add)
			Attributes.Add(create());

		return this;
	}

	/// <summary>
	/// Load settings.
	/// </summary>
	/// <param name="storage">Settings storage.</param>
	public void Load(SettingsStorage storage)
	{
		Id = storage.GetValue<string>(nameof(Id));

		try
		{
			TValue getValue<TValue>()
				=> storage.GetValue<TValue>(nameof(Value));

			if (typeof(T).Is<Security>())
			{
				var secId = getValue<string>();
				if (!secId.IsEmpty())
					Value = (ServicesRegistry.TrySecurityProvider?.LookupById(secId)).To<T>();
			}
			else if (typeof(T).Is<Portfolio>())
			{
				var pfName = getValue<string>();
				if (!pfName.IsEmpty())
					Value = (ServicesRegistry.TryPortfolioProvider?.LookupByPortfolioName(pfName)).To<T>();
			}
			else
				Value = getValue<T>();
		}
		catch (Exception ex)
		{
			ex.LogError();
		}

		CanOptimize = storage.GetValue(nameof(CanOptimize), CanOptimize);
		OptimizeFrom = storage.GetValue<SettingsStorage>(nameof(OptimizeFrom))?.FromStorage();
		OptimizeTo = storage.GetValue<SettingsStorage>(nameof(OptimizeTo))?.FromStorage();
		OptimizeStep = storage.GetValue<SettingsStorage>(nameof(OptimizeStep))?.FromStorage();
	}

	/// <summary>
	/// Save settings.
	/// </summary>
	/// <param name="storage">Settings storage.</param>
	public void Save(SettingsStorage storage)
	{
		object saveValue()
		{
			var v = Value;

			return v switch
			{
				IPersistable ps => ps.Save(),
				Security s => s.Id,
				Portfolio pf => pf.Name,
				_ => v
			};
		}

		storage
			.Set(nameof(Id), Id)
			.Set(nameof(Value), saveValue())
			.Set(nameof(CanOptimize), CanOptimize)
			.Set(nameof(OptimizeFrom), OptimizeFrom?.ToStorage())
			.Set(nameof(OptimizeTo), OptimizeTo?.ToStorage())
			.Set(nameof(OptimizeStep), OptimizeStep?.ToStorage())
		;
	}

	/// <inheritdoc />
	public override string ToString() => $"{this.GetName()}={Value}";
}