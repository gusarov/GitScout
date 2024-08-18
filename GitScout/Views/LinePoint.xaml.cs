using GitScout.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
	/// Interaction logic for LinePoint.xaml
	/// </summary>
	public partial class LinePoint : UserControl
	{
		// public static readonly DependencyProperty XProperty = DependencyProperty.Register("X", typeof(double), typeof(Line), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));
		// public static readonly DependencyProperty YProperty = DependencyProperty.Register("Y", typeof(double), typeof(Line), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));


		public LinePoint(Point start, CommitNode node)
		{
			SetValue(Canvas.LeftProperty, start.X);
			SetValue(Canvas.TopProperty, start.Y);

			// X = start.X;			Y = start.Y;
			InitializeComponent();
		}

		/*

		[TypeConverter(typeof(LengthConverter))]
		public double X
		{
			get
			{
				return (double)GetValue(XProperty);
			}
			set
			{
				SetValue(XProperty, value);
			}
		}

		[TypeConverter(typeof(LengthConverter))]
		public double Y
		{
			get
			{
				return (double)GetValue(YProperty);
			}
			set
			{
				SetValue(YProperty, value);
			}
		}

		*/
	}
}
