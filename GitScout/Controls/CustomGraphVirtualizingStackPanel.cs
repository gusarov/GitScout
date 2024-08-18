using GitScout.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GitScout.Controls
{
	public class CustomGraphVirtualizingStackPanel : VirtualizingStackPanel
	{
		public CustomGraphVirtualizingStackPanel()
		{
			// SetHorizontalOffset(200);
			// SetVerticalOffset(200);
		}
		/*
		protected override Size MeasureOverride(Size availableSize)
		{
			foreach (var child in Children.OfType<UIElement>())
			{
				child.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
			}
			return base.MeasureOverride(availableSize);
		}

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
		*/

		protected override void OnRender(DrawingContext drawingContext)
		{
			base.OnRender(drawingContext);

			// Draw connections

		}

	}

	public static class VisualTreeExtensions
	{
		public static T FindChild<T>(this DependencyObject parent) where T : DependencyObject
		{
			if (parent == null) return null;

			T foundChild = null;
			int childrenCount = VisualTreeHelper.GetChildrenCount(parent);

			for (int i = 0; i < childrenCount; i++)
			{
				var child = VisualTreeHelper.GetChild(parent, i);
				if (child is T)
				{
					foundChild = (T)child;
					break;
				}
				else
				{
					foundChild = FindChild<T>(child);
					if (foundChild != null) break;
				}
			}
			return foundChild;
		}

		public static IEnumerable<DependencyObject> GetAllChild(this DependencyObject parent)
		{
			if (parent == null)
			{
				yield break;
			}

			int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
			for (int i = 0; i < childrenCount; i++)
			{
				var child = VisualTreeHelper.GetChild(parent, i);
				yield return child;
				foreach (var subChild in GetAllChild(child))
				{
					yield return subChild;
				}
			}
		}
	}
}
