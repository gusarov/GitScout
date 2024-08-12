using GitScout.DataContext;
using GitScout.Git;
using GitScout.Git.LibGit2;
using GitScout.Settings;
using GitScout.Settings.Implementation;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GitScout;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
	public MainWindow()
	{
		InitializeComponent();
		
		var sc = new ServiceCollection();
		sc.AddSingleton<ISettings, SettingsImplementation>();
		sc.AddSingleton<IGitIntegrationFactory, LibGit2GitIntegrationFactory>();
		sc.AddSingleton<MainDataContext>();

		var sp = sc.BuildServiceProvider();

		UiServiceLocator.Instance.GitIntegrationFactory = sp.GetRequiredService<IGitIntegrationFactory>();
		DataContext = sp.GetRequiredService<MainDataContext>();
	}

	private void MenuItem_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
	{

	}
}