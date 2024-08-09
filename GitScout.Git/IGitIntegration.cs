namespace GitScout.Git;

public interface IGitIntegration
{
	public IEnumerable<string> GetBranchNames();
}
