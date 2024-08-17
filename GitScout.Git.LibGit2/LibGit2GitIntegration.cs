﻿using GitScout.Utils;
using LibGit2Sharp;
using System.Runtime.InteropServices;

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
		var nodes = new Dictionary<string, Commit>();
		var allCommits = _repository.Commits.QueryBy(new CommitFilter
		{
			SortBy = CommitSortStrategies.Topological | CommitSortStrategies.Time,
			IncludeReachableFrom = _repository.Refs,
		});
		return allCommits.Select(x => x.GetLibGit2CommitInfo(nodes));
	}
}

public class LibGit2CommitInfo : ICommitInfo
{
	private readonly Commit _commit;

	public LibGit2CommitInfo(Commit commit, Dictionary<string, Commit> set)
	{
		_commit = commit;
		_commitsSet = set;
	}

	public string Message { get => _commit.MessageShort; }

	public string Hash { get => _commit.Sha; }

	public string Author { get => _commit.Author.Name; }
	public DateTimeOffset AuthorDate { get => _commit.Author.When; }

	public string Committer { get => _commit.Committer.Name; }
	public DateTimeOffset CommitterDate { get => _commit.Committer.When; }

	public IEnumerable<ICommitInfo> Parents => _commit.Parents.Select(x => x.GetLibGit2CommitInfo(_commitsSet));

	Dictionary<string, Commit> _commitsSet;
}

public static class LibGit2Extensions
{
	public static LibGit2CommitInfo GetLibGit2CommitInfo(this Commit commit, Dictionary<string, Commit> set)
	{
		ref var knownCommit = ref CollectionsMarshal.GetValueRefOrAddDefault(set, commit.Sha, out _);
		if (knownCommit == null)
		{
			knownCommit = commit;
		}
		var known = knownCommit;
		return ObjectExtensions.Get(commit, x => ObjectExtensions<Commit, LibGit2CommitInfo>.Instance.Get(known, k => new LibGit2CommitInfo(k, set)));
	}
}


public class LibGit2GitIntegrationFactory : IGitIntegrationFactory
{
	IGitIntegration IGitIntegrationFactory.Open(string path)
	{
		return new LibGit2GitIntegration(path);
	}
}
