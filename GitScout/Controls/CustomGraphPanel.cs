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

		/*
		protected override Size ArrangeOverride(Size finalSize)
		{
			foreach (UIElement child in Children)
			{
				if (child is CommitNodeVisual node)
				{
					// Assume node.LogicalPosition is pre-calculated
					var x = node.LogicalPosition.X * 100; // Distance between nodes horizontally
					var y = node.LogicalPosition.Y * 50;  // Distance between nodes vertically
					child.Arrange(new Rect(x, y, child.DesiredSize.Width, child.DesiredSize.Height));
				}
			}
			return finalSize;
		}
		*/

		/*
		protected override void OnRender(DrawingContext drawingContext)
		{
			base.OnRender(drawingContext);

			// Draw connections
			foreach (UIElement child in Children)
			{
				if (child is CommitNodeVisual node)
				{
					foreach (var parent in node.Parents)
					{
						Point start = new Point(node.LogicalPosition.X * 100 + 20, node.LogicalPosition.Y * 50 + 20);  // Adjust magic numbers for centering
						Point end = new Point(parent.LogicalPosition.X * 100 + 20, parent.LogicalPosition.Y * 50 + 20);
						drawingContext.DrawLine(new Pen(Brushes.Black, 2), start, end);
					}
				}
			}
		}
		*/
	}

	public class CommitNodeVisual : UserControl
	{
		public Point LogicalPosition { get; set; }
		public List<CommitNodeVisual> Parents { get; set; }
	}

}
