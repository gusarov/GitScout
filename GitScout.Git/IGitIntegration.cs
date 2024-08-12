namespace GitScout.Git;

public interface IGitIntegration
{
	public IEnumerable<string> GetBranchNames();
	public string GetActiveBranchName();
}

public interface IGitIntegrationFactory
{
	public IGitIntegration Open(string path);
}
