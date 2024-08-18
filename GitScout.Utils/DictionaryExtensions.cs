using System.Runtime.InteropServices;

namespace GitScout;

public static class DictionaryExtensions
{
	public static TValue? GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue?> dic, TKey key, Func<TKey, TValue> factory)
	{
		ref var value = ref CollectionsMarshal.GetValueRefOrAddDefault(dic, key, out bool exist);
		if (!exist)
		{
			value = factory(key);
		}
		return value;
	}
}

