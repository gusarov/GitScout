using GitScout.DataContext;
using GitScout.Utils;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GitScout.ViewModels
{
	class CommitAnalysisContextItem
	{
		public List<CommitNode> Chidlren = new List<CommitNode>();
	}

	public class CommitsAnalyzer
	{
		public int CountOfLines { get; set; }

		public async Task AnalyzeAsync(IEnumerable<CommitNode> commits)
		{
			// build a bidirectional counterparts:
			var commitsList = commits.ToList();
			commits = commitsList;

			var context = new ObjectExtensions<CommitNode, CommitAnalysisContextItem>();
			int row = 0;
			CommitNode last = null;

			// find roots and maintain bidirectional relationships
			var roots = new List<CommitNode>();
			foreach (var commit in commits)
			{
				var parents = commit.Parents;
				bool hasParents = false;
				if (parents != null)
				{
					foreach (var parent in parents)
					{
						// all parents declared in a node should know they have a children
						context.Get(parent).Chidlren.Add(commit);
						hasParents = true;
					}
				}
				if (!hasParents)
				{
					roots.Add(commit);
				}

				commit.LogicalPositionY = ++row; // in unordered mode, the source defines position. For pre-ordered mode (e.g. by author date) order it before analysis!
				last = commit;
			}

			// propagate final children list to view models
			foreach (var commit in commits)
			{
				if (commit.Children.Subscribers > 0)
				{
					await commit.Children.UpdateAsync(context.Get(commit).Chidlren); // await commit.Children.UpdateAsync(context.Get(commit).Chidlren); // for real-time updates				}
				}
				else
				{
					commit.Children = new TrackableObservableCollection<CommitNode>(context.Get(commit).Chidlren); // for faster updates while no bindings
				}
				foreach (var child in commit.Children)
				{
					if (child.Parents.First() == commit)
					{
						commit.BranchChildren.Add(child);
					}
					else
					{
						// commit.MergeChildren.Add(child);
					}
				}
			}

			var activeBranches = new List<CommitNode>();

			foreach (var commit in commits)
			{
				if (commit.BranchChildren.Count > 0)
				{
					// Find and replace the active branch if possible
					var index = activeBranches.FindIndex(b => commit.BranchChildren.Contains(b));
					if (index != -1)
					{
						activeBranches[index] = commit;  // Replace the branch with the new commit
						commit.LogicalPositionX = index;
					}
					else
					{
						// If no active branch to replace, add new
						commit.LogicalPositionX = activeBranches.Count;
						activeBranches.Add(commit);
					}

					// Remove all other branch children from active branches
					foreach (var child in commit.BranchChildren)
					{
						var childIndex = activeBranches.IndexOf(child);
						if (childIndex != -1 && childIndex != index)
						{
							activeBranches.RemoveAt(childIndex);
						}
					}
				}
				else
				{
					// If no branch children, this is a new branch
					commit.LogicalPositionX = activeBranches.Count;
					activeBranches.Add(commit);
				}

				// Set the j-coordinate
				// commit.LogicalPositionX = activeBranches.IndexOf(commit);
			}

#if A
			// now breadth first calculate the depth and leafs. TODO this assumes no unrelated graphs, need to scan for it using visited map
			var leafs = new List<CommitNode>();
			var queue = new Queue<(CommitNode, int, CommitsLine)>();
			var nextLineId = 0;
			foreach (var commit in roots)
			{
				queue.Enqueue((commit, 0, new CommitsLine(++nextLineId)));
			}
			while (queue.Count > 0)
			{
				var (node, depth, line) = queue.Dequeue();


				if (node.Lines.Count > 0)
				{
					// we already visited this node, so the only thing is add a new merge line
					node.Lines.Add(line);
					continue;
				}
				node.Lines.Add(line);

				if (node.Children.Count == 0)
				{
					node.LogicalLeaf = true;
					leafs.Add(node);
				}
				else if (node.Children.Count == 1)
				{
					queue.Enqueue((node.Children[0], depth + 1, line));
				}
				else
				{
					foreach (var item in node.Children)
					{
						queue.Enqueue((item, depth + 1, new CommitsLine(++nextLineId)));
					}
				}

				node.LogicalHeight = depth;
			}

			var linePosition = new Dictionary<CommitsLine, int>();

			foreach (var commit in commits)
			{
				foreach (var line in commit.Lines)
				{
					var pos = linePosition.GetOrAdd(line, _ => linePosition.Count + 1);
					if (commit.LogicalPositionX == 0)
					{
						commit.LogicalPositionX = pos;
					}
				}
			}

			/*

			foreach (var commit in commits)
			{
				commit.LogicalPositionX = commit.Lines.FirstOrDefault()?.Id ?? 0;
			}

			CountOfLines = nextLineId;
			*/

			// order leafs by height
			// leafs.Sort(DelegatedComparer.With<CommitNode>((a, b) => a.LogicalHeight.CompareTo(b.LogicalHeight)));
			var branchPositions = new Dictionary<CommitNode, int>();
			var lastSeenTrack = new Dictionary<CommitNode, int>();
			int currentTrack = 0;

			var trackAssignment = new Dictionary<CommitNode, int>();
			foreach (var commit in commits)
			{
				if (commit.Parents.Count() == 0)
				{
					// Root commit or a disconnected commit starts a new track
					branchPositions[commit] = currentTrack++;
				}
				else if (commit.Parents.Count() == 1)
				{
					var parent = commit.Parents.First();
					// Continue on the same track as the parent unless this commit is a branch point
					if (!branchPositions.ContainsKey(parent))
						branchPositions[parent] = currentTrack; // Ensure parent has a track

					branchPositions[commit] = branchPositions[parent]; // Single parent, continue on the same track

					// If this commit has multiple children, it might be a branching point
					if (commit.Children.Count > 1)
					{
						int baseTrack = branchPositions[commit];
						// Assign a new track to each new branch
						foreach (var child in commit.Children)
						{
							if (child != commit.Children.First()) // First child continues on the same track
								trackAssignment[child] = ++currentTrack;
						}
					}
				}
				else if (commit.Parents.Count() > 1)
				{
					// Merge commit, choose the lowest track among parents
					int minTrack = commit.Parents.Min(p => branchPositions.ContainsKey(p) ? branchPositions[p] : currentTrack);
					branchPositions[commit] = minTrack;
				}

				// Apply new track assignments from the dictionary
				foreach (var pair in trackAssignment)
				{
					branchPositions[pair.Key] = pair.Value;
				}
				trackAssignment.Clear(); // Clear after applying to avoid carrying over to next loop iteration
			}

			int lastY = 0;
			foreach (var commit in commits)
			{
				if (commit.LogicalPositionY <= lastY)
				{
					throw new Exception("unsorted Ys");
				}
				lastY = commit.LogicalPositionY;
				/*
				if (!branchPositions.ContainsKey(commit))
				{
					branchPositions[commit] = 0;  // Default to track 0 if not set
				}
				*/
				commit.LogicalPositionX = branchPositions[commit];

				// If this commit is a branching point, allocate new tracks to each child
				/*
				if (commit.Children.Count > 1)
				{
					int nextTrack = branchPositions[commit];
					foreach (var child in commit.Children)
					{
						branchPositions[child] = ++nextTrack;  // Assign new tracks to each branch
					}
				}
				*/
			}
			
			// Console.WriteLine(	);
#endif
		}

	}
}
