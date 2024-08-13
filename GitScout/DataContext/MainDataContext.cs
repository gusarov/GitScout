﻿using GitScout.Commands;
using GitScout.Settings;
using GitScout.Utils;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
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

class ViewModels
{
	public static TViewModel OpenVm<TModel, TViewModel>(TModel model)
		where TModel : class
		where TViewModel : class, new()
	{
		return ViewModels<TModel, TViewModel>.Instance.GetViewModel(model, model => new TViewModel());
	}

	public static TViewModel OpenVm<TModel, TViewModel>(TModel model, Func<TModel, TViewModel> factory)
		where TModel : class
		where TViewModel : class
	{
		return ViewModels<TModel, TViewModel>.Instance.GetViewModel(model, factory);
	}

	public static RepositoryScopedDataContext OpenVm(RepoInfo repo)
	{
		return OpenVm(repo, repo => new RepositoryScopedDataContext(repo));
	}
}

class ViewModels<TModel, TViewModel>
	where TModel : class
	where TViewModel : class
{
	public static ViewModels<TModel, TViewModel> Instance = new ViewModels<TModel, TViewModel>();

	private ViewModels()
	{
	}

	WeakKeyDictionary<TModel, TViewModel> _viewModels = new WeakKeyDictionary<TModel, TViewModel>();

	public TViewModel GetViewModel(TModel model, Func<TModel, TViewModel> factory)
	{
		return _viewModels.GetOrAdd(model, factory)!;
	}
}

internal class MainDataContext : ViewModel
{
	private readonly ISettings _settings;
	private RepoInfo? currentRepo;
	CommonOpenFileDialog? _fbd;

	public MainDataContext(ISettings settings)
	{
		ReposList = settings.Get<ReposList>();
		var lastRepoInfo = ReposList.Repos.FirstOrDefault(x => string.Equals(x.Path, ReposList.LastRepo, StringComparison.OrdinalIgnoreCase));
		if (lastRepoInfo != null)
		{
			CurrentRepo = lastRepoInfo;
		}
		else if (ReposList.Repos.Count > 0)
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
			OnPropertyChanged(nameof(CurrentRepoVm));
		}
	}

	public RepositoryScopedDataContext? CurrentRepoVm
	{
		get => CurrentRepo != null
			? ViewModels.OpenVm(CurrentRepo)
			: null;
	}

	public ICommand OpenRepoCommand
	{
		get
		{
			return new DelegatedCommand(_ =>
			{
				_fbd ??= new CommonOpenFileDialog
				{
					IsFolderPicker = true,
				};
			retry:
				if (_fbd.ShowDialog(UiServiceLocator.Instance.MainWindow) == CommonFileDialogResult.Ok && Directory.Exists(_fbd.FileName))
				{
					var newPath = _fbd.FileName;
					var existing = ReposList.Repos.FirstOrDefault(x => string.Equals(x.Path, newPath, StringComparison.InvariantCultureIgnoreCase));
					if (existing != null)
					{
						CurrentRepo = existing;
						ReposList.LastRepo = newPath;
						_settings.Save(ReposList);
					}
					else
					{
						var repoInfo = new RepoInfo(newPath);
						if (repoInfo.Git is ErrorGitIntegration ex)
						{
							MessageBox.Show(UiServiceLocator.Instance.MainWindow
								, $"This is not a git repo.{Environment.NewLine}{ex.Ex.Message}"
								, $"{HumanReadable.Instance.GetHumanReadableExceptionType(ex.Ex)}"
								, MessageBoxButton.OK
								, MessageBoxImage.Error);
							goto retry;
						}
						else
						{
							ReposList.Repos.Add(CurrentRepo = repoInfo);
							ReposList.LastRepo = newPath;
							_settings.Save(ReposList);
						}
					}
				}
			});
		}
	}
	public ICommand OpenExistingRepoCommand
	{
		get
		{
			return new DelegatedCommand(repoInfo =>
			{
				CurrentRepo = (RepoInfo?)repoInfo;
				ReposList.LastRepo = CurrentRepo?.Path;
				_settings.Save(ReposList);
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
