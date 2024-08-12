using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace GitScout.Converters;

public class NullToVisibilityConverter : IValueConverter
{
	public NullToVisibilityConverter()
	{

	}

	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		var isNull = ReferenceEquals(null, value);
		if ((parameter is string par) && par.Contains("i", StringComparison.Ordinal))
		{
			isNull = !isNull;
		}
		return isNull ? Visibility.Visible : Visibility.Collapsed;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
