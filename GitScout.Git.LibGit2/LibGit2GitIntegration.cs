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
}

public class LibGit2GitIntegrationFactory : IGitIntegrationFactory
{
	IGitIntegration IGitIntegrationFactory.Open(string path)
	{
		return new LibGit2GitIntegration(path);
	}
}
