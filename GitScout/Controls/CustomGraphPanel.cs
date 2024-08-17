using GitScout.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GitScout.Controls
{
	public class CustomGraphPanel : StackPanel
	{
		/*
		protected override Size MeasureOverride(Size availableSize)
		{
			foreach (var child in Children.OfType<UIElement>())
			{
				child.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
			}
			return base.MeasureOverride(availableSize);
		}
		*/

		protected override Size ArrangeOverride(Size finalSize)
		{
			double lastPosY = 0;
			foreach (var child in Children.OfType<ContentPresenter>())
			{
				if (child.Content is CommitNode node)
				{
					// Assume node.LogicalPosition is pre-calculated
					var x = (node.LogicalPositionX + 1) * 40; // Distance between nodes horizontally
					// var y = node.LogicalPositionY * 50;  // Distance between nodes vertically
					child.Arrange(new Rect(x, lastPosY, child.DesiredSize.Width, child.DesiredSize.Height));
					lastPosY += child.DesiredSize.Height;
				}
			}
			return finalSize;
		}

		protected override void OnRender(DrawingContext drawingContext)
		{
			base.OnRender(drawingContext);

			// Draw connections
			var contentPresenterChildren = Children.OfType<ContentPresenter>().ToArray();
			var byContent = contentPresenterChildren.ToDictionary(x => (CommitNode)x.Content);
			foreach (var child in contentPresenterChildren)
			{
				if (child.Content is CommitNode node)
				{
					var start = child.TranslatePoint(new Point(-20, child.ActualHeight / 2), this);
					drawingContext.DrawEllipse(Brushes.Black, new Pen(Brushes.Blue, 2), start, 8, 8);
					foreach (var parent in node.Parents)
					{
						var parentPresenter = byContent[parent];
						if (parentPresenter != null)
						{
							var end = parentPresenter.TranslatePoint(new Point(-20, parentPresenter.ActualHeight / 2), this);
							drawingContext.DrawLine(new Pen(Brushes.Black, 2), start, end);
						}
					}
				}
			}
		}
	}
}
