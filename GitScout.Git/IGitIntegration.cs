using System.Collections;

namespace GitScout.Git;

public interface IGitIntegration
{
	IEnumerable<string> GetBranchNames();
	string GetActiveBranchName();
	IEnumerable<ICommitInfo> GetCommits();
}

public interface IGitIntegrationFactory
{
	IGitIntegration Open(string path);
}

public interface ICommitInfo
{
	string Hash { get; }
	string Message { get; }
	string Author { get; }
	DateTimeOffset AuthorDate { get; }
	string Committer { get; }
	DateTimeOffset CommitterDate { get; }
	IEnumerable<ICommitInfo> Parents { get; }
}