using System;
using System.Globalization;
using System.Windows.Data;

namespace PoltavaPromTehGaz.Converters
{
	public class UkrainianMonthConverter : IValueConverter
	{
		private readonly CultureInfo ukrainianCulture = new CultureInfo("uk-UA");

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is DateTime date)
			{
				return date.ToString("MMMM yyyy", ukrainianCulture);
			}
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}