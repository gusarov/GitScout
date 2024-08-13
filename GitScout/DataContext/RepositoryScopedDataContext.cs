using GitScout.Settings;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
			_ = SyncronizeAsync();
		}

		public ObservableCollection<BranchViewModel> Branches { get; } = new ObservableCollection<BranchViewModel>();

		async Task SyncronizeAsync()
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


			await Branches.SyncConsumeSet(rootBranches);
			var queue = new Queue<BranchFolderViewModel>(Branches.OfType<BranchFolderViewModel>());
			while (queue.Count > 0)
			{
				var item = queue.Dequeue();
				await item.Children.SyncConsumeSet(dicNewChildrens[item]);
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
