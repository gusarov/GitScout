using GitScout.DataContext;
using GitScout.Git;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace GitScout.Settings;

internal class ReposList
{
	public ObservableCollection<RepoInfo> Repos { get; set; } = new ObservableCollection<RepoInfo>();
	public string? LastRepo { get; set; }
}

internal class RepoInfo
{
	public string Path { get; set; }

	public string Short => System.IO.Path.GetFileName(Path);

	Lazy<IGitIntegration> _git;
	public IGitIntegration Git => _git.Value;

	public RepoInfo(string path)
	{
		Path = path ?? throw new ArgumentNullException(nameof(path));
		_git = new Lazy<IGitIntegration>(() =>
		{
			try
			{
				return UiServiceLocator.Instance.GitIntegrationFactory.Open(Path);
			}
			catch (Exception ex)
			{
				return new ErrorGitIntegration(ex);
			}
		});
	}

	public string CurrentBranch => Git.GetActiveBranchName();
}

class ErrorGitIntegration : IGitIntegration
{
	public ErrorGitIntegration(Exception ex)
	{
		Ex = ex;
	}
	public Exception Ex { get; }

	string IGitIntegration.GetActiveBranchName()
	{
		return "Bad Repo";
	}

	IEnumerable<string> IGitIntegration.GetBranchNames()
	{
		return ["Bad Repo"];
	}

	IEnumerable<ICommitInfo> IGitIntegration.GetCommits()
	{
		throw new NotImplementedException();
	}
}

internal class UiServiceLocator
{
	public static UiServiceLocator Instance = new UiServiceLocator();

    private UiServiceLocator()
    {
			
    }

    public IGitIntegrationFactory GitIntegrationFactory;

	public MainWindow MainWindow;
	public Dispatcher Dispatcher;
	public MainDataContext MainDataContext;
}