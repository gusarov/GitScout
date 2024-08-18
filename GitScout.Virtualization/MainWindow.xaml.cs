using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GitScout.Virtualization;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
	public static MainDataContext MainDataContext;
	public static MainWindow MainWindowInstance;

	public MainWindow()
	{
		MainWindowInstance = this;
		DataContext = MainDataContext = new MainDataContext();
		ObjectCountTracker.Instance.PropertyChangedNeedDispatching += (s, action) => Dispatcher.BeginInvoke(action);
		InitializeComponent();
	}
}
