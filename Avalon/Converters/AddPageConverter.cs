using System;
using System.Diagnostics;
using System.Globalization;
using Avalonia.Collections;
using Avalonia.Data.Converters;

namespace Avalon.Converters
{
    public class AddPageConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            Debug.WriteLine("Returnning Convert");
            return value;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            Debug.WriteLine("Convert Back");
            return value;
        }
    }
}
