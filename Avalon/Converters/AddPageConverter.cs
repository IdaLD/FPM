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
            Debug.WriteLine("Converting");

            AvaloniaList<string> NewList = new AvaloniaList<string>();

            AvaloniaList<int> OldList = value as AvaloniaList<int>;

            foreach (int nr in OldList)
            {
                string text = "Page nr: " + nr;
                NewList.Add(text);
                Debug.WriteLine(text);
            }

            return NewList;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
