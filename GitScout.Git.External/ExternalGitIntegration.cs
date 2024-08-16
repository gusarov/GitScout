#define EXEC_SYNC
using static GitScout.Git.External.Executor;

namespace GitScout.Git.External;

public class ExternalGitIntegration : IGitIntegration
{
	[Serializable]
	public class NotAGitRepoException : Exception
	{
		public NotAGitRepoException() { }
		public NotAGitRepoException(string message) : base(message) { }
		public NotAGitRepoException(string message, Exception inner) : base(message, inner) { }
		protected NotAGitRepoException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}

	private readonly string _path;

	public ExternalGitIntegration(string path)
	{
		_path = path ?? throw new ArgumentNullException(nameof(path));
		try
		{
			var topLevel = Executor.Instance.Execute(_path, "git", $"rev-parse --show-toplevel", throwOnErrorStream: false).FirstOrDefault();
			if (topLevel == null || Path.GetFullPath(topLevel.ToLowerInvariant()) != Path.GetFullPath(path.ToLowerInvariant()))
			{
				throw new NotAGitRepoException($"Path '{path}' is not a valid git repository");
			}
		}
		catch (CommandExecutionException)
		{
			throw;
		}
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

	public IEnumerable<ICommitInfo> GetCommits()
	{
		throw new NotImplementedException();
	}
}

public class ExternalGitIntegrationFactory : IGitIntegrationFactory
{
	IGitIntegration IGitIntegrationFactory.Open(string path)
	{
		return new ExternalGitIntegration(path);
	}
}
