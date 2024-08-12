namespace GitScout.Utils;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class WeakReferenceKeyDictionary<TKey, TValue> // : IDictionary<TKey, TValue>
	where TKey : class
{
	byte _seed;
	private Dictionary<WeakReference<TKey>, TValue> _internalDictionary = new Dictionary<WeakReference<TKey>, TValue>(new WeakReferenceComparer<TKey>());

	public void Add(TKey key, TValue value)
	{
		_internalDictionary[new WeakReference<TKey>(key)] = value;
	}

	public bool TryGetValue(TKey key, out TValue value)
	{
		CollectGarbageKeys();
		return _internalDictionary.TryGetValue(new WeakReference<TKey>(key), out value);
	}

	private void CollectGarbageKeys()
	{
		if (unchecked(++_seed) == 0)
		{
			var keysToRemove = new List<WeakReference<TKey>>();
			foreach (var key in _internalDictionary.Keys)
			{
				if (!key.TryGetTarget(out _))
				{
					keysToRemove.Add(key);
				}
			}

			foreach (var key in keysToRemove)
			{
				_internalDictionary.Remove(key);
			}
		}
	}

	private class WeakReferenceComparer<T> : IEqualityComparer<WeakReference<T>> where T : class
	{
		public bool Equals(WeakReference<T> x, WeakReference<T> y)
		{
			T first = null, second = null;

			if (x != null && x.TryGetTarget(out first) && y != null && y.TryGetTarget(out second))
			{
				return EqualityComparer<T>.Default.Equals(first, second);
			}

			return false;
		}

		public int GetHashCode(WeakReference<T> obj)
		{
			T target = null;
			if (obj.TryGetTarget(out target))
			{
				return EqualityComparer<T>.Default.GetHashCode(target);
			}

			return 0;
		}
	}
}

