using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;

namespace GitScout.Converters;
public class CountToVisibilityConverter : IValueConverter
{
	public CountToVisibilityConverter()
	{

	}
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		int count = (int)value;
		return count == 0 ? Visibility.Visible : Visibility.Collapsed;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
