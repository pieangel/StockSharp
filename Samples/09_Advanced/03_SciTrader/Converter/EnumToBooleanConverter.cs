﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using StockSharp.BusinessEntities;
namespace SciTrader.Converter
{
	public class EnumToBooleanConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value.Equals(parameter);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (bool)value ? parameter : Binding.DoNothing;
		}
	}

	public class SecurityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is Security security)
				return security.Id; // Convert `Security` to `string` (or another UI-friendly format)

			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			// If you need to convert back from `string` to `Security`, you may need a lookup mechanism
			return null;
		}
	}
}
