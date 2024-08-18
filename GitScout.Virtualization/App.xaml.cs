using System.Configuration;
using System.Data;
using System.Windows;

namespace GitScout.Virtualization;
/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
	public App()
	{
		// Force GC
		ThreadPool.QueueUserWorkItem(delegate
		{
			while (true)
			{
				GC.Collect();
				GC.WaitForFullGCApproach();
				Thread.Sleep(1000);
			}
		});
	}
}

