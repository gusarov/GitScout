using GitScout.Git;
using GitScout.Settings;
using GitScout.Utils;
using GitScout.ViewModels;
using LibGit2Sharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GitScout.DataContext
{
	internal class RepositoryScopedDataContext : ViewModel
	{
		private readonly RepoInfo _repoInfo;
		ConcurrentDictionary<string, BranchViewModel> _fullBranchPathViewModels = new ConcurrentDictionary<string, BranchViewModel>();
		ConcurrentDictionary<string, BranchFolderViewModel> _fullFolderPathViewModels = new ConcurrentDictionary<string, BranchFolderViewModel>();
		// ConcurrentBag<BranchViewModel> _rootBranchViewModels = new ConcurrentBag<BranchViewModel>();
		// ConcurrentBag<BranchFolderViewModel> _rootBranchFoldersViewModels = new ConcurrentBag<BranchFolderViewModel>();

		public RepositoryScopedDataContext(RepoInfo repoInfo)
		{
			_repoInfo = repoInfo;
			_ = SyncronizeBranchesAsync();

			// var c1 = new CommitNode() { Hash = "e124f76afd86bd87f8818f039697432c062e705d", AuthorDate = DateTime.UtcNow.AddHours(-2), Author = "dmitry.guarov@gmail.com", Message = "Test3" };
			// var c2 = new CommitNode() { Hash = "e124f76afd86bd87f8818f039697432c062e705e", AuthorDate = DateTime.UtcNow.AddHours(-1), Author = "dmitry.guarov@gmail.com", Message = "Test2", Parents = [c1] };
			// var c3 = new CommitNode() { Hash = "e124f76afd86bd87f8818f039697432c062e705f", AuthorDate = DateTime.UtcNow.AddHours(-0), Author = "dmitry.guarov@gmail.com", Message = "Test1", Parents = [c2] };

			foreach (var item in repoInfo.Git.GetCommits())
			{
				Commits.Add(ObjectExtensions.Get(item, item => new ActualCommitNode(item)));
			}
			new CommitsAnalyzer().AnalyzeAsync(Commits).Wait();
			/*


			int rowIndex = 0;
			// var branchLevels = new Dictionary<string, int>();

			foreach (var commit in Commits.Reverse())
			{
				commit.LogicalPosition = new Point(rowIndex, DetermineBranchLevel(commit, branchLevels));
				rowIndex++;
			}
			*/
		}
		/*

		private int DetermineBranchLevel(CommitNode commit, Dictionary<string, int> branchLevels)
		{
			if (commit.Parents.Count() == 0)
			{
				// Root commit, typically the initial commit on mainline
				return 0;
			}

			int level = 0;
			if (commit.Parents.Count() == 1)
			{
				// Normal single-parent commit, continue on the same branch
				var parent = commit.Parents.Single();
				level = branchLevels.ContainsKey(parent.Hash) ? branchLevels[parent.Hash] : 0;
			}
			else
			{
				// Merge commit, determine the lowest level of the parents to continue
				level = commit.Parents.Min(parent => branchLevels.ContainsKey(parent.Hash) ? branchLevels[parent.Hash] : 0);
			}

			// If this commit starts a new branch, increment the level
			if (IsNewBranch(commit))
			{
				level++;
			}

			// Store or update the current level of this commit's branch
			branchLevels[commit.CommitId] = level;

			return level;
		}
		private bool IsNewBranch(CommitNodeVisual commit)
		{
			// Determine if this commit is the start of a new branch
			// This is a simplification, you'll need to adjust this based on actual Git data and how you track branches
			return commit.Parents.Count > 1 || (commit.Parents.Count == 1 && commit.Message.Contains("branch"));
		}
		*/

		public ObservableCollection<BranchViewModel> Branches { get; } = new ObservableCollection<BranchViewModel>();
		public ObservableCollection<CommitNode> Commits { get; } = new ObservableCollection<CommitNode>();

		async Task SyncronizeBranchesAsync()
		{
			await Task.Yield();
			var branchPaths = _repoInfo.Git.GetBranchNames();
			var rootBranches = new HashSet<BranchViewModel>();
			var dicNewChildrens = new Dictionary<BranchFolderViewModel, HashSet<BranchViewModel>>();
			foreach (var item in branchPaths)
			{
				var parts = item.Split('/');
				var parents = new List<string>(4);
				for (int i = 0; i < parts.Length; i++)
				{
					var isLeaf = i == parts.Length - 1;
					var part = parts[i];
					var parentPath = string.Join("/", parents) + "/";
					var parentFolderVm = parentPath == "/" ? null : _fullFolderPathViewModels[parentPath];
					BranchViewModel vm;
					if (isLeaf)
					{
						var fullPath = part;
						// when we are parsing git output, our leafs are always branches, never folders
						vm = _fullBranchPathViewModels.GetOrAdd(fullPath, _ => new BranchLeafViewModel(parentFolderVm, fullPath, part));
					}
					else
					{
						var fullPath = part + "/";
						vm = _fullFolderPathViewModels.GetOrAdd(fullPath, _ => new BranchFolderViewModel(parentFolderVm, fullPath, part + "/"));
						parents.Add(part);
					}
					if (i == 0)
					{
						rootBranches.Add(vm);
					}
					else if (parentFolderVm != null)
					{
						if (!dicNewChildrens.TryGetValue(parentFolderVm, out var newChildren))
						{
							dicNewChildrens[parentFolderVm] = newChildren = new HashSet<BranchViewModel>();
						}
						newChildren.Add(vm);
					}
				}
			}


			await Branches.UpdateConsumeSetAsync(rootBranches);
			var queue = new Queue<BranchFolderViewModel>(Branches.OfType<BranchFolderViewModel>());
			while (queue.Count > 0)
			{
				var item = queue.Dequeue();
				await item.Children.UpdateConsumeSetAsync(dicNewChildrens[item]);
				foreach (var subFolder in item.Children.OfType<BranchFolderViewModel>())
				{
					queue.Enqueue(subFolder);
				}
			}
		}

	}

	abstract class BranchViewModel : ViewModel
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="parentBranchFolder">e.g. null</param>
		/// <param name="branchPath">e.g. release/v2025.0.0</param>
		/// <param name="branchName">v2025.0.0</param>
		/// <exception cref="ArgumentNullException"></exception>
		public BranchViewModel(BranchFolderViewModel? parentBranchFolder, string branchPath, string branchName)
		{
			ParentBranchFolder = parentBranchFolder;
			BranchPath = branchPath ?? throw new ArgumentNullException(nameof(branchPath));
			BranchName = branchName ?? throw new ArgumentNullException(nameof(branchName));
		}

		public BranchFolderViewModel? ParentBranchFolder { get; }
		public string BranchPath { get; }
		public string BranchName { get; }

		public override string ToString()
		{
			return BranchPath;
		}
	}

	class BranchLeafViewModel : BranchViewModel
	{
		public BranchLeafViewModel(BranchFolderViewModel? parentBranchFolder, string branchPath, string branchLeafName)
			: base(parentBranchFolder, branchPath, branchLeafName)
		{
		}

		public override string ToString()
		{
			return $"{base.ToString()} (Leaf)";
		}
	}

	class BranchFolderViewModel : BranchViewModel
	{		public BranchFolderViewModel(BranchFolderViewModel? parentBranchFolder, string branchPath, string branchFolderName)
			: base(parentBranchFolder, branchPath, branchFolderName)
		{
		}

		public ObservableCollection<BranchViewModel> Children { get; } = new ObservableCollection<BranchViewModel>();

		public override string ToString()
		{
			return $"{base.ToString()} (Folder)";
		}
	}
}
