using System.Windows.Controls;
using GitScout.DataContext;

namespace GitScout.Controls;

public class TrackableTextBlock : TextBlock
{
	public TrackableTextBlock()
	{
		ObjectCountTracker.Instance.RegisterConstruction(this);
	}

#if DEBUG
	~TrackableTextBlock()
	{
		ObjectCountTracker.Instance.RegisterDestruction(this);
	}
#endif
}
