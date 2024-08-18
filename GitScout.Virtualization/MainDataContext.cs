namespace GitScout.Virtualization;

public class MainDataContext 
{
	public bool DeferredScroll => Items.Count > 2000;
	public IntMaxCollection<MyModel> Items { get; } = new IntMaxCollection<MyModel>();
	public ObjectCountTracker Statistics { get; } = ObjectCountTracker.Instance;
}
