using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GitScout.Virtualization;

public class Statistics : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler? PropertyChanged;

	private void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
	}

	public static readonly Statistics Instance = new Statistics();

	private Statistics()
	{
			
	}

	int _textControlsCount;

	public int TextControlsCount
	{
		get
		{
			return _textControlsCount;
		}
		set
		{
			_textControlsCount = value;
			MainWindow.MainWindowInstance.Dispatcher.BeginInvoke(() =>
			{
				OnPropertyChanged(nameof(TextControlsCount));
			});
		}
	}

	int _dataElementsCount;

	public int DataElementsCount
	{
		get
		{
			return _dataElementsCount;
		}
		set
		{
			_dataElementsCount = value;
			MainWindow.MainWindowInstance.Dispatcher.BeginInvoke(() =>
			{
				OnPropertyChanged(nameof(DataElementsCount));
			});
		}
	}
}