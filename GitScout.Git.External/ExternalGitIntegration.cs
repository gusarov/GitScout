#define EXEC_SYNC
namespace GitScout.Git.External;

public class ExternalGitIntegration : IGitIntegration
{
	private readonly string _path;

	public ExternalGitIntegration(string path)
    {
		_path = path;
	}

    public IEnumerable<string> GetBranchNames()
	{
		var list = Executor.Instance.Execute(_path, "git", $"-c pager.branch=false branch --format %(refname:short)", throwOnErrorStream: false);
		return list;
	}
}
