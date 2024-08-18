namespace GitScout.Utils;

using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Security;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public class TrackableObservableCollection<T> : ObservableCollection<T>
{
	public TrackableObservableCollection()
	{
			
	}

	public TrackableObservableCollection(IEnumerable<T> source)
		: base(source)
	{

	}

	public int Subscribers { get; set; }

	public override event NotifyCollectionChangedEventHandler? CollectionChanged
	{
		add { base.CollectionChanged += value; Subscribers++; }
		remove { base.CollectionChanged -= value; Subscribers--; }
	}
}

internal class Maintainer
{
	public static Maintainer Instance = new Maintainer();

	private WeakHashSet<IMaintainable> _maintanables = new WeakHashSet<IMaintainable>();
	private System.Threading.Timer _timer;

	private SortedDictionary<IMaintainable, object?> _maintanablesScheduled = new SortedDictionary<IMaintainable, object?>();
	private System.Threading.Timer _timerSchedule;
	private bool _timerScheduleSet;

	private Maintainer()
	{
		// now when Instance is created, we should re-register this _maintanables that was missed inside WeakHashSet initializer
		Add(_maintanables);
		_timer = new System.Threading.Timer(TimerElapsed, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
		_timerSchedule = new System.Threading.Timer(TimerElapsedSchedule);
	}

	private void TimerElapsed(object? state)
	{
		lock (_maintanables)
		{
			foreach (var kvp in _maintanables)
			{
				try
				{
					kvp.Maintenance();
				}
				catch (Exception ex)
				{
					Trace.TraceError(ex.ToString());
				}
			}
		}
	}

	private void TimerElapsedSchedule(object? state)
	{
		lock (_timerSchedule)
		{
			_timerScheduleSet = false;
		}
		IMaintainable[] array;
		lock (_maintanablesScheduled)
		{
			array = _maintanablesScheduled.Keys.ToArray();
			_maintanablesScheduled.Clear();
		}
		foreach (var maintainable in array)
		{
			try
			{
				maintainable.Maintenance();
			}
			catch (Exception ex)
			{
				Trace.TraceError(ex.ToString());
			}
		}
	}

	public void Add(IMaintainable maintanable)
	{
		lock (_maintanables)
		{
			_maintanables.Add(maintanable);
		}
	}

	internal void Schedule(IMaintainable maintanable)
	{
		lock (_timerSchedule)
		{
			if (!_timerScheduleSet)
			{
				_timerScheduleSet = true;
				_timerSchedule.Change(100, Timeout.Infinite);
			}
		}
		lock (_maintanablesScheduled)
		{
			_maintanablesScheduled.Add(maintanable, null);
		}
	}
}

public interface IMaintainable
{
	public void Maintenance();
}

public class WeakHashSet<TKey> : WeakKeyDictionary<TKey, object?>, IEnumerable<TKey>
	where TKey : class
{
	public void Add(TKey key)
	{
		Add(key, null);
	}

	public new IEnumerator<TKey> GetEnumerator() => InternalKeys.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => InternalKeys.GetEnumerator();
}

public class WeakKeyDictionary2<TKey, TValue>
	where TKey : class
	where TValue : class

{
	public WeakKeyDictionary2()
	{
			
	}

	private ConditionalWeakTable<TKey, TValue?> _weakTable = new ConditionalWeakTable<TKey, TValue?>();

	public TValue? GetOrAdd(TKey key, Func<TKey, TValue?> valueFactory)
	{
		return _weakTable.GetValue(key, key => valueFactory(key));
	}

	public TValue? this[TKey key]
	{
		get => _weakTable.TryGetValue(key, out var value) ? value : null;
		set => _weakTable.AddOrUpdate(key, value);
	}

	public int Count => _weakTable.Count();
}

[Obsolete("Use WeakKeyDictionary2")]
public class WeakKeyDictionary<TKey, TValue> : IMaintainable
	// , IDictionary<<EquatableWeakReference, TValue>
	, IDictionary<TKey, TValue?>
	, ICollection<KeyValuePair<TKey, TValue?>>
	, IEnumerable<KeyValuePair<TKey, TValue?>>
	, IEnumerable
	where TKey : class
{
	byte _seed;
	private ConcurrentDictionary<EquatableWeakReference, TValue?> _internalDictionary = new ConcurrentDictionary<EquatableWeakReference, TValue?>(WeakKeyComparer.Comparer);

	public WeakKeyDictionary()
	{
		Maintainer.Instance?.Add(this); // it is null while Maintainer static ctor is executing (field initializing). So Maintainer have to re-register itself
	}

	public void Add(TKey key, TValue? value)
	{
		TryCollectGarbageKeys();
		_internalDictionary[new EquatableWeakReference(key)] = value;
	}

	public bool TryGetValue(TKey key, out TValue? value)
	{
		TryCollectGarbageKeys();
		return _internalDictionary.TryGetValue(new EquatableWeakReference(key), out value);
	}

	public TValue? GetOrAdd(TKey key, Func<TKey, TValue?> valueFactory)
	{
		TryCollectGarbageKeys();
		var wkey = new EquatableWeakReference(key);
		return _internalDictionary.GetOrAdd(wkey, _ => valueFactory(key));
	}


	public TValue? GetOrAdd<TArg>(TKey key, Func<TKey, TArg, TValue?> valueFactory, TArg arg)
	{
		TryCollectGarbageKeys();
		var wkey = new EquatableWeakReference(key);
		return _internalDictionary.GetOrAdd(wkey, (_, a) => valueFactory(key, a), arg);
	}

	internal IEnumerable<TKey> InternalKeys =>
		_internalDictionary.Keys
			.Select(x => (TKey?)x.Target)
			.Where(x => x is not null)!;

	#region IDictionary<TKey, TValue?>

	ICollection<TKey> IDictionary<TKey, TValue?>.Keys => InternalKeys.ToArray();

	ICollection<TValue?> IDictionary<TKey, TValue?>.Values => _internalDictionary.Values;

	public TValue? this[TKey key]
	{
		get
		{
			return _internalDictionary[new EquatableWeakReference(key)];
		}
		set
		{
			_internalDictionary[new EquatableWeakReference(key)] = value;
		}
	}

	public bool ContainsKey(TKey key) => _internalDictionary.ContainsKey((new EquatableWeakReference(key)));
	public bool Remove(TKey key) => _internalDictionary.TryRemove(new EquatableWeakReference(key), out var _);

	#endregion

	#region ICollection<T>

	void ICollection<KeyValuePair<TKey, TValue?>>.Add(KeyValuePair<TKey, TValue?> item) => Add(item.Key, item.Value);
	bool ICollection<KeyValuePair<TKey, TValue?>>.Contains(KeyValuePair<TKey, TValue?> item) => throw new NotSupportedException("Check by key instead");
	void ICollection<KeyValuePair<TKey, TValue?>>.CopyTo(KeyValuePair<TKey, TValue?>[] array, int arrayIndex) => throw new NotImplementedException();
	public int Count => _internalDictionary.Count;
	bool ICollection<KeyValuePair<TKey, TValue?>>.IsReadOnly => false;
	bool ICollection<KeyValuePair<TKey, TValue?>>.Remove(KeyValuePair<TKey, TValue?> item) => throw new NotSupportedException("Remove by key instead");
	public void Clear()
	{
		_internalDictionary.Clear();
		_seed = 0;
	}

	#endregion

	#region IEnumerable<T>

	public IEnumerator<KeyValuePair<TKey, TValue?>> GetEnumerator() =>
		_internalDictionary
			.Select(kvp => new { kvp, Target = (TKey?)kvp.Key.Target })
			.Where(x => x.Target is not null)
			.Select(x => new KeyValuePair<TKey, TValue?>(x.Target!, x.kvp.Value))
			.GetEnumerator();

	#endregion

	#region IEnumerable

	IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<KeyValuePair<TKey, TValue?>>)this).GetEnumerator();

	#endregion

	private void TryCollectGarbageKeys()
	{
		if (unchecked(++_seed) == 0)
		{
			CollectGarbageKeys();
		}
	}

	private void CollectGarbageKeys()
	{
		List<EquatableWeakReference>? keysToRemove = null;
		foreach (var key in _internalDictionary.Keys)
		{
			if (!key.IsAlive)
			{
				keysToRemove ??= new(8);
				keysToRemove.Add(key);
			}
		}

		if (keysToRemove != null)
		{
			foreach (var key in keysToRemove)
			{
				_internalDictionary.TryRemove(key, out _);
			}
		}
	}

	void IMaintainable.Maintenance()
	{
		CollectGarbageKeys();
	}

}

class WeakKeyComparer : IEqualityComparer<EquatableWeakReference>
{
	public static WeakKeyComparer Comparer { get; } = new WeakKeyComparer();

	private WeakKeyComparer()
	{
	}

	bool IEqualityComparer<EquatableWeakReference>.Equals(EquatableWeakReference? x, EquatableWeakReference? y)
	{
		if (ReferenceEquals(x, y))
		{
			return true;
		}
		if (ReferenceEquals(x, null))
		{
			return ReferenceEquals(y, null);
		}
		if (ReferenceEquals(y, null))
		{
			return false;
		}
		if (x.GetHashCode() != y.GetHashCode())
		{
			return false;
		}
		var target = x.Target;
		var target2 = y.Target;
		if (ReferenceEquals(target, null))
		{
			return ReferenceEquals(target2, null);
		}
		if (ReferenceEquals(target2, null))
		{
			return false;
		}
		return Equals(target, target2);
	}

	public int GetHashCode([DisallowNull] EquatableWeakReference equatableWeakReference)
	{
		return equatableWeakReference.GetHashCode();
	}
}

// It is not generic WeakReference to simplify comparison both with Target and EquatableWeakReference via same simple method Equals(object)
// Also for cleaner good old .Target syntax
class EquatableWeakReference : WeakReference, IEquatable<EquatableWeakReference>
{
	private readonly int _hashCode;

	public EquatableWeakReference(object target)
		: base(target)
	{
		_hashCode = target.GetHashCode();
	}

	/*
	~EquatableWeakReference()
	{
		Maintainer.Instance.Schedule(_maintanable);
	}
	*/

	bool IEquatable<EquatableWeakReference>.Equals(EquatableWeakReference? other)
	{
		return Equals(other);
	}

	public override bool Equals(object? other)
	{
		if (ReferenceEquals(other, null))
		{
			return false;
		}
		if (other.GetHashCode() != _hashCode)
		{
			return false;
		}
		if (ReferenceEquals(other, this))
		{
			return true;
		}
		return ReferenceEquals(other, Target);
	}

	public override int GetHashCode()
	{
		return _hashCode;
	}
}

public class WeakValueDictionary<TKey, TValue>
	where TValue : class

{
	class Controller
	{
		private readonly IDictionary _dic;
		private readonly TKey _key;

		public Controller(IDictionary dic, TKey key)
		{
			_dic = dic;
			_key = key;
		}

		~Controller()
		{
			lock (_dic)
			{
				_dic.Remove(_key);
			}
		}

		public void Release()
		{
			GC.SuppressFinalize(this);
		}
	}

	Dictionary<TKey, WeakReference<TValue>> _dictionary = new Dictionary<TKey, WeakReference<TValue>>();
	private readonly ConditionalWeakTable<TValue, Controller> _controllers = new ConditionalWeakTable<TValue, Controller>();

	public TValue GetOrAdd(TKey key, Func<TKey, TValue> factory)
	{
		lock (_dictionary)
		{
			ref var valuePlaceholder = ref CollectionsMarshal.GetValueRefOrAddDefault(_dictionary, key, out bool exists);
			TValue? targetValue = null;
			var isAlive = valuePlaceholder?.TryGetTarget(out targetValue) ?? false;
			if (!exists || !isAlive || valuePlaceholder == null || targetValue == null)
			{
				var value = factory(key);
				valuePlaceholder = new WeakReference<TValue>(value); // assigning ref local also inherently updates dictionary
				_controllers.Add(value, new Controller(_dictionary, key));
				return value;
			}
			else
			{
				return targetValue;
			}
		}
	}

	public TValue? this[TKey key]
	{
		get
		{
			ref var valuePlaceholder = ref CollectionsMarshal.GetValueRefOrAddDefault(_dictionary, key, out bool exists);
			TValue? targetValue = null;
			valuePlaceholder?.TryGetTarget(out targetValue);
			return targetValue;
		}
		set
		{
			_dictionary[key] = new WeakReference<TValue>(value);
			_controllers.TryGetValue(value, out var controller);
			if (controller != null)
			{
				controller.Release(); // we should make sure that previus controller is disengaged, otherwise it will kill new entry
			}
			_controllers.AddOrUpdate(value, new Controller(_dictionary, key));
		}
	}

	public int Count => _dictionary.Count;
}