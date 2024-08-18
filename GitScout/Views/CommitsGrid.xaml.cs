using GitScout.Controls;
using GitScout.Settings;
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

		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);
			UpdateCanvasOverlayWidth();
		}

		int c = 0;
		void UpdateCanvasOverlayWidth()
		{
			Trace.WriteLine("UpdateCanvasOverlayWidth " + ++c);
			var aw = _gridView.Columns[0].ActualWidth;
			if (aw <= 0)
			{
				Dispatcher.BeginInvoke(() =>
				{
					UpdateCanvasOverlayWidth();
				});
			}
			else
			{
				UiServiceLocator.Instance.MainDataContext.CanvasOverlayWidth = aw;
			}
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
					// var t = VisualTreeHelper.GetContentBounds()
					// var start = childPresenter.TranslatePoint(new Point(20 + 40 * node.LogicalPositionX, childPresenter.ActualHeight / 2), _listView);
					var branchEllipse = childPresenter.FindChild<Ellipse>();
					var start = branchEllipse.TranslatePoint(new Point(branchEllipse.ActualWidth / 2, branchEllipse.ActualHeight / 2), _listView);
					// drawingContext.DrawEllipse(Brushes.Gray, new Pen(Brushes.Blue, 2), start, 8, 8);
					foreach (var parent in node.Parents)
					{
						if (byContent.TryGetValue(parent, out var parentPresenter))
						{
							var parentBranchEllipse = parentPresenter.FindChild<Ellipse>();
							var end = parentBranchEllipse.TranslatePoint(new Point(parentBranchEllipse.ActualWidth / 2, parentBranchEllipse.ActualHeight / 2), _listView);
							// var end = parentPresenter.TranslatePoint(new Point(20 + 40 * parent.LogicalPositionX, parentPresenter.ActualHeight / 2), _listView);
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
						else
						{
							// we don't see parent due to virtualization, but we know it exists!
							// So, let's draw a line disappearing to the bottom
							_overlayCanvas.Children.Add(new Line
							{
								X1 = start.X,
								Y1 = start.Y,
								X2 = start.X,
								Y2 = Math.Min(start.Y + 300, _overlayCanvas.ActualHeight),
								Stroke = new LinearGradientBrush(new GradientStopCollection
								{
									new GradientStop(Color.FromArgb(255, Colors.Blue.R, Colors.Blue.G, Colors.Blue.B), 0),
									new GradientStop(Color.FromArgb(32 , Colors.Blue.R, Colors.Blue.G, Colors.Blue.B), 1),
								}, 90),
								StrokeThickness = 2,
							});
						}
					}
					// _overlayCanvas.Children.Add(new LinePoint(start, node));
				}
			}
		}
	}
}
