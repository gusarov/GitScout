using GitScout.Controls;
using GitScout.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GitScout.Views
{
	/// <summary>
	/// Interaction logic for CommitsListGraph.xaml
	/// </summary>
	public partial class CommitsListGraph : UserControl
	{
		public CommitsListGraph()
		{
			InitializeComponent();
		}

		CustomGraphVirtualizingStackPanel _itemsPanel;
		CustomGraphVirtualizingStackPanel ItemsPanel
		{
			get
			{
				if (_itemsPanel == null)
				{
					_itemsPanel = _listView.FindChild<CustomGraphVirtualizingStackPanel>();
				}
				return _itemsPanel;
			}
		}

		private void listView_ScrollChanged(object sender, ScrollChangedEventArgs e)
		{
			Trace.WriteLine("listView_ScrollChanged " + Guid.NewGuid().ToString());
			UpdateLines();
		}

		private void listView_LayoutUpdated(object sender, EventArgs e)
		{
			return;
			Trace.WriteLine("listView_LayoutUpdated " + Guid.NewGuid().ToString());
			UpdateLines();
		}

		public void UpdateLines()
		{
			// TODO optimize & cache line creation
			_overlayCanvas.Children.Clear();

			var childrenPresenters = ItemsPanel.Children.OfType<ContentControl>().ToArray(); // for some item template it is ItemPresented, for others it is ContentControl
			var byContent = childrenPresenters.ToDictionary(x => (CommitNode)x.Content);
			foreach (var childPresenter in childrenPresenters)
			{
				if (childPresenter.Content is CommitNode node)
				{
					var branchEllipse = childPresenter.FindChild<Ellipse>();
					var start = branchEllipse.TranslatePoint(new Point(branchEllipse.ActualWidth / 2, branchEllipse.ActualHeight / 2), _listView);
					// drawingContext.DrawEllipse(Brushes.Gray, new Pen(Brushes.Blue, 2), start, 8, 8);
					foreach (var parent in node.Parents)
					{
						if (byContent.TryGetValue(parent, out var parentPresenter))
						{
							var parentBranchEllipse = parentPresenter.FindChild<Ellipse>();
							var end = parentBranchEllipse.TranslatePoint(new Point(parentBranchEllipse.ActualWidth / 2, parentBranchEllipse.ActualHeight / 2), _listView);
							// drawingContext.DrawLine(new Pen(Brushes.Gray, 2), start, end);
							_overlayCanvas.Children.Add(new Line
							{
								X1 = start.X,
								Y1 = start.Y,
								X2 = end.X,
								Y2 = end.Y,
								Stroke = Brushes.Blue,
								StrokeThickness = 2,
							});
						}
					}
				}
			}
		}
	}
}
