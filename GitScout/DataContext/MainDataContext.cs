using GitScout.Commands;
using GitScout.Settings;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GitScout.DataContext;

internal class ViewModel : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler? PropertyChanged;

	protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}

internal class MainDataContext : ViewModel
{
	private readonly ISettings _settings;
	private RepoInfo? currentRepo;
	CommonOpenFileDialog _fbd;

	public MainDataContext(ISettings settings)
	{
		ReposList = settings.Get<ReposList>();
		if (ReposList.Repos.Count > 0)
		{
			CurrentRepo = ReposList.Repos[0];
		}

		_settings = settings;
	}
	
	public ReposList ReposList { get; set; }
	public RepoInfo? CurrentRepo
	{
		get => currentRepo;
		set
		{
			currentRepo = value;
			OnPropertyChanged();
		}
	}

	public ICommand OpenRepoCommand
	{
		get
		{
			return new DelegatedCommand(delegate
			{
				_fbd ??= new CommonOpenFileDialog
				{
					IsFolderPicker = true,
				};
				if (_fbd.ShowDialog() == CommonFileDialogResult.Ok && Directory.Exists(_fbd.FileName))
				{
					var newPath = _fbd.FileName;
					var existing = ReposList.Repos.FirstOrDefault(x => string.Equals(x.Path, newPath, StringComparison.InvariantCultureIgnoreCase));
					if (existing != null)
					{
						CurrentRepo = existing;
					}
					else
					{
						ReposList.Repos.Add(CurrentRepo = new RepoInfo
						{
							Path = newPath,
						});
						_settings.Save(ReposList);
					}
				}
			});
		}
	}
}

internal class DataContextProxy : Freezable
{
	protected override Freezable CreateInstanceCore()
	{
		return new DataContextProxy();
	}

	public object Data
	{
		get { return (object)GetValue(DataProperty); }
		set { SetValue(DataProperty, value); }
	}

	public static readonly DependencyProperty DataProperty =
		DependencyProperty.Register("Data", typeof(object), typeof(DataContextProxy), new UIPropertyMetadata(null));
}
