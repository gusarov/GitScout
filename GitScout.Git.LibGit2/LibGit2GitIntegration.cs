using GitScout.Utils;
using LibGit2Sharp;

namespace GitScout.Git.LibGit2;

public class LibGit2GitIntegration : IGitIntegration
{
	Repository _repository;

	public LibGit2GitIntegration(string path)
	{
		_repository = new Repository(path);
	}

	IEnumerable<string> IGitIntegration.GetBranchNames()
	{
		return _repository.Branches.Select(x => x.FriendlyName);
	}

	string IGitIntegration.GetActiveBranchName()
	{
		return _repository.Head.FriendlyName;
	}

	IEnumerable<ICommitInfo> IGitIntegration.GetCommits()
	{
		return _repository.Commits.Take(10).Select(x => ObjectExtensions.Get(x, x => new LibGit2CommitInfo(x)));
	}
}

public class LibGit2CommitInfo : ICommitInfo
{
	private readonly Commit _commit;

	public LibGit2CommitInfo(Commit commit)
	{
		_commit = commit;
		// commit.Parents
	}

	public string Message { get => _commit.MessageShort; }

	public string Hash { get => _commit.Sha; }
}


public class LibGit2GitIntegrationFactory : IGitIntegrationFactory
{
	IGitIntegration IGitIntegrationFactory.Open(string path)
	{
		return new LibGit2GitIntegration(path);
	}
}
