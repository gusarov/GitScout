namespace GitScout.Utils;

public static class DelegatedComparer
{
	public static DelegatedComparer<T> With<T>(Func<T?, T?, int> comparer)
	{
		return new DelegatedComparer<T>(comparer);
	}
}

public class DelegatedComparer<T> : IComparer<T>
{
	private readonly Func<T?, T?, int> _comparer;

	public DelegatedComparer(Func<T?, T?, int> comparer)
	{
		_comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
	}
	public int Compare(T? a, T? b)
	{
		return _comparer(a, b);
	}
}

