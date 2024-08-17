using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace GitScout.Converters;

/// <summary>
/// Visible when not null
/// Collapsed when null
/// i - Inverse (Visible when not null)
/// h - Hide (default)
/// c - Collapse
/// </summary>
public class NullToVisibilityConverter : IValueConverter
{
	public NullToVisibilityConverter()
	{

	}

	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		var exists = !ReferenceEquals(null, value);

		bool i, h, c;
		if (parameter is string par)
		{
			i = par.Contains("i", StringComparison.Ordinal);
			h = par.Contains("h", StringComparison.Ordinal);
			c = par.Contains("h", StringComparison.Ordinal);
		}
		else
		{
			i = false;
			h = false;
			c = false;
		}

		if (c && h)
		{
			throw new Exception("Can set Hide and Collapse simultaniously");
		}

		Visibility invisible;
		if (c)
		{
			invisible = Visibility.Collapsed;
		}
		else
		{
			invisible = Visibility.Hidden; // default
		}

		if (i)
		{
			exists = !exists;
		}
		return exists
			? Visibility.Visible
			: invisible;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
