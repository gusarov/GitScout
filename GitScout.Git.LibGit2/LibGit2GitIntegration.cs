using LibGit2Sharp;

namespace GitScout.Git.LibGit2;

public class LibGit2GitIntegration : IGitIntegration
{
	Repository _repository;

	public LibGit2GitIntegration(string path)
	{
		_repository = new Repository(path);
	}

	public IEnumerable<string> GetBranchNames()
	{
		return _repository.Branches.Select(x => x.FriendlyName);
	}
}
