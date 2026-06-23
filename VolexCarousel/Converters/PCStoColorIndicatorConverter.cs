using System;
using System.Drawing;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace VolexCarousel.Converters
{
    internal class PCStoColorIndicatorConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            bool isGreen = value is int pcsValue && pcsValue >= 2000;
            bool isYellow = value is int pcsValue2 && pcsValue2 < 2000 && pcsValue2 >= 1000;
            bool isRed = value is int pcsValue3 && pcsValue3 < 1000;
            IBrush color = isGreen ? Brushes.LimeGreen : isYellow ? Brushes.Yellow : isRed ? Brushes.IndianRed : Brushes.White;
            return color;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return new Avalonia.Data.BindingNotification(value);
        }
    }
}
