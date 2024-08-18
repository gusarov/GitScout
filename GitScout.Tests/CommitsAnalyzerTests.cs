using GitScout.Git;
using GitScout.ViewModels;
using System.Net.Http.Headers;
using GitScout;

namespace GitScout.Tests;

[TestClass]
public class CommitsAnalyzerTests
{
	CommitsAnalyzer _sut = new CommitsAnalyzer();

	[TestMethod]
	public async Task Should_build_bidirectional_relationships()
	{
		var c1 = new CommitNode() { Hash = @"|   1", Parents = [] };
		var c2 = new CommitNode() { Hash = @"|   2", Parents = [c1] };
		var c3 = new CommitNode() { Hash = @"  \ 3", Parents = [c1] };
		var c4 = new CommitNode() { Hash = @"|   4", Parents = [c2] };

		var commits = new CommitNode[] { c4, c3, c2, c1 };
		await _sut.AnalyzeAsync(commits);

		CollectionAssert.AreEquivalent(new CommitNode[] { c2, c3 }, c1.Children);
		CollectionAssert.AreEquivalent(new CommitNode[] { c4 }, c2.Children);
		CollectionAssert.AreEquivalent(new CommitNode[] { }, c3.Children);
		CollectionAssert.AreEquivalent(new CommitNode[] { }, c4.Children);
	}

	[TestMethod]
	public async Task Should_build_graph_virtual_points()
	{
		var c1 = new CommitNode() { Hash = @"|   1", Parents = [] };
		var c2 = new CommitNode() { Hash = @"|   2", Parents = [c1] };
		var c3 = new CommitNode() { Hash = @"  \ 3", Parents = [c1] };
		var c4 = new CommitNode() { Hash = @"|   4", Parents = [c2] };

		var commits = new CommitNode[] { c1, c2, c3, c4 };
		await _sut.AnalyzeAsync(commits);

		CollectionAssert.AreEquivalent(new CommitNode[] { c2, c3 }, c1.Children);
		CollectionAssert.AreEquivalent(new CommitNode[] { c4 }, c2.Children);
		CollectionAssert.AreEquivalent(new CommitNode[] { }, c3.Children);
		CollectionAssert.AreEquivalent(new CommitNode[] { }, c4.Children);
	}
}