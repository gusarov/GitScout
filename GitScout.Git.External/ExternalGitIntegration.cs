#define EXEC_SYNC
namespace GitScout.Git.External;

public class ExternalGitIntegration : IGitIntegration
{
	private readonly string _path;

	public ExternalGitIntegration(string path)
	{
		_path = path;
	}

	IEnumerable<string> IGitIntegration.GetBranchNames()
	{
		var list = Executor.Instance.Execute(_path, "git", $"-c pager.branch=false branch --format %(refname:short)", throwOnErrorStream: false);
		return list;
	}

	string IGitIntegration.GetActiveBranchName()
	{
		var list = Executor.Instance.Execute(_path, "git", $"rev-parse --abbrev-ref HEAD", throwOnErrorStream: false);
		return list.FirstOrDefault() ?? throw new Exception("Unknown response of branch name");
	}
}

public class ExternalGitIntegrationFactory : IGitIntegrationFactory
{
	IGitIntegration IGitIntegrationFactory.Open(string path)
	{
		return new ExternalGitIntegration(path);
	}
}
