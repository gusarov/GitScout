using System.Collections;

namespace GitScout.Git;

public interface IGitIntegration
{
	public IEnumerable<string> GetBranchNames();
	public string GetActiveBranchName();
	public IEnumerable<ICommitInfo> GetCommits();
}

public interface IGitIntegrationFactory
{
	public IGitIntegration Open(string path);
}

public interface ICommitInfo
{
	public string Hash { get; }
}