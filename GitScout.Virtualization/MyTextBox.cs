using System.Windows.Controls;

namespace GitScout.Virtualization;

public class MyTextBox : TextBox
{
	public MyTextBox()
	{
		lock (MainWindow.MainDataContext)
		{
			Statistics.Instance.TextControlsCount++;
		}
	}

	~MyTextBox()
	{
		lock (MainWindow.MainDataContext)
		{
			Statistics.Instance.TextControlsCount--;
		}
	}
}
