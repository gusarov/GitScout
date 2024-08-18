using System.Collections;

namespace GitScout.Virtualization;

public class MyModel : IVirtualizableModel
{
	private readonly int _key;

	/*
	public MyClass()
	{
			
	}
	*/
	public MyModel(IList owner, int key)
	{
		Owner = owner;
		_key = key;
		ObjectCountTracker.Instance.RegisterConstruction(this);
	}

#if DEBUG
	~MyModel()
	{
		ObjectCountTracker.Instance.RegisterDestruction(this);
	}
#endif

	public string Hash { get; set; } = Guid.NewGuid().ToString();

	public IList Owner { get; }

	public int Index => _key;

	public override string ToString()
	{
		return $"item #{_key:X8} {Hash}";
	}
}
