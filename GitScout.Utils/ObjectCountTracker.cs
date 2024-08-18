using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

namespace GitScout;

public class ObjectCountTracker : INotifyPropertyChanged
{
	public static readonly ObjectCountTracker Instance = new ObjectCountTracker();

	private ObjectCountTracker()
	{
#if DEBUG
		// Force GC & periodic notify
		_thread = new Thread(() =>
		{
			while (true)
			{
				GC.Collect();
				GC.WaitForFullGCApproach();
				lock (_counts)
				{
					if (_isDirty)
					{
						_isDirty = false;
						try
						{
							PropertyChangedNeedDispatching?.Invoke(this, () =>
							{
								OnPropertyChanged(nameof(Stats));
							});
						}
						catch (Exception ex)
						{
							Trace.TraceError(ex.ToString()); // exception is anotehr way to shutdown a thread
							break;
						}
					}
				}
				Thread.Sleep(1000);
			}
		})
		{
			Priority = ThreadPriority.BelowNormal,
			IsBackground = true,
		};
		_thread.Start();
#endif
	}

	private Thread _thread;
	private bool _isDirty;
	private Dictionary<string, int> _counts = new();

	public event EventHandler<Action>? PropertyChangedNeedDispatching;
	public event PropertyChangedEventHandler? PropertyChanged;

	private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	[Conditional("DEBUG")]
	public void RegisterConstruction<T>(T instance)
	{
		lock (_counts)
		{
			ref var count = ref CollectionsMarshal.GetValueRefOrAddDefault(_counts, typeof(T).Name, out _);
			count++;
			_isDirty = true;
		}
	}

	[Conditional("DEBUG")]
	public void RegisterDestruction<T>(T instance)
	{
		lock (_counts)
		{
			ref var count = ref CollectionsMarshal.GetValueRefOrAddDefault(_counts, typeof(T).Name, out _);
			count--;
			_isDirty = true;
		}
	}

	public IReadOnlyDictionary<string, int> Stats => _counts.AsReadOnly();
}