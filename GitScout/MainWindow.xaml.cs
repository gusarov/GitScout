using GitScout.DataContext;
using GitScout.Git;
using GitScout.Git.External;
using GitScout.Git.LibGit2;
using GitScout.Settings;
using GitScout.Settings.Implementation;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
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
	[DllImport("dwmapi.dll")]
	private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

	private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
	private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

	private static bool UseImmersiveDarkMode(IntPtr handle, bool enabled)
	{
		if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17763))
		{
			var attribute = DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1;
			if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 18985))
			{
				attribute = DWMWA_USE_IMMERSIVE_DARK_MODE;
			}

			int useImmersiveDarkMode = enabled ? 1 : 0;
			return DwmSetWindowAttribute(handle, attribute, ref useImmersiveDarkMode, sizeof(int)) == 0;
		}

		return false;
	}

	protected override void OnSourceInitialized(EventArgs e)
	{
		base.OnSourceInitialized(e);
		var isDarkTheme = App.IsWindowsThemeDark();
		HwndSource source = (HwndSource)PresentationSource.FromVisual(this);
		UseImmersiveDarkMode(source.Handle, isDarkTheme);
	}

	public MainWindow()
	{
		InitializeComponent();
		
		var sc = new ServiceCollection();
		sc.AddSingleton<ISettings, SettingsImplementation>();
		// sc.AddSingleton<IGitIntegrationFactory, ExternalGitIntegrationFactory>();
		sc.AddSingleton<IGitIntegrationFactory, LibGit2GitIntegrationFactory>();
		sc.AddSingleton<MainDataContext>();

		var sp = sc.BuildServiceProvider();

		UiServiceLocator.Instance.GitIntegrationFactory = sp.GetRequiredService<IGitIntegrationFactory>();
		UiServiceLocator.Instance.MainWindow = this;
		DataContext = sp.GetRequiredService<MainDataContext>();
	}



	private void MenuItem_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
	{

	}
}