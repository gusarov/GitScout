namespace GitScout.Utils;

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Timers;

internal class Maintainer
{
    public static Maintainer Instance = new Maintainer();

    private WeakReferenceHashSet<IMaintanable> _maintanables = new WeakReferenceHashSet<IMaintanable>();
    private Timer _timer;

    private Maintainer()
    {
        // now when Instance is created, we should re-register this _maintanables
        Add(_maintanables);
        _timer = new Timer(
#if DEBUG
            TimeSpan.FromSeconds(2)
#else
            TimeSpan.FromMinutes(1)
#endif
            );
        _timer.Elapsed += TimerElapsed;
        _timer.Enabled = true;
    }

    private void TimerElapsed(object? sender, ElapsedEventArgs e)
    {
        _timer.Enabled = false;
        try
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
        finally
        {
            _timer.Enabled = true;
        }
    }

    public void Add(IMaintanable maintanable)
    {
        lock (_maintanables)
        {
            _maintanables.Add(maintanable);
        }
    }
}

public interface IMaintanable
{
    public void Maintenance();
}

public class WeakReferenceHashSet<TKey> : WeakReferenceKeyDictionary<TKey, object?>, IEnumerable<TKey>
    where TKey : class
{
    public void Add(TKey key)
    {
        Add(key, null);
    }

    public new IEnumerator<TKey> GetEnumerator() => InternalKeys.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => InternalKeys.GetEnumerator();
}

public class WeakReferenceKeyDictionary<TKey, TValue> : IMaintanable
    // , IDictionary<<WeakReference<TKey>, TValue>
    , IDictionary<TKey, TValue?>
    , ICollection<KeyValuePair<TKey, TValue?>>
    , IEnumerable<KeyValuePair<TKey, TValue?>>
    , IEnumerable
    where TKey : class
{
    byte _seed;
    private ConcurrentDictionary<WeakReference<TKey>, TValue?> _internalDictionary = new ConcurrentDictionary<WeakReference<TKey>, TValue?>(new WeakReferenceComparer<TKey>());

    public WeakReferenceKeyDictionary()
    {
        Maintainer.Instance?.Add(this); // it is null while Maintainer static ctor is executing (field initializing)
    }

    public void Add(TKey key, TValue? value)
    {
        TryCollectGarbageKeys();
        _internalDictionary[new WeakReference<TKey>(key)] = value;
    }

    public bool TryGetValue(TKey key, out TValue? value)
    {
        TryCollectGarbageKeys();
        return _internalDictionary.TryGetValue(new WeakReference<TKey>(key), out value);
    }

    public TValue GetOrAdd<TArg>(TKey key, Func<TKey, TValue> valueFactory)
    {
        TryCollectGarbageKeys();
        var wkey = new WeakReference<TKey>(key);
        return _internalDictionary.GetOrAdd(wkey, _ => valueFactory(key));
    }


    public TValue GetOrAdd<TArg>(TKey key, Func<TKey, TArg, TValue> valueFactory, TArg arg)
    {
        TryCollectGarbageKeys();
        var wkey = new WeakReference<TKey>(key);
        return _internalDictionary.GetOrAdd(wkey, (_, a) => valueFactory(key, a), arg);
    }

    internal IEnumerable<TKey> InternalKeys =>
        _internalDictionary.Keys
            .Select(x => x.TryGetTarget(out var target) ? target : null)
            .Where(x => x is not null)!;

    #region IDictionary<TKey, TValue?>

    ICollection<TKey> IDictionary<TKey, TValue?>.Keys => _internalDictionary.Keys
        .Select(x => x.TryGetTarget(out var target) ? target : null)
        .Where(x => x is not null)
        .ToArray()!;

    ICollection<TValue?> IDictionary<TKey, TValue?>.Values => _internalDictionary.Values;

    public TValue? this[TKey key]
    {
        get
        {
            return _internalDictionary[new WeakReference<TKey>(key)];
        }
        set
        {
            _internalDictionary[new WeakReference<TKey>(key)] = value;
        }
    }

    public bool ContainsKey(TKey key) => _internalDictionary.ContainsKey((new WeakReference<TKey>(key)));
    public bool Remove(TKey key) => _internalDictionary.TryRemove(new WeakReference<TKey>(key), out var _);

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
            .Select(kvp => kvp.Key.TryGetTarget(out var target) ? new { kvp, target } : new { kvp, target = default(TKey?) }!)
            .Where(x => x.target is not null)
            .Select(x => new KeyValuePair<TKey, TValue?>(x.target, x.kvp.Value))
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
        List<WeakReference<TKey>>? keysToRemove = null;
        foreach (var key in _internalDictionary.Keys)
        {
            if (!key.TryGetTarget(out _))
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

    void IMaintanable.Maintenance()
    {
        CollectGarbageKeys();
    }

    private class WeakReferenceComparer<T> : IEqualityComparer<WeakReference<T>> where T : class
	{
        public WeakReferenceComparer()
        {
        }

        public bool Equals(WeakReference<T>? x, WeakReference<T>? y)
		{
			if (x != null && x.TryGetTarget(out var first) && y != null && y.TryGetTarget(out var second))
			{
				return EqualityComparer<T>.Default.Equals(first, second);
			}

			return false;
		}

		public int GetHashCode([DisallowNull] WeakReference<T> obj)
		{
			if (obj.TryGetTarget(out var target))
			{
				return EqualityComparer<T>.Default.GetHashCode(target);
			}

			return 0;
		}
	}
}

