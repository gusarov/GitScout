#define EXEC_SYNC

using GitScout.Git.External;
using GitScout.Git.LibGit2;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
using Newtonsoft.Json.Bson;

namespace GitScout.Git.Tests;

[TestClass]
public class AsmInit
{
	public static string ConsoleWriterPath;

	[AssemblyInitialize]
	public static void AssemblyInit(TestContext context)
	{
		ConsoleWriterPath = Path.Combine(Path.GetTempPath(), "GitScoutTests", "ConsoleWriter");
		if (!Directory.Exists(ConsoleWriterPath))
		{
			Directory.CreateDirectory(ConsoleWriterPath);
		}
		Executor.Instance.Execute("../../../../GitScout.Tests.ConsoleWriter", "dotnet", $"publish --output \"{ConsoleWriterPath}\"", throwOnErrorStream: false);
		ConsoleWriterPath = Path.Combine(ConsoleWriterPath, "GitScout.Tests.ConsoleWriter");
	}
}

[TestClass]
public class GitIntegrationExternalTests : GitIntegrationTests
{
	public override void TestInit()
	{
		_sut = new ExternalGitIntegration(_repo.RepoPath);
	}
}

[TestClass]
public class GitIntegrationLibGit2Tests : GitIntegrationTests
{
	public override void TestInit()
	{
		_sut = new LibGit2GitIntegration(_repo.RepoPath);
	}
}

[TestClass]
public abstract class GitIntegrationTests
{
	protected IGitIntegration _sut;
	protected TestGitRepo? _repo;

	public abstract void TestInit();

	[TestInitialize]
	public void TestInitialize()
	{
		_repo = TestGitRepoFactory.Instance.GetRepo();
		TestInit();
	}

	[TestMethod]
	public void Should_10_retrieve_branch_names()
	{
		var repo = TestGitRepoFactory.Instance.GetRepo();

		repo.EnsureBranch("branch1");
		repo.EnsureBranch("branch2");

		var branches = _sut.GetBranchNames().ToArray();
		Assert.IsTrue(branches.Any(x => x == "branch1"));
		Assert.IsTrue(branches.Any(x => x == "branch2"));
	}
}

public class TestGitRepoFactory
{
	public static TestGitRepoFactory Instance = new TestGitRepoFactory();

	private Dictionary<string, TestGitRepo> _testGitRepo = new Dictionary<string, TestGitRepo>();

	public TestGitRepo GetRepo(string repoCode = "default")
	{
		if (!_testGitRepo.TryGetValue(repoCode, out var testGitRepo))
		{
			_testGitRepo[repoCode] = testGitRepo = new TestGitRepo(repoCode);
		}
		return testGitRepo;
	}
}

public class TestGitRepo
{
	string _path;

	public string RepoPath { get => _path; }

	public TestGitRepo(string code)
	{
		_path = Path.Combine(Path.GetTempPath(), "GitScoutTests", ".TestGitRepo", code);
		Directory.CreateDirectory(_path);
	}

	public void EnsureInit()
	{
		if (!Directory.Exists(Path.Combine(_path, ".git")))
		{
			Executor.Instance.Execute(_path, "git", "init");
			File.WriteAllText(Path.Combine(_path, ".gitignore"), "obj/");
			Executor.Instance.Execute(_path, "git", "add .");
			Executor.Instance.Execute(_path, "git", "commit -m \"First Commit\"");
		}
	}

	public void EnsureBranch(string branchName)
	{
		EnsureInit();
		var list = Executor.Instance.Execute(_path, "git", $"branch", throwOnErrorStream: false);
		if (!list.Any(x => x.Trim() == branchName))
		{
			Executor.Instance.Execute(_path, "git", $"branch {branchName}");
		}
	}
}
