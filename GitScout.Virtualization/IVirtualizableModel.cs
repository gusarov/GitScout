using System.Collections;

namespace GitScout.Virtualization;

/// <summary>
/// In order to easely and properly track virtual elements, they have to remember list and index!
/// </summary>
public interface IVirtualizableModel
{
	IList Owner { get; }
	int Index { get; }
}
