using GitScout.Git;
using GitScout.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GitScout.ViewModels
{
	public class CommitNode // to reduce memory, it is not notifieable view model, just a contract
	{
		public virtual string Message { get; init ; }
		public virtual string Author { get; init; }
		public virtual DateTimeOffset AuthorDate { get; init; }
		public virtual IEnumerable<CommitNode> Parents { get; init; }
		public virtual string Hash { get; init; }

		// public virtual IEnumerable<CommitLink> ParentsLinks { get; init; } = new ObservableCollection<CommitLink>();
		public virtual TrackableObservableCollection<CommitNode> Children { get; set; } = new TrackableObservableCollection<CommitNode>();
		public virtual List<CommitNode> BranchChildren { get; set; } = new List<CommitNode>();
		public virtual List<CommitNode> MergeChildren { get; set; } = new List<CommitNode>();
		// public virtual ObservableCollection<CommitLink> ChildrenLinks { get; set; } = new ObservableCollection<CommitLink>();
		public virtual TrackableObservableCollection<CommitsLine> Lines { get; init; } = new TrackableObservableCollection<CommitsLine>();


		public Thickness LineEllipseMargin => new Thickness(LogicalPositionX * 40, 0, 0, 0);
		public int LogicalPositionX { get; set; }
		public int LogicalPositionY { get; set; }
		public int LogicalHeight { get; set; }
		public bool LogicalLeaf { get; set; }

		public override string ToString()
		{
			return GetHashCode() + " " + Message;
		}
	}

	public class CommitsLine
	{
		public CommitsLine(int id)
		{
			Id = id;
		}

		public int Id { get; }

		public override string ToString()
		{
			return Id.ToString();
		}
	}

	/*
	public class CommitLink
	{
		public CommitLink((CommitNode Parent, CommitNode Child) edge)
		{
			Edge = edge;
		}

		public (CommitNode Parent, CommitNode Child) Edge { get; }

		public CommitNode Child => Edge.Child;
		public CommitNode Parent => Edge.Parent;
	}
	*/

	public class ActualCommitNode : CommitNode
	{
		private readonly ICommitInfo _commitInfo;

		public ActualCommitNode(ICommitInfo commitInfo)
		{
			_commitInfo = commitInfo;
		}
		public override string Message { get => _commitInfo.Message; }
		public override string Author { get => _commitInfo.Author; }
		public override DateTimeOffset AuthorDate { get => _commitInfo.AuthorDate; }
		public override string Hash { get => _commitInfo.Hash; }
		public override IEnumerable<CommitNode> Parents { get => _commitInfo.Parents.Select(x => ObjectExtensions.Get(x, x => new ActualCommitNode(x))); }
	}

}
