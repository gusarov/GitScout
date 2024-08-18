using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using System.Configuration;
using System.Data;
using System.Runtime.InteropServices;
using System.Windows;

namespace GitScout;
/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
	/*
	public App()
	{
		var uiSettings = new UISettings();
		uiSettings.ColorValuesChanged += UiSettings_ColorValuesChanged;
	}

	private void UiSettings_ColorValuesChanged(UISettings sender, object args)
	{
		// Ensure you call this on the UI thread
		Dispatcher.Invoke(() => { LoadThemeBasedOnSystemSetting(); });
	}
	*/

	protected override void OnStartup(StartupEventArgs e)
	{
		base.OnStartup(e);
		LoadThemeBasedOnSystemSetting();
	}

	private void LoadThemeBasedOnSystemSetting()
	{
		var darkTheme = IsWindowsThemeDark();
		var themeUri = darkTheme ? "DarkTheme.xaml" : "LightTheme.xaml";
		var dictionary = new ResourceDictionary { Source = new Uri(themeUri, UriKind.Relative) };

		Resources.MergedDictionaries.Add(dictionary);
		Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("CommonDictionary.xaml", UriKind.Relative) });
	}

	static bool? _isDark;

	public static bool IsWindowsThemeDark()
	{
		if (_isDark == null)
		{
#if DEBUG
			_isDark = Random.Shared.Next(2) == 0;
			return _isDark.Value;
#endif
			const string registryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
			const string registryValueName = "AppsUseLightTheme";

			using (var key = Registry.CurrentUser.OpenSubKey(registryKeyPath))
			{
				var registryValueObj = key?.GetValue(registryValueName);
				if (registryValueObj == null)
				{
					return false; // Default to dark theme if the setting is not found
				}

				var registryValue = (int)registryValueObj;
				_isDark = registryValue <= 0; // 0 means dark theme, 1 means light theme
			}
		}
		return _isDark.Value;
	}
}

